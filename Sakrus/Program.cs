using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using QuestPDF.Infrastructure;
using Sakrus.Components;
using Sakrus.Data;
using Sakrus.Infrastructure.Data;
using Sakrus.Services;

// Configuração global do QuestPDF (feita uma única vez, aqui, não dentro dos métodos)
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// --- Blazor e UI ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

// --- Banco de Dados (PostgreSQL) ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Autenticação com Cookie ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/login";
        options.AccessDeniedPath = "/acesso-negado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

// --- Data Protection: persiste chaves entre reinicializações do container ---
var keysPath = new System.IO.DirectoryInfo("/app/dataprotection-keys");
if (!keysPath.Exists) keysPath.Create();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(keysPath)
    .SetApplicationName("Sakrus");

// --- Serviços de Negócio ---
builder.Services.AddScoped<IGavetaPublicaService, GavetaPublicaService>();
builder.Services.AddScoped<IAtendimentoFaturamentoService, AtendimentoFaturamentoService>();
builder.Services.AddScoped<IJazigoService, JazigoService>();
builder.Services.AddScoped<IFalecidoService, FalecidoService>();  // sem duplicata
builder.Services.AddScoped<ICapelaService, CapelaService>();
builder.Services.AddScoped<RelatorioService>();
builder.Services.AddScoped<PdfGeneratorService>();
builder.Services.AddScoped<EstoqueService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<DatabaseSeeder>();

var app = builder.Build();

// --- Pipeline HTTP ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

// A ORDEM IMPORTA: Authentication antes de Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// --- Inicialização do Banco: Migrations + Seed (com retry) ---
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    const int maxRetries = 10;
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation("Tentativa {Attempt}/{Max} de conexão com o banco de dados...", attempt, maxRetries);

            // Aplica migrations pendentes automaticamente (cria o schema se não existir)
            await db.Database.MigrateAsync();

            // Seed do usuário admin padrão
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();

            logger.LogInformation("Banco de dados pronto.");
            break;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // backoff: 2s, 4s, 8s...
            logger.LogWarning("Banco ainda não disponível. Aguardando {Delay}s antes de tentar novamente. Erro: {Message}",
                delay.TotalSeconds, ex.Message);
            await Task.Delay(delay);
        }
    }
}

app.Run();