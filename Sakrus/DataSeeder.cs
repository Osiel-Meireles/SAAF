using Microsoft.EntityFrameworkCore;
using Sakrus.Core.Entities;
using Sakrus.Core.Enums;
using Sakrus.Infrastructure.Data;

namespace Sakrus;

public static class DataSeeder
{
    public static async Task SeedLotesAsync(ApplicationDbContext context)
    {
        // Limpar dados anteriores de Jazigos, Gavetas e Ossuarios gerados pelo seed incorreto
        // ATENÇÃO: Só executa se não houver Falecidos vinculados para evitar perda de dados reais.
        var hasFalecidos = await context.Falecidos.AnyAsync();
        if (!hasFalecidos)
        {
            await context.Database.ExecuteSqlRawAsync("DELETE FROM \"Jazigos\"; DELETE FROM \"GavetasPublicas\"; DELETE FROM \"Ossuarios\";");
        }
        else
        {
            // Se já tem falecidos, não podemos apagar. Retorna silenciosamente ou podemos criar uma logica de merge.
            // Para segurança do seu banco, retornaremos se a base já tiver dados reais em uso.
            if (await context.Jazigos.AnyAsync(j => j.Quadra == "01" && j.Ala == "01")) 
                return; 
        }

        var modeloPadrao = await context.ModelosJazigos.FirstOrDefaultAsync(m => m.Nome == "Padrão");
        if (modeloPadrao == null)
        {
            modeloPadrao = new ModeloJazigo
            {
                Nome = "Padrão",
                PercentualConcessao = 100,
                PercentualManutencao = 0,
                TaxaConstrucao = 0
            };
            context.ModelosJazigos.Add(modeloPadrao);
            await context.SaveChangesAsync();
        }

        var mId = modeloPadrao.Id;
        var jazigosParaInserir = new List<Jazigo>();

        // HELPER FUNCTIONS
        void AddJazigos(string quadra, string ala, int count, bool isInfantil = false, Dictionary<int, string[]>? variacoes = null)
        {
            for (int i = 1; i <= count; i++)
            {
                // Lote normal
                jazigosParaInserir.Add(new Jazigo
                {
                    CodigoIdentificador = $"JAZ-Q{quadra.Replace(" ", "")}-A{ala}-L{i:D3}",
                    Quadra = quadra,
                    Ala = ala,
                    NumeroLote = i.ToString("D3"),
                    ModeloJazigoId = mId,
                    IsInfantil = isInfantil,
                    Ocupado = false
                });

                // Lotes com variação (ex: 10-A, 10-B)
                if (variacoes != null && variacoes.ContainsKey(i))
                {
                    foreach (var varSufixo in variacoes[i])
                    {
                        jazigosParaInserir.Add(new Jazigo
                        {
                            CodigoIdentificador = $"JAZ-Q{quadra.Replace(" ", "")}-A{ala}-L{i:D3}-{varSufixo}",
                            Quadra = quadra,
                            Ala = ala,
                            NumeroLote = $"{i:D3}-{varSufixo}",
                            ModeloJazigoId = mId,
                            IsInfantil = isInfantil,
                            Ocupado = false
                        });
                    }
                }
            }
        }

        void AddJazigosMult(string quadra, int[] alas, int count, bool isInfantil = false)
        {
            foreach (var ala in alas)
            {
                AddJazigos(quadra, ala.ToString("D2"), count, isInfantil);
            }
        }
        
        void AddJazigosRange(string quadra, int startAla, int endAla, int count, bool isInfantil = false)
        {
            for (int ala = startAla; ala <= endAla; ala++)
            {
                AddJazigos(quadra, ala.ToString("D2"), count, isInfantil);
            }
        }

        // ==========================================
        // QUADRA 01
        // ==========================================
        AddJazigos("01", "01", 59);
        AddJazigos("01", "02", 60);
        AddJazigos("01", "03", 49);
        AddJazigos("01", "04", 50);
        AddJazigos("01", "05", 48);
        AddJazigosMult("01", new[] { 6, 7 }, 20);
        AddJazigos("01", "08", 16);
        AddJazigosMult("01", new[] { 9, 10 }, 18);
        AddJazigos("01", "11", 37);
        AddJazigosMult("01", new[] { 12, 13, 15 }, 34);
        AddJazigos("01", "14", 35);
        AddJazigos("01", "16", 30);
        AddJazigos("01", "17", 35, variacoes: new() { { 1, new[] { "A", "B" } }, { 25, new[] { "A", "B" } }, { 30, new[] { "A", "B" } }, { 31, new[] { "A", "B" } } });
        AddJazigos("01", "18", 33, variacoes: new() { { 10, new[] { "A", "B" } } });
        AddJazigos("01", "19", 36);
        AddJazigos("01", "20", 37);
        AddJazigos("01", "21", 41);
        AddJazigos("01", "22", 45);
        AddJazigos("01", "23", 42);
        AddJazigos("01", "24", 43);
        AddJazigos("01", "25", 40);
        AddJazigos("01", "26", 39);
        AddJazigos("01", "27", 38);
        AddJazigos("01", "28", 23);
        AddJazigos("01", "29", 16);

        // ==========================================
        // QUADRA 02
        // ==========================================
        AddJazigosMult("02", new[] { 2, 3, 9, 10, 14 }, 20);
        AddJazigos("02", "04", 16);
        AddJazigos("02", "05", 21, variacoes: new() { { 12, new[] { "A" } } });
        AddJazigos("02", "06", 20, variacoes: new() { { 6, new[] { "A", "B" } } });
        AddJazigosMult("02", new[] { 7, 17 }, 19);
        AddJazigosMult("02", new[] { 8, 18 }, 21);
        AddJazigos("02", "11", 22, variacoes: new() { { 5, new[] { "A", "B" } } });
        AddJazigosMult("02", new[] { 12, 15, 16, 21 }, 18);
        AddJazigos("02", "13", 21, variacoes: new() { { 11, new[] { "A", "B" } } });
        AddJazigos("02", "19", 17);
        AddJazigosMult("02", new[] { 20, 25 }, 25);
        AddJazigosMult("02", new[] { 22, 26, 28 }, 23);
        AddJazigos("02", "23", 24);
        AddJazigos("02", "24", 25, variacoes: new() { { 20, new[] { "A", "B", "C" } } });
        AddJazigos("02", "27", 22);
        AddJazigos("02", "29", 25);
        AddJazigos("02", "30", 17);
        AddJazigos("02", "31", 3);

        // ==========================================
        // QUADRA 03
        // ==========================================
        AddJazigosRange("03", 1, 16, 32);
        AddJazigos("03", "17", 50);

        // ==========================================
        // QUADRA 03 INFANTIS
        // ==========================================
        AddJazigosRange("03 Infantis", 1, 2, 48, isInfantil: true);
        AddJazigosRange("03 Infantis", 3, 4, 68, isInfantil: true);

        // ==========================================
        // QUADRA 04
        // ==========================================
        AddJazigosRange("04", 1, 6, 16);

        // ==========================================
        // QUADRA COLETIVO
        // ==========================================
        AddJazigos("Coletivo", "00", 8);

        // ==========================================
        // QUADRA INFANTIS
        // ==========================================
        AddJazigos("Infantis", "01", 300, isInfantil: true);
        AddJazigos("Infantis", "02", 100, isInfantil: true);

        // ==========================================
        // DEMAIS QUADRAS
        // ==========================================
        AddJazigos("Provisória", "01", 100);
        AddJazigos("Novo Paraná", "01", 50);
        AddJazigos("Vila II", "01", 100);
        AddJazigos("Bela Vista", "01", 20);
        AddJazigos("Muriçoca", "01", 20);


        // Salvar Jazigos em Lotes
        int chunkSize = 500;
        for (int i = 0; i < jazigosParaInserir.Count; i += chunkSize)
        {
            context.Jazigos.AddRange(jazigosParaInserir.Skip(i).Take(chunkSize));
            await context.SaveChangesAsync();
        }

        // ==========================================
        // GAVETAS PÚBLICAS (Rotativos e Permanentes)
        // ==========================================
        var gavetasParaInserir = new List<GavetaPublica>();

        void AddGavetas(string quadra, string setor, int count)
        {
            for (int i = 1; i <= count; i++)
            {
                gavetasParaInserir.Add(new GavetaPublica
                {
                    Quadra = quadra,
                    Setor = setor,
                    Lote = i.ToString("D3"),
                    NumeroGaveta = i.ToString("D3"),
                    Ocupada = false
                });
            }
        }

        // Rotativos
        AddGavetas("05.07 Gaveta Pública", "01", 256);
        AddGavetas("Jazigo Rotativo Público QD 17", "01", 200);
        AddGavetas("Jazigo Rotativo Público QD 18", "01", 208);
        AddGavetas("Jazigo Temporário Público QD 05", "01", 256);

        // Permanentes
        AddGavetas("05 - PAFIR", "05", 200);
        AddGavetas("05.04 - Santo Antônio", "GAV", 200);
        AddGavetas("Gaveta Infantil Q 01 Ala 12", "INF", 300);
        AddGavetas("Santa Clara", "01", 120);
        AddGavetas("Santa Clara II", "01", 100);
        AddGavetas("Santo Antônio", "01", 100);
        AddGavetas("São Cristóvão", "01", 200);

        for (int i = 0; i < gavetasParaInserir.Count; i += chunkSize)
        {
            context.GavetasPublicas.AddRange(gavetasParaInserir.Skip(i).Take(chunkSize));
            await context.SaveChangesAsync();
        }

        // ==========================================
        // OSSUÁRIOS
        // ==========================================
        var ossuariosParaInserir = new List<Ossuario>();
        
        // Público I: Gavetas 01 a 50
        for (int i = 1; i <= 50; i++)
        {
            ossuariosParaInserir.Add(new Ossuario
            {
                Identificador = $"OSS-PUB-{i:D3}",
                Quadra = "Público I",
                Ala = "01",
                NumeroLote = i.ToString("D3"),
                Tipo = TipoOssuario.Geral,
                Capacidade = 10 
            });
        }
        
        context.Ossuarios.AddRange(ossuariosParaInserir);
        await context.SaveChangesAsync();
    }
}
