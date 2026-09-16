using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using QuestPDF.Infrastructure;
using Sakrus;
using Sakrus.Components;
using Sakrus.Endpoints;
using Sakrus.Data;
using Sakrus.Infrastructure.Data;
using Sakrus.Services;
using System.Threading.RateLimiting;

// ConfiguraÃ§Ã£o global do QuestPDF (feita uma Ãºnica vez, aqui, nÃ£o dentro dos mÃ©todos)
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// --- Blazor e UI ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

// --- Banco de Dados (PostgreSQL) com Auditoria ---
builder.Services.AddScoped<Sakrus.Core.ICurrentUserService, Sakrus.Services.CurrentUserService>();
builder.Services.AddScoped<AuditInterceptor>();
builder.Services.AddDbContextFactory<ApplicationDbContext>((sp, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
}, ServiceLifetime.Scoped);

// ServiÃ§os Scoped ainda podem usar o ApplicationDbContext
builder.Services.AddScoped<ApplicationDbContext>(p => 
    p.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());

// --- AutenticaÃ§Ã£o com Cookie ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/login";
        options.AccessDeniedPath = "/acesso-negado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        // SEC-01: Política de segurança do cookie configurável via env var.
        // Por padrão usa SameAsRequest (compatível com HTTP por IP).
        // Para forçar HTTPS, defina REQUIRE_HTTPS_COOKIES=true no ambiente.
        var requireHttps = Environment.GetEnvironmentVariable("REQUIRE_HTTPS_COOKIES") == "true";
        options.Cookie.SecurePolicy = requireHttps
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax; // Lax é compatível com HTTP e redirecionamentos
    });

builder.Services.AddAuthorization(options =>
{
    // SEC-06: PolÃ­tica padrÃ£o: todas as pÃ¡ginas exigem autenticaÃ§Ã£o
    options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
builder.Services.AddHttpContextAccessor();

// --- AuthenticationStateProvider para Blazor Server ---
builder.Services.AddScoped<AuthenticationStateProvider, PersistingAuthenticationStateProvider>();

// --- Data Protection: persiste chaves entre reinicializaÃ§Ãµes do container ---
// ARQ-06: Path condicional para funcionar tanto em Docker (Linux) quanto em dev (Windows)
var keysPathStr = builder.Environment.IsProduction()
    ? "/app/dataprotection-keys"
    : Path.Combine(builder.Environment.ContentRootPath, "dataprotection-keys");
var keysPath = new System.IO.DirectoryInfo(keysPathStr);
if (!keysPath.Exists) keysPath.Create();
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(keysPath)
    .SetApplicationName("Sakrus");

// --- ServiÃ§os de NegÃ³cio ---
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
builder.Services.AddScoped<Sakrus.Services.AgendaService>();

// CRIT-02/03: Serviço de armazenamento seguro de arquivos (fora do wwwroot)
builder.Services.AddSingleton<FileStorageService>();

// HIGH-03: Rate Limiting — protege o endpoint de login contra brute-force em nível de rede
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 10;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
    // Resposta padrão para requisições bloqueadas
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

var app = builder.Build();

// --- Configuração de Localização (pt-BR) ---
var supportedCultures = new[] { "pt-BR" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

// --- Pipeline HTTP ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // HSTS Ã© desativado em containers sem HTTPS configurado
    // Se ativar HTTPS no futuro, descomentar: app.UseHsts();
}

// SEC-04: Headers de segurança HTTP
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    // HIGH-01: Content Security Policy — previne XSS e injeção de scripts externos
    // 'unsafe-inline' é necessário para Blazor Server e MudBlazor (estilos inline)
    // wss: é necessário para o SignalR do Blazor Server
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline'; " +
        "style-src 'self' 'unsafe-inline' fonts.googleapis.com; " +
        "font-src 'self' fonts.gstatic.com data:; " +
        "connect-src 'self' wss: ws:; " +
        "img-src 'self' data:; " +
        "frame-ancestors 'none';";
    await next();
});

app.UseStaticFiles();

// A ORDEM IMPORTA: RateLimiter → Authentication → Authorization → Antiforgery
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Endpoints HTTP para login/logout (cookie auth precisa de request HTTP real)
app.MapAuthEndpoints();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// --- InicializaÃ§Ã£o do Banco: Migrations + Seed (com retry) ---
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    const int maxRetries = 10;
    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation("Tentativa {Attempt}/{Max} de conexÃ£o com o banco de dados...", attempt, maxRetries);

            // Aplica migrations pendentes automaticamente (cria o schema se nÃ£o existir)
            await db.Database.MigrateAsync();

            // Seed do usuÃ¡rio admin padrÃ£o
            var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
            await seeder.SeedAsync();

            // Seed dos lotes baseados no PDF
            await DataSeeder.SeedLotesAsync(db);

            logger.LogInformation("Banco de dados pronto.");
            break;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt)); // backoff: 2s, 4s, 8s...
            logger.LogWarning("Banco ainda nÃ£o disponÃ­vel. Aguardando {Delay}s antes de tentar novamente. Erro: {Message}",
                delay.TotalSeconds, ex.Message);
            await Task.Delay(delay);
        }
    }
}

app.Run();
