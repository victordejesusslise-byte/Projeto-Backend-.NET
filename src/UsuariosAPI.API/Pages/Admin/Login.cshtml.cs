using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UsuariosAPI.API.Pages.Admin;

public class LoginModel : PageModel
{
    private readonly IConfiguration _configuration;

    public LoginModel(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [BindProperty]
    public string? Senha { get; set; }

    public string? Mensagem { get; private set; }

    public IActionResult OnGet()
    {
        return User.Identity?.IsAuthenticated == true
            ? RedirectToPage("/Admin/Tabela")
            : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var senhaConfigurada = _configuration["ADMIN_PANEL_PASSWORD"];

        if (string.IsNullOrWhiteSpace(senhaConfigurada))
        {
            Mensagem = "A senha da area privada nao foi configurada no .env.";
            return Page();
        }

        if (string.IsNullOrWhiteSpace(Senha) || !SenhaConfere(Senha, senhaConfigurada))
        {
            Mensagem = "Senha invalida.";
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "Administrador"),
            new(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            });

        return RedirectToPage("/Admin/Tabela");
    }

    private static bool SenhaConfere(string senhaInformada, string senhaConfigurada)
    {
        var hashInformado = SHA256.HashData(Encoding.UTF8.GetBytes(senhaInformada));
        var hashConfigurado = SHA256.HashData(Encoding.UTF8.GetBytes(senhaConfigurada));
        return CryptographicOperations.FixedTimeEquals(hashInformado, hashConfigurado);
    }
}
