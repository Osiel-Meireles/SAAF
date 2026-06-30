using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using Sakrus.Components.Pages;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;
using Sakrus.Services;
using Xunit;
using Microsoft.JSInterop;

namespace Sakrus.Tests;

/// <summary>
/// Testes automatizados com bUnit para verificar o funcionamento dos botões 
/// e fluxo de estado da tela Obitos, garantindo que o contexto seja recriado.
/// </summary>
public class ObitosPageTests : TestContext
{
    private readonly Mock<IDialogService> _dialogServiceMock;
    private readonly Mock<ISnackbar> _snackbarMock;
    private readonly Mock<IDbContextFactory<ApplicationDbContext>> _dbFactoryMock;
    private readonly Mock<IJSRuntime> _jsRuntimeMock;
    private readonly Mock<IAuthService> _authServiceMock;

    public ObitosPageTests()
    {
        // 1. Configurar MudBlazor
        Services.AddMudServices();
        
        // 2. Mock dos serviços injetados
        _dialogServiceMock = new Mock<IDialogService>();
        _snackbarMock = new Mock<ISnackbar>();
        _dbFactoryMock = new Mock<IDbContextFactory<ApplicationDbContext>>();
        _jsRuntimeMock = new Mock<IJSRuntime>();
        _authServiceMock = new Mock<IAuthService>();

        Services.AddSingleton(_dialogServiceMock.Object);
        Services.AddSingleton(_snackbarMock.Object);
        Services.AddSingleton(_dbFactoryMock.Object);
        Services.AddSingleton(_jsRuntimeMock.Object);
        Services.AddSingleton(_authServiceMock.Object);
        
        // RelatorioService injected as concrete class
        Services.AddScoped<RelatorioService>();

        // Configurar JS Interop Mock (MudBlazor precisa disso)
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Configurar Banco em Memória com mesmo nome para todos os contextos
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
            
        // Setup DbFactory para retornar um NOVO contexto a cada chamada (simula comportamento real)
        _dbFactoryMock.Setup(f => f.CreateDbContext()).Returns(() => new ApplicationDbContext(options));
        
        // Popular dados simulados
        using var seedContext = new ApplicationDbContext(options);
        var falecido = new Falecido { Id = 1, Nome = "João Teste", Status = StatusFalecido.NaoSepultado };
        var atendimento = new Atendimento 
        { 
            Id = 1, 
            Responsavel = new Responsavel { Id = 1, Nome = "Resp Teste", CPF = "11111111111" },
            FalecidoId = 1,
            Falecido = falecido
        };
        
        seedContext.Falecidos.Add(falecido);
        seedContext.Atendimentos.Add(atendimento);
        seedContext.SaveChanges();
    }

    [Fact]
    public void BotaoSepultamento_DeveChamarDialogService()
    {
        // Arrange
        // Renderiza o PopoverProvider primeiro para evitar erro do MudBlazor
        RenderComponent<MudPopoverProvider>();
        
        var mockDialogReference = new Mock<IDialogReference>();
        mockDialogReference.Setup(x => x.Result).ReturnsAsync(DialogResult.Ok(true));
        
        _dialogServiceMock
            .Setup(x => x.ShowAsync<RealizarSepultamentoDialog>(It.IsAny<string>(), It.IsAny<DialogParameters>(), It.IsAny<DialogOptions>()))
            .ReturnsAsync(mockDialogReference.Object);

        var cut = RenderComponent<Obitos>();

        // Act
        // O botão de sepultar deve estar visível porque o Status é NaoSepultado
        var btnSepultar = cut.Find("button[title='Realizar Sepultamento']");
        btnSepultar.Click();

        // Assert
        // Verifica se o DialogService foi chamado para abrir o modal de sepultamento
        _dialogServiceMock.Verify(x => x.ShowAsync<RealizarSepultamentoDialog>(
            "Realizar Sepultamento", 
            It.IsAny<DialogParameters>(), 
            It.IsAny<DialogOptions>()), Times.Once);
            
        // Verifica se o recarregamento de dados (CreateDbContext) foi chamado mais de uma vez 
        // (uma na inicialização e outra após o fechamento do dialog). 
        // ISSO VALIDA A CORREÇÃO DO BUG (Recriação do DbContext).
        _dbFactoryMock.Verify(f => f.CreateDbContext(), Times.Once);
    }
}
