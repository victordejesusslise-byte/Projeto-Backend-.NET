using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UsuariosAPI.Application.DTOs.Requests;
using UsuariosAPI.Application.DTOs.Responses;
using UsuariosAPI.Application.Exceptions;
using UsuariosAPI.Application.Interfaces;
using UsuariosAPI.Domain.Enums;

namespace UsuariosAPI.API.Pages;

public class IndexModel : PageModel
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IUsuarioService usuarioService, ILogger<IndexModel> logger)
    {
        _usuarioService = usuarioService;
        _logger = logger;
    }

    [BindProperty]
    public UsuarioFormularioModel Formulario { get; set; } = new();

    [BindProperty]
    public long? BuscaId { get; set; }

    public UsuarioResponse? UsuarioEncontrado { get; private set; }
    public string? MensagemFormulario { get; private set; }
    public string TipoMensagemFormulario { get; private set; } = "sucesso";
    public string? MensagemBusca { get; private set; }
    public bool EmEdicao => Formulario.Id.HasValue && Formulario.Id.Value > 0;

    public void OnGet()
    {
    }

    public async Task OnGetEditarAsync(long id, CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await _usuarioService.ObterPorIdAsync(id, cancellationToken);
            PreencherFormulario(usuario);
            MensagemFormulario = $"Editando usuário #{usuario.Id}.";
            TipoMensagemFormulario = "sucesso";
        }
        catch (BusinessException ex)
        {
            MensagemFormulario = ObterMensagemErro(ex);
            TipoMensagemFormulario = "erro";
        }
    }

    public async Task<IActionResult> OnPostSalvarAsync(CancellationToken cancellationToken)
    {
        try
        {
            var genero = ConverterGenero(Formulario.Genero);

            if (Formulario.Id.HasValue && Formulario.Id.Value > 0)
            {
                var request = new AtualizarUsuarioRequest(
                    Formulario.Nome?.Trim() ?? string.Empty,
                    Formulario.Sobrenome?.Trim() ?? string.Empty,
                    Formulario.Email?.Trim() ?? string.Empty,
                    genero,
                    Formulario.DataNascimento);

                await _usuarioService.AtualizarAsync(Formulario.Id.Value, request, cancellationToken);
                MensagemFormulario = "Usuário atualizado com sucesso.";
            }
            else
            {
                var request = new CriarUsuarioRequest(
                    Formulario.Nome?.Trim() ?? string.Empty,
                    Formulario.Sobrenome?.Trim() ?? string.Empty,
                    Formulario.Email?.Trim() ?? string.Empty,
                    genero,
                    Formulario.DataNascimento);

                await _usuarioService.CriarAsync(request, cancellationToken);
                MensagemFormulario = "Usuário cadastrado com sucesso.";
            }

            TipoMensagemFormulario = "sucesso";
            Formulario = new UsuarioFormularioModel();
        }
        catch (BusinessException ex)
        {
            MensagemFormulario = ObterMensagemErro(ex);
            TipoMensagemFormulario = "erro";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao salvar usuário pela tela Razor.");
            MensagemFormulario = "Erro interno ao salvar o usuário.";
            TipoMensagemFormulario = "erro";
        }

        return Page();
    }

    public async Task<IActionResult> OnPostBuscarAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!BuscaId.HasValue || BuscaId.Value <= 0)
                throw new Application.Exceptions.ValidationException(
                    new[] { "Informe um ID maior que zero." });

            UsuarioEncontrado = await _usuarioService.ObterPorIdAsync(BuscaId.Value, cancellationToken);
        }
        catch (BusinessException ex)
        {
            MensagemBusca = ObterMensagemErro(ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao buscar usuário pela tela Razor.");
            MensagemBusca = "Erro interno ao buscar usuário.";
        }

        return Page();
    }

    public string FormatarData(DateTime? data)
    {
        return data?.ToString("dd/MM/yyyy") ?? "-";
    }

    private void PreencherFormulario(UsuarioResponse usuario)
    {
        Formulario = new UsuarioFormularioModel
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Sobrenome = usuario.Sobrenome,
            Email = usuario.Email,
            Genero = usuario.Genero,
            DataNascimento = usuario.DataNascimento
        };
    }

    private static Genero ConverterGenero(string? genero)
    {
        if (Enum.TryParse<Genero>(genero, ignoreCase: true, out var valor))
            return valor;

        throw new Application.Exceptions.ValidationException(
            new[] { "Gênero inválido. Valores aceitos: Masculino, Feminino ou Outro." });
    }

    private static string ObterMensagemErro(BusinessException exception)
    {
        return exception is Application.Exceptions.ValidationException validationException
            ? string.Join(" ", validationException.Erros)
            : exception.Message;
    }
}

public class UsuarioFormularioModel
{
    public long? Id { get; set; }
    public string? Nome { get; set; }
    public string? Sobrenome { get; set; }
    public string? Email { get; set; }
    public string? Genero { get; set; }
    public DateTime? DataNascimento { get; set; }
}
