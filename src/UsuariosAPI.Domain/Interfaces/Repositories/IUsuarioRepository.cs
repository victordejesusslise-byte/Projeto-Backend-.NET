using UsuariosAPI.Domain.Entities;

namespace UsuariosAPI.Domain.Interfaces.Repositories;

public interface IUsuarioRepository
{
    Task<(IEnumerable<Usuario> Usuarios, int Total)> ObterTodosAsync(
        int pagina,
        int tamanhoPagina,
        string? nome = null,
        string? email = null,
        CancellationToken cancellationToken = default);

    Task<Usuario?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<Usuario> AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task<Usuario> AtualizarAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task RemoverAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task<bool> EmailExisteAsync(string email, long? ignorarId = null, CancellationToken cancellationToken = default);
}
