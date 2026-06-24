using Microsoft.AspNetCore.Mvc;
using UsuariosAPI.Application.DTOs.Requests;
using UsuariosAPI.Application.DTOs.Responses;
using UsuariosAPI.Application.Interfaces;

namespace UsuariosAPI.API.Controllers;

/// <summary>
/// Gerenciamento de usuários — operações de CRUD com paginação e filtros.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service)
    {
        _service = service;
    }

    /// <summary>Lista todos os usuários com filtros e paginação.</summary>
    /// <param name="pagina">Número da página (default: 1).</param>
    /// <param name="tamanhoPagina">Itens por página, entre 1 e 100 (default: 10).</param>
    /// <param name="nome">Filtro parcial por nome ou sobrenome.</param>
    /// <param name="email">Filtro parcial por e-mail.</param>
    /// <param name="cancellationToken">Sinal de cancelamento da requisição.</param>
    /// <response code="200">Listagem paginada de usuários.</response>
    /// <response code="400">Tipo inválido em um parâmetro de consulta.</response>
    /// <response code="422">Parâmetros de paginação inválidos.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResponse<UsuarioResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Listar(
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 10,
        [FromQuery] string? nome = null,
        [FromQuery] string? email = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ListarUsuariosRequest
        {
            Pagina = pagina,
            TamanhoPagina = tamanhoPagina,
            Nome = nome,
            Email = email
        };

        var resultado = await _service.ListarAsync(request, cancellationToken);
        return Ok(new ApiResponse<PagedResponse<UsuarioResponse>>(true, "Usuários listados com sucesso.", resultado));
    }

    /// <summary>Retorna os detalhes de um usuário pelo ID.</summary>
    /// <param name="id">ID numérico do usuário. Exemplo: 1.</param>
    /// <param name="cancellationToken">Sinal de cancelamento da requisição.</param>
    /// <response code="200">Dados do usuário encontrado.</response>
    /// <response code="404">Usuário não encontrado.</response>
    /// <response code="422">Identificador menor ou igual a zero.</response>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ObterPorId(
        long id,
        CancellationToken cancellationToken)
    {
        var usuario = await _service.ObterPorIdAsync(id, cancellationToken);
        return Ok(new ApiResponse<UsuarioResponse>(true, "Usuário encontrado.", usuario));
    }

    /// <summary>Cadastra um novo usuário.</summary>
    /// <param name="request">Dados do novo usuário.</param>
    /// <param name="cancellationToken">Sinal de cancelamento da requisição.</param>
    /// <response code="201">Usuário criado com sucesso.</response>
    /// <response code="400">Corpo ausente, JSON inválido ou tipo incorreto.</response>
    /// <response code="409">E-mail já está em uso.</response>
    /// <response code="422">Dados de entrada inválidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<UsuarioResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Criar(
        [FromBody] CriarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var usuario = await _service.CriarAsync(request, cancellationToken);
        return CreatedAtAction(
            nameof(ObterPorId),
            new { id = usuario.Id },
            new ApiResponse<UsuarioResponse>(true, "Usuário cadastrado com sucesso.", usuario));
    }

    /// <summary>Atualiza os dados de um usuário existente.</summary>
    /// <param name="id">ID numérico do usuário. Exemplo: 1.</param>
    /// <param name="request">Novos dados completos do usuário.</param>
    /// <param name="cancellationToken">Sinal de cancelamento da requisição.</param>
    /// <response code="200">Usuário atualizado com sucesso.</response>
    /// <response code="400">Corpo ausente, JSON inválido ou tipo incorreto.</response>
    /// <response code="404">Usuário não encontrado.</response>
    /// <response code="409">E-mail já está em uso por outro usuário.</response>
    /// <response code="422">Dados de entrada inválidos.</response>
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(ApiResponse<UsuarioResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Atualizar(
        long id,
        [FromBody] AtualizarUsuarioRequest request,
        CancellationToken cancellationToken)
    {
        var usuario = await _service.AtualizarAsync(id, request, cancellationToken);
        return Ok(new ApiResponse<UsuarioResponse>(true, "Usuário atualizado com sucesso.", usuario));
    }

    /// <summary>Remove um usuário pelo ID.</summary>
    /// <param name="id">ID numérico do usuário. Exemplo: 1.</param>
    /// <param name="cancellationToken">Sinal de cancelamento da requisição.</param>
    /// <response code="204">Usuário removido com sucesso.</response>
    /// <response code="404">Usuário não encontrado.</response>
    /// <response code="422">Identificador menor ou igual a zero.</response>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Remover(
        long id,
        CancellationToken cancellationToken)
    {
        await _service.RemoverAsync(id, cancellationToken);
        return NoContent();
    }
}
