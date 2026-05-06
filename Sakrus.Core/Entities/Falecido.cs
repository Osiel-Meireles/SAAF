using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

public class Falecido
{
    [Key]
    public int Id { get; set; }
    public CausaMorte CausaDaMorte { get; set; }
}