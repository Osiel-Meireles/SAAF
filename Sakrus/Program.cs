using MudBlazor.Services;
using Microsoft.EntityFrameworkCore;
using Sakrus.Components;
using Sakrus.Infrastructure.Data;
using Sakrus.Services;

var builder = WebApplication.CreateBuilder(args);

// Adiciona os serviços do Blazor e MudBlazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

// 1. CORREÇÃO: Registro do Banco de Dados (PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Registro dos Serviços de Regras de Negócio
builder.Services.AddScoped<GavetaPublicaService>();
builder.Services.AddScoped<AtendimentoFaturamentoService>();
builder.Services.AddScoped<JazigoService>();
builder.Services.AddMudServices();

var app = builder.Build();

// Configuração do pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();