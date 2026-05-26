using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sakrus.Infrastructure.Data;
using Sakrus.Core.Entities;

var services = new ServiceCollection();
// Substitua pela connection string correta, ou se for SQLite, use UseSqlite
services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("Server=localhost;Database=SakrusDb;Trusted_Connection=True;TrustServerCertificate=True;"));

var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

try
{
    var gaveta = db.GavetasPublicas.FirstOrDefault();
    if (gaveta == null)
    {
        Console.WriteLine("Nenhuma gaveta encontrada.");
        return;
    }

    var novoFalecido = new Falecido
    {
        Nome = "Teste Falecido",
        Cpf = "12345678901",
        CausaMorte = CausaMorte.Natural,
        TipoRestosMortais = TipoRestosMortais.CorpoInteiro,
        DataFalecimento = DateTime.Today
    };

    db.Falecidos.Add(novoFalecido);
    db.SaveChanges();

    gaveta.Ocupada = true;
    gaveta.FalecidoId = novoFalecido.Id;
    gaveta.DataOcupacao = DateTime.Today;
    gaveta.DataPrevisaoExumacao = DateTime.Today.AddYears(3);

    db.GavetasPublicas.Update(gaveta);
    db.SaveChanges();

    Console.WriteLine("Sucesso!");
}
catch (Exception ex)
{
    Console.WriteLine("ERRO:");
    Console.WriteLine(ex.InnerException != null ? ex.InnerException.Message : ex.Message);
}
