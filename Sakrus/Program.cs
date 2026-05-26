using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using QuestPDF.Infrastructure;
using Sakrus.Components;
using Sakrus.Endpoints;
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
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Serviços Scoped ainda podem usar o ApplicationDbContext
builder.Services.AddScoped<ApplicationDbContext>(p => 
    p.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

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

builder.Services.AddAuthorization(options =>
{
    // SEC-06: Política padrão: todas as páginas exigem autenticação
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddHttpContextAccessor();

// --- AuthenticationStateProvider para Blazor Server ---
builder.Services.AddScoped<AuthenticationStateProvider, PersistingAuthenticationStateProvider>();

// --- Data Protection: persiste chaves entre reinicializações do container ---
// ARQ-06: Path condicional para funcionar tanto em Docker (Linux) quanto em dev (Windows)
var keysPathStr = builder.Environment.IsProduction()
    ? "/app/dataprotection-keys"
    : Path.Combine(builder.Environment.ContentRootPath, "dataprotection-keys");
var keysPath = new System.IO.DirectoryInfo(keysPathStr);
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
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<PdfGeneratorService>();
builder.Services.AddScoped<EstoqueService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<DatabaseSeeder>();

var app = builder.Build();

// --- Pipeline HTTP ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // HSTS é desativado em containers sem HTTPS configurado
    // Se ativar HTTPS no futuro, descomentar: app.UseHsts();
}

app.UseStaticFiles();

// A ORDEM IMPORTA: Authentication → Authorization → Antiforgery
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Endpoints HTTP para login/logout (cookie auth precisa de request HTTP real)
app.MapAuthEndpoints();

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
