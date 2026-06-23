using Xunit;
using UsuariosAPI.Application.DTOs.Requests;
using UsuariosAPI.Application.Validators;
using UsuariosAPI.Domain.Enums;
namespace UsuariosAPI.UnitTests.Validators;

public class CriarUsuarioValidatorTests
{
    private readonly CriarUsuarioValidator _validator = new();

    [Fact]
    public async Task Validar_DadosCompletos_PassaSemErros()
    {
        var request = new CriarUsuarioRequest("Ana", "Lima", "ana@teste.com", Genero.Feminino, new DateTime(1995, 6, 15));
        var result = await _validator.ValidateAsync(request);
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Validar_NomeVazio_RetornaErro(string nome)
    {
        var request = new CriarUsuarioRequest(nome, "Lima", "ana@teste.com", Genero.Feminino, null);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Nome");
    }

    [Fact]
    public async Task Validar_EmailInvalido_RetornaErro()
    {
        var request = new CriarUsuarioRequest("Ana", "Lima", "nao-e-email", Genero.Feminino, null);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public async Task Validar_DataNascimentoFutura_RetornaErro()
    {
        var request = new CriarUsuarioRequest(
            "Ana", "Lima", "ana@teste.com", Genero.Feminino,
            DateTime.Today.AddDays(1));
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "DataNascimento");
    }

    [Fact]
    public async Task Validar_NomeMuitoLongo_RetornaErro()
    {
        var request = new CriarUsuarioRequest(
            new string('A', 101), "Lima", "ana@teste.com", Genero.Feminino, null);
        var result = await _validator.ValidateAsync(request);
        Assert.False(result.IsValid);
    }
}
