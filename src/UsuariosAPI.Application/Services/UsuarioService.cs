using FluentValidation;
using UsuariosAPI.Application.DTOs.Requests;
using UsuariosAPI.Application.DTOs.Responses;
using UsuariosAPI.Application.Exceptions;
using UsuariosAPI.Application.Interfaces;
using UsuariosAPI.Domain.Entities;
using UsuariosAPI.Domain.Interfaces.Repositories;

namespace UsuariosAPI.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repository;
    private readonly IValidator<CriarUsuarioRequest> _criarValidator;
    private readonly IValidator<AtualizarUsuarioRequest> _atualizarValidator;
    private readonly IValidator<ListarUsuariosRequest> _listarValidator;

    public UsuarioService(
        IUsuarioRepository repository,
        IValidator<CriarUsuarioRequest> criarValidator,
        IValidator<AtualizarUsuarioRequest> atualizarValidator,
        IValidator<ListarUsuariosRequest> listarValidator)
    {
        _repository = repository;
        _criarValidator = criarValidator;
        _atualizarValidator = atualizarValidator;
        _listarValidator = listarValidator;
    }

    public async Task<PagedResponse<UsuarioResponse>> ListarAsync(
        ListarUsuariosRequest request,
        CancellationToken cancellationToken = default)
    {
        var validacao = await _listarValidator.ValidateAsync(request, cancellationToken);
        if (!validacao.IsValid)
            throw new Exceptions.ValidationException(validacao.Errors.Select(e => e.ErrorMessage));

        var (usuarios, total) = await _repository.ObterTodosAsync(
            request.Pagina,
            request.TamanhoPagina,
            request.Nome,
            request.Email,
            cancellationToken);

        var totalPaginas = (int)Math.Ceiling(total / (double)request.TamanhoPagina);

        return new PagedResponse<UsuarioResponse>(
            usuarios.Select(MapToResponse),
            request.Pagina,
            request.TamanhoPagina,
            total,
            totalPaginas);
    }

    public async Task<UsuarioResponse> ObterPorIdAsync(
        long id,
        CancellationToken cancellationToken = default)
    {
        ValidarId(id);

        var usuario = await _repository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Usuário", id);

        return MapToResponse(usuario);
    }

    public async Task<UsuarioResponse> CriarAsync(
        CriarUsuarioRequest request,
        CancellationToken cancellationToken = default)
    {
        var validacao = await _criarValidator.ValidateAsync(request, cancellationToken);
        if (!validacao.IsValid)
            throw new Exceptions.ValidationException(validacao.Errors.Select(e => e.ErrorMessage));

        if (await _repository.EmailExisteAsync(request.Email, cancellationToken: cancellationToken))
            throw new ConflictException($"Já existe um usuário com o e-mail '{request.Email}'.");

        var usuario = new Usuario(
            request.Nome,
            request.Sobrenome,
            request.Email,
            request.Genero,
            request.DataNascimento);

        var criado = await _repository.AdicionarAsync(usuario, cancellationToken);
        return MapToResponse(criado);
    }

    public async Task<UsuarioResponse> AtualizarAsync(
        long id,
        AtualizarUsuarioRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidarId(id);

        var validacao = await _atualizarValidator.ValidateAsync(request, cancellationToken);
        if (!validacao.IsValid)
            throw new Exceptions.ValidationException(validacao.Errors.Select(e => e.ErrorMessage));

        var usuario = await _repository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Usuário", id);

        if (await _repository.EmailExisteAsync(request.Email, ignorarId: id, cancellationToken: cancellationToken))
            throw new ConflictException($"Já existe um usuário com o e-mail '{request.Email}'.");

        usuario.Atualizar(
            request.Nome,
            request.Sobrenome,
            request.Email,
            request.Genero,
            request.DataNascimento);

        var atualizado = await _repository.AtualizarAsync(usuario, cancellationToken);
        return MapToResponse(atualizado);
    }

    public async Task RemoverAsync(long id, CancellationToken cancellationToken = default)
    {
        ValidarId(id);

        var usuario = await _repository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Usuário", id);

        await _repository.RemoverAsync(usuario, cancellationToken);
    }

    private static void ValidarId(long id)
    {
        if (id <= 0)
            throw new Exceptions.ValidationException(
                new[] { "Id do usuário deve ser maior que zero." });
    }

    private static UsuarioResponse MapToResponse(Usuario u) => new(
        u.Id,
        u.Nome,
        u.Sobrenome,
        u.Email,
        u.Genero.ToString(),
        u.DataNascimento,
        u.CriadoEm,
        u.AtualizadoEm);
}
