using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UsuariosAPI.Application.DTOs.Requests;
using UsuariosAPI.Application.DTOs.Responses;
using UsuariosAPI.Application.Exceptions;
using UsuariosAPI.Application.Interfaces;
using UsuariosAPI.Domain.Enums;

namespace UsuariosAPI.API.Pages.Admin;

[Authorize]
public class TabelaModel : PageModel
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<TabelaModel> _logger;

    public TabelaModel(IUsuarioService usuarioService, ILogger<TabelaModel> logger)
    {
        _usuarioService = usuarioService;
        _logger = logger;
    }

    [BindProperty]
    public AdminUsuarioFormularioModel Formulario { get; set; } = new();

    [BindProperty]
    public long? BuscaId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FiltroNome { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FiltroEmail { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Pagina { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int TamanhoPagina { get; set; } = 10;

    public PagedResponse<UsuarioResponse>? Usuarios { get; private set; }
    public UsuarioResponse? UsuarioEncontrado { get; private set; }
    public string? MensagemAcao { get; private set; }
    public string TipoMensagemAcao { get; private set; } = "sucesso";
    public string? MensagemBusca { get; private set; }
    public string? MensagemLista { get; private set; }
    public string TipoMensagemLista { get; private set; } = "sucesso";
    public bool EmEdicao => Formulario.Id.HasValue && Formulario.Id.Value > 0;

    public string ResumoLista
    {
        get
        {
            if (Usuarios is null)
                return "Lista ainda não carregada.";

            var totalPaginas = Math.Max(Usuarios.TotalPaginas, 1);
            return $"{Usuarios.Total} registro(s) encontrado(s). Página {Usuarios.Pagina} de {totalPaginas}.";
        }
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        await CarregarListaAsync(cancellationToken);
    }

    public async Task OnGetEditarAsync(long id, CancellationToken cancellationToken)
    {
        try
        {
            var usuario = await _usuarioService.ObterPorIdAsync(id, cancellationToken);
            PreencherFormulario(usuario);
            MensagemAcao = $"Editando usuário #{usuario.Id}.";
            TipoMensagemAcao = "sucesso";
        }
        catch (BusinessException ex)
        {
            MensagemAcao = ObterMensagemErro(ex);
            TipoMensagemAcao = "erro";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao carregar usuário para edição na área privada.");
            MensagemAcao = "Erro interno ao carregar usuário para edição.";
            TipoMensagemAcao = "erro";
        }

        await CarregarListaAsync(cancellationToken);
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
                MensagemAcao = $"Usuário #{Formulario.Id.Value} atualizado com sucesso.";
            }
            else
            {
                var request = new CriarUsuarioRequest(
                    Formulario.Nome?.Trim() ?? string.Empty,
                    Formulario.Sobrenome?.Trim() ?? string.Empty,
                    Formulario.Email?.Trim() ?? string.Empty,
                    genero,
                    Formulario.DataNascimento);

                var criado = await _usuarioService.CriarAsync(request, cancellationToken);
                MensagemAcao = $"Usuário #{criado.Id} cadastrado com sucesso.";
            }

            TipoMensagemAcao = "sucesso";
            Formulario = new AdminUsuarioFormularioModel();
        }
        catch (BusinessException ex)
        {
            MensagemAcao = ObterMensagemErro(ex);
            TipoMensagemAcao = "erro";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao salvar usuário na área privada.");
            MensagemAcao = "Erro interno ao salvar usuário.";
            TipoMensagemAcao = "erro";
        }

        await CarregarListaAsync(cancellationToken);
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
            _logger.LogError(ex, "Erro inesperado ao buscar usuário na área privada.");
            MensagemBusca = "Erro interno ao buscar usuário.";
        }

        await CarregarListaAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostRemoverAsync(long id, CancellationToken cancellationToken)
    {
        try
        {
            await _usuarioService.RemoverAsync(id, cancellationToken);
            MensagemLista = $"Usuário #{id} removido com sucesso.";
            TipoMensagemLista = "sucesso";

            if (Formulario.Id == id)
                Formulario = new AdminUsuarioFormularioModel();
        }
        catch (BusinessException ex)
        {
            MensagemLista = ObterMensagemErro(ex);
            TipoMensagemLista = "erro";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao remover usuário na área privada.");
            MensagemLista = "Erro interno ao remover usuário.";
            TipoMensagemLista = "erro";
        }

        await CarregarListaAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostSairAsync()
    {
        await HttpContext.SignOutAsync();
        return RedirectToPage("/Admin/Login");
    }

    public string FormatarData(DateTime? data)
    {
        return data?.ToString("dd/MM/yyyy") ?? "-";
    }

    public string FormatarDataHora(DateTime data)
    {
        return data.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
    }

    private async Task CarregarListaAsync(CancellationToken cancellationToken)
    {
        try
        {
            NormalizarPaginacao();

            Usuarios = await _usuarioService.ListarAsync(
                new ListarUsuariosRequest
                {
                    Pagina = Pagina,
                    TamanhoPagina = TamanhoPagina,
                    Nome = FiltroNome,
                    Email = FiltroEmail
                },
                cancellationToken);
        }
        catch (BusinessException ex)
        {
            Usuarios = new PagedResponse<UsuarioResponse>(
                Enumerable.Empty<UsuarioResponse>(),
                Math.Max(Pagina, 1),
                Math.Clamp(TamanhoPagina, 1, 100),
                0,
                0);

            MensagemLista = ObterMensagemErro(ex);
            TipoMensagemLista = "erro";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao carregar tabela privada.");
            MensagemLista = "Erro interno ao carregar tabela.";
            TipoMensagemLista = "erro";
        }
    }

    private void NormalizarPaginacao()
    {
        if (Pagina < 1)
            Pagina = 1;

        if (TamanhoPagina < 1)
            TamanhoPagina = 10;

        if (TamanhoPagina > 100)
            TamanhoPagina = 100;
    }

    private void PreencherFormulario(UsuarioResponse usuario)
    {
        Formulario = new AdminUsuarioFormularioModel
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

public class AdminUsuarioFormularioModel
{
    public long? Id { get; set; }
    public string? Nome { get; set; }
    public string? Sobrenome { get; set; }
    public string? Email { get; set; }
    public string? Genero { get; set; }
    public DateTime? DataNascimento { get; set; }
}
