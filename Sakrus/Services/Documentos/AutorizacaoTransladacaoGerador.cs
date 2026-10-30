using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Services.Documentos;

/// <summary>
/// Documento 10-A — Autorização para Transladação de Despojos.
/// </summary>
public class AutorizacaoTransladacaoGerador : GeradorBase
{
    public AutorizacaoTransladacaoGerador(IDbContextFactory<ApplicationDbContext> dbFactory)
        : base(dbFactory)
    {
    }

    public override string Codigo => "10-A";

    public override IReadOnlyList<CampoDefinicao> Campos => new[]
    {
        new CampoDefinicao("cemiterioOrigem", "Cemitério de origem", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("municipioOrigem", "Município de origem", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("cemiterioDestino", "Cemitério de destino (LEM)", TipoCampo.Texto, Obrigatorio: true),
        new CampoDefinicao("dataAutorizacao", "Data da autorização", TipoCampo.Data),
        new CampoDefinicao("responsavelId", "Responsável / Requerente", TipoCampo.Selecao, FonteOpcoes: "responsaveis")
    };

    public override async Task<byte[]> GerarAsync(ContextoEmissao ctx)
    {
        using var db = await DbFactory.CreateDbContextAsync();

        var (falecido, atendimento, responsavelPadrao, _) = await DadosComuns.ResolverFalecidoEOrigemAsync(db, ctx.EntidadeId);

        var responsavelId = ctx.Campos.GetInt("responsavelId") ?? responsavelPadrao?.Id;
        PessoaQualificacao? responsavel = null;
        if (responsavelId.HasValue)
        {
            responsavel = await DadosComuns.ResponsavelAsync(db, responsavelId.Value);
        }

        var cemiterioOrigem = ctx.Campos.Get("cemiterioOrigem");
        var municipioOrigem = ctx.Campos.Get("municipioOrigem");
        var cemiterioDestino = ctx.Campos.Get("cemiterioDestino");

        Exigir(
            ("Cemitério de origem", cemiterioOrigem),
            ("Município de origem", municipioOrigem),
            ("Cemitério de destino", cemiterioDestino));

        var dataAutorizacao = ctx.Campos.GetData("dataAutorizacao") ?? ctx.DataEmissao;
        var falecidoDados = await DadosComuns.FalecidoAsync(db, falecido.Id);

        return Pdf(ctx, "AUTORIZAÇÃO PARA TRANSLADAÇÃO DE DESPOJOS", col =>
        {
            col.TituloBloco("DADOS DO FALECIDO E REGISTRO CIVIL");
            col.TabelaModelo(
                new (string, string?, int)[] { ("Nome", falecidoDados.Nome, 12) },
                new (string, string?, int)[]
                {
                    ("Sexo", falecidoDados.Sexo, 2), ("Idade", falecidoDados.Idade?.ToString(), 2),
                    ("Data do óbito", falecidoDados.DataObito?.ToString("dd/MM/yyyy"), 4),
                    ("Nº da D.O.", falecidoDados.NumeroDeclaracaoObito, 4)
                },
                new (string, string?, int)[]
                {
                    ("Cartório de Registro Civil", falecidoDados.Cartorio, 5),
                    ("Município do cartório", falecidoDados.MunicipioCartorio, 3),
                    ("Matrícula de óbito", falecidoDados.MatriculaObito, 4)
                });

            col.TituloBloco("DADOS DO REQUERENTE / RESPONSÁVEL");
            col.TabelaModelo(
                new (string, string?, int)[] { ("Nome", responsavel?.Nome, 12) },
                new (string, string?, int)[]
                {
                    ("CPF", responsavel?.Cpf, 4), ("RG", responsavel?.Rg, 4), ("Parentesco", responsavel?.Parentesco, 4)
                },
                new (string, string?, int)[]
                {
                    ("Endereço", responsavel?.Endereco, 6), ("Telefone", responsavel?.Telefone, 6)
                });

            col.TituloBloco("ORIGEM E DESTINO DOS DESPOJOS");
            col.TabelaModelo(
                new (string, string?, int)[]
                {
                    ("Cemitério de origem", cemiterioOrigem, 6),
                    ("Município de origem", municipioOrigem, 6)
                },
                new (string, string?, int)[]
                {
                    ("Cemitério de destino", cemiterioDestino, 6),
                    ("Município de destino", "Luís Eduardo Magalhães - BA", 6)
                });

            col.Declaracao(TextosPadrao.AutorizacaoTransladacaoTexto);

            col.LocalEData(new ContextoEmissao(ctx.EntidadeId, ctx.Campos, ctx.Instituicao, ctx.Numero, dataAutorizacao));
            col.AssinaturaCoordenacao(ctx);
        });
    }
}
