using Microsoft.Extensions.DependencyInjection;

namespace Sakrus.Services.Documentos;

public static class DocumentosServiceCollectionExtensions
{
    public static IServiceCollection AddModuloDocumentos(this IServiceCollection services)
    {
        if (!services.Any(d => d.ServiceType == typeof(TimeProvider)))
            services.AddSingleton(TimeProvider.System);

        services.AddScoped<INumeracaoService, NumeracaoService>();
        services.AddScoped<IProtocoloService, ProtocoloService>();
        services.AddScoped<IGavetaSelecaoService, GavetaSelecaoService>();
        services.AddScoped<IDocumentoEmissaoService, DocumentoEmissaoService>();

        var geradorTypes = typeof(DocumentosServiceCollectionExtensions).Assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IDocumentoGerador).IsAssignableFrom(t));

        foreach (var tipo in geradorTypes)
            services.AddScoped(typeof(IDocumentoGerador), tipo);

        return services;
    }
}
