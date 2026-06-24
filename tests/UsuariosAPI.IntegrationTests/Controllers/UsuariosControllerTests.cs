using Xunit;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UsuariosAPI.Application.DTOs.Requests;
using UsuariosAPI.Application.DTOs.Responses;
using UsuariosAPI.Domain.Enums;
using UsuariosAPI.Infrastructure.Data.Context;

namespace UsuariosAPI.IntegrationTests.Controllers;

public class UsuariosControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UsuariosControllerTests(WebApplicationFactory<Program> factory)
    {
        var databaseName = "TestDb_" + Guid.NewGuid();

        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureLogging(logging => logging.ClearProviders());

            builder.ConfigureServices(services =>
            {
                // Substitui o banco real por SQLite in-memory para testes
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(databaseName));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task SITE_Raiz_ExibeTelaCrudComCabecalhosDeSeguranca()
    {
        var response = await _client.GetAsync("/site");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Contains("default-src 'self'", response.Headers.GetValues("Content-Security-Policy").Single());

        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Gerenciamento de", html);
        Assert.Contains("id=\"form-usuario\"", html);
        Assert.Contains("id=\"form-busca-id\"", html);
        Assert.Contains("Área privada", html);
        Assert.DoesNotContain("id=\"usuarios-tabela\"", html);
    }

    [Fact]
    public async Task GET_Usuarios_Retorna200()
    {
        var response = await _client.GetAsync("/api/v1/usuarios");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task POST_Usuarios_DadosValidos_Retorna201()
    {
        var request = new CriarUsuarioRequest(
            "Victor", "Teste", "victor@integ.com", Genero.Masculino, new DateTime(1998, 3, 20));

        var response = await _client.PostAsJsonAsync("/api/v1/usuarios", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<UsuarioResponse>>();
        Assert.NotNull(body);
        Assert.True(body!.Sucesso);
        Assert.Equal("Victor", body.Dados!.Nome);
    }

    [Fact]
    public async Task POST_Usuarios_EmailDuplicado_Retorna409()
    {
        var request = new CriarUsuarioRequest(
            "Ana", "Dup", "dup@integ.com", Genero.Feminino, null);

        await _client.PostAsJsonAsync("/api/v1/usuarios", request);
        var response = await _client.PostAsJsonAsync("/api/v1/usuarios", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.False(body!.Sucesso);
        Assert.False(string.IsNullOrWhiteSpace(body.TraceId));
    }

    [Fact]
    public async Task GET_Usuarios_IdInexistente_Retorna404()
    {
        var response = await _client.GetAsync("/api/v1/usuarios/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.False(string.IsNullOrWhiteSpace(body!.TraceId));
    }

    [Fact]
    public async Task PUT_Usuarios_AtualizaDados_Retorna200()
    {
        // Cria
        var create = new CriarUsuarioRequest("Put", "Teste", "put@integ.com", Genero.Outro, null);
        var postResp = await _client.PostAsJsonAsync("/api/v1/usuarios", create);
        var created = await postResp.Content.ReadFromJsonAsync<ApiResponse<UsuarioResponse>>();

        // Atualiza
        var update = new AtualizarUsuarioRequest("Atualizado", "Sobrenome", "put@integ.com", Genero.Outro, null);
        var putResp = await _client.PutAsJsonAsync($"/api/v1/usuarios/{created!.Dados!.Id}", update);

        Assert.Equal(HttpStatusCode.OK, putResp.StatusCode);
        var updated = await putResp.Content.ReadFromJsonAsync<ApiResponse<UsuarioResponse>>();
        Assert.Equal("Atualizado", updated!.Dados!.Nome);
    }

    [Fact]
    public async Task DELETE_Usuarios_UsuarioExistente_Retorna204()
    {
        var create = new CriarUsuarioRequest("Del", "User", "del@integ.com", Genero.Feminino, null);
        var postResp = await _client.PostAsJsonAsync("/api/v1/usuarios", create);
        var created = await postResp.Content.ReadFromJsonAsync<ApiResponse<UsuarioResponse>>();

        var deleteResp = await _client.DeleteAsync($"/api/v1/usuarios/{created!.Dados!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);
    }

    [Fact]
    public async Task POST_Usuarios_JsonMalformado_Retorna400Padronizado()
    {
        using var content = new StringContent("{ nome: ", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/v1/usuarios", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.NotNull(body);
        Assert.False(body!.Sucesso);
        Assert.NotEmpty(body.Erros!);
        Assert.False(string.IsNullOrWhiteSpace(body.TraceId));
    }

    [Fact]
    public async Task POST_Usuarios_EmailInvalido_Retorna422ComErros()
    {
        var request = new CriarUsuarioRequest(
            "Ana", "Teste", "email-invalido", Genero.Feminino, null);

        var response = await _client.PostAsJsonAsync("/api/v1/usuarios", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.Contains("E-mail inválido.", body!.Erros!);
        Assert.False(string.IsNullOrWhiteSpace(body.TraceId));
    }

    [Fact]
    public async Task GET_Usuarios_PaginacaoInvalida_Retorna422()
    {
        var response = await _client.GetAsync("/api/v1/usuarios?pagina=0&tamanhoPagina=101");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.Equal(2, body!.Erros!.Count());
    }

    [Fact]
    public async Task GET_Usuarios_IdZero_Retorna422()
    {
        var response = await _client.GetAsync("/api/v1/usuarios/0");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.Contains("Id do usuário deve ser maior que zero.", body!.Erros!);
    }
}
