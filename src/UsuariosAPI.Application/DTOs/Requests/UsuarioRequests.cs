using UsuariosAPI.Domain.Enums;

namespace UsuariosAPI.Application.DTOs.Requests;

/// <summary>DTO para criação de um novo usuário.</summary>
public record CriarUsuarioRequest(
    string Nome,
    string Sobrenome,
    string Email,
    Genero Genero,
    DateTime? DataNascimento
);

/// <summary>DTO para atualização de um usuário existente.</summary>
public record AtualizarUsuarioRequest(
    string Nome,
    string Sobrenome,
    string Email,
    Genero Genero,
    DateTime? DataNascimento
);

/// <summary>Parâmetros de filtro e paginação para listagem.</summary>
public record ListarUsuariosRequest
{
    public int Pagina { get; init; } = 1;
    public int TamanhoPagina { get; init; } = 10;
    public string? Nome { get; init; }
    public string? Email { get; init; }
}
