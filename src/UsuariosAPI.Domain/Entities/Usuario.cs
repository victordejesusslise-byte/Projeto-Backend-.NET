using UsuariosAPI.Domain.Enums;

namespace UsuariosAPI.Domain.Entities;

public class Usuario
{
    public long Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Sobrenome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public Genero Genero { get; private set; }
    public DateTime? DataNascimento { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    protected Usuario() { }

    public Usuario(string nome, string sobrenome, string email, Genero genero, DateTime? dataNascimento)
    {
        Nome = nome;
        Sobrenome = sobrenome;
        Email = email;
        Genero = genero;
        DataNascimento = dataNascimento;
        CriadoEm = DateTime.UtcNow;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Atualizar(string nome, string sobrenome, string email, Genero genero, DateTime? dataNascimento)
    {
        Nome = nome;
        Sobrenome = sobrenome;
        Email = email;
        Genero = genero;
        DataNascimento = dataNascimento;
        AtualizadoEm = DateTime.UtcNow;
    }
}
