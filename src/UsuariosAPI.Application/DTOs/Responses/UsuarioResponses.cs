using UsuariosAPI.Domain.Enums;

namespace UsuariosAPI.Application.DTOs.Responses;

/// <summary>Dados retornados de um usuário.</summary>
public record UsuarioResponse(
    long Id,
    string Nome,
    string Sobrenome,
    string Email,
    string Genero,
    DateTime? DataNascimento,
    DateTime CriadoEm,
    DateTime AtualizadoEm
);

/// <summary>Resposta paginada de listagem de usuários.</summary>
public record PagedResponse<T>(
    IEnumerable<T> Data,
    int Pagina,
    int TamanhoPagina,
    int Total,
    int TotalPaginas
);

/// <summary>Envelope de resposta padrão da API.</summary>
public record ApiResponse<T>(
    bool Sucesso,
    string Mensagem,
    T? Dados,
    IEnumerable<string>? Erros = null,
    string? TraceId = null,
    DebugResponse? Debug = null
);

/// <summary>
/// Detalhes técnicos retornados exclusivamente no ambiente Development.
/// </summary>
public record DebugResponse(
    string Tipo,
    string Mensagem,
    string? StackTrace
);
