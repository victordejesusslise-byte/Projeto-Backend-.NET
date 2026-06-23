using UsuariosAPI.Application.DTOs.Requests;
using UsuariosAPI.Application.DTOs.Responses;

namespace UsuariosAPI.Application.Interfaces;

public interface IUsuarioService
{
    Task<PagedResponse<UsuarioResponse>> ListarAsync(
        ListarUsuariosRequest request,
        CancellationToken cancellationToken = default);

    Task<UsuarioResponse> ObterPorIdAsync(
        long id,
        CancellationToken cancellationToken = default);

    Task<UsuarioResponse> CriarAsync(
        CriarUsuarioRequest request,
        CancellationToken cancellationToken = default);

    Task<UsuarioResponse> AtualizarAsync(
        long id,
        AtualizarUsuarioRequest request,
        CancellationToken cancellationToken = default);

    Task RemoverAsync(
        long id,
        CancellationToken cancellationToken = default);
}
