using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sakrus.Core.Entities;

/// <summary>
/// Representa uma Pessoa no sistema — responsável pelo falecido em um atendimento
/// e/ou proprietário/co-usuário de um jazigo.
/// Futuramente será a base para login GOV.br dos cidadãos.
/// </summary>
public class Responsavel
{
    public int Id { get; set; }
    
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;
    
    [RegularExpression(@"^\d{11}$", ErrorMessage = "CPF deve conter 11 dígitos")]
    public string? CPF { get; set; }
    
    [MaxLength(20)]
    public string RG { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string OrgaoEmissor { get; set; } = string.Empty;
    
    [MaxLength(100)]
    public string Profissao { get; set; } = string.Empty;
    
    [MaxLength(250)]
    public string Endereco { get; set; } = string.Empty;
    
    [MaxLength(20)]
    public string Telefone { get; set; } = string.Empty;

    // ── Campos adicionais (opcionais — retrocompatíveis) ─────────────────────

    /// <summary>E-mail de contato. Futuramente usado para login GOV.br.</summary>
    [MaxLength(200)]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>Data de nascimento — auxilia no processo de sucessão de jazigos.</summary>
    public DateTime? DataNascimento { get; set; }

    /// <summary>
    /// Indica se este responsável possui ao menos um vínculo ativo como
    /// titular, co-usuário ou beneficiário de jazigo.
    /// </summary>
    public bool EhProprietario { get; set; } = false;

    // ── Navigation Properties ────────────────────────────────────────────────

    /// <summary>Documentos (RG, CPF, comprovante etc.) vinculados a esta pessoa.</summary>
    public List<DocumentoAnexo> Documentos { get; set; } = new();

    /// <summary>Vínculos de propriedade/uso com jazigos.</summary>
    public List<JazigoProprietario> JazigoProprietarios { get; set; } = new();
}