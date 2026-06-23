using Xunit;
using Moq;
using UsuariosAPI.Application.DTOs.Requests;
using UsuariosAPI.Application.Exceptions;
using UsuariosAPI.Application.Services;
using UsuariosAPI.Application.Validators;
using UsuariosAPI.Domain.Entities;
using UsuariosAPI.Domain.Enums;
using UsuariosAPI.Domain.Interfaces.Repositories;

namespace UsuariosAPI.UnitTests.Services;

public class UsuarioServiceTests
{
    private readonly Mock<IUsuarioRepository> _repoMock;
    private readonly UsuarioService _service;

    public UsuarioServiceTests()
    {
        _repoMock = new Mock<IUsuarioRepository>();
        _service = new UsuarioService(
            _repoMock.Object,
            new CriarUsuarioValidator(),
            new AtualizarUsuarioValidator(),
            new ListarUsuariosValidator());
    }

    // ---------------------------------------------------------------
    // ObterPorId
    // ---------------------------------------------------------------

    [Fact]
    public async Task ObterPorId_UsuarioExistente_RetornaResponse()
    {
        var usuario = new Usuario("João", "Silva", "joao@email.com", Genero.Masculino, new DateTime(1990, 1, 1));
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default)).ReturnsAsync(usuario);

        var result = await _service.ObterPorIdAsync(1);

        Assert.Equal("João", result.Nome);
        Assert.Equal("joao@email.com", result.Email);
    }

    [Fact]
    public async Task ObterPorId_UsuarioNaoExistente_LancaNotFoundException()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(999, default)).ReturnsAsync((Usuario?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.ObterPorIdAsync(999));
    }

    // ---------------------------------------------------------------
    // Criar
    // ---------------------------------------------------------------

    [Fact]
    public async Task Criar_DadosValidos_RetornaUsuarioCriado()
    {
        var request = new CriarUsuarioRequest("Maria", "Souza", "maria@email.com", Genero.Feminino, null);

        _repoMock.Setup(r => r.EmailExisteAsync(request.Email, null, default)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AdicionarAsync(It.IsAny<Usuario>(), default))
                 .ReturnsAsync((Usuario u, CancellationToken _) => u);

        var result = await _service.CriarAsync(request);

        Assert.Equal("Maria", result.Nome);
        Assert.Equal("maria@email.com", result.Email);
        _repoMock.Verify(r => r.AdicionarAsync(It.IsAny<Usuario>(), default), Times.Once);
    }

    [Fact]
    public async Task Criar_EmailJaCadastrado_LancaConflictException()
    {
        var request = new CriarUsuarioRequest("João", "Dup", "dup@email.com", Genero.Masculino, null);
        _repoMock.Setup(r => r.EmailExisteAsync(request.Email, null, default)).ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.CriarAsync(request));
    }

    [Fact]
    public async Task Criar_EmailInvalido_LancaValidationException()
    {
        var request = new CriarUsuarioRequest("X", "Y", "email-invalido", Genero.Masculino, null);

        await Assert.ThrowsAsync<Application.Exceptions.ValidationException>(() => _service.CriarAsync(request));
    }

    [Fact]
    public async Task Criar_DataNascimentoFutura_LancaValidationException()
    {
        var request = new CriarUsuarioRequest(
            "X", "Y", "valido@email.com", Genero.Masculino,
            DateTime.Today.AddDays(1));

        await Assert.ThrowsAsync<Application.Exceptions.ValidationException>(() => _service.CriarAsync(request));
    }

    // ---------------------------------------------------------------
    // Atualizar
    // ---------------------------------------------------------------

    [Fact]
    public async Task Atualizar_UsuarioExistente_RetornaAtualizado()
    {
        var usuario = new Usuario("Old", "Name", "old@email.com", Genero.Masculino, null);
        var request = new AtualizarUsuarioRequest("Novo", "Nome", "novo@email.com", Genero.Masculino, null);

        _repoMock.Setup(r => r.ObterPorIdAsync(1, default)).ReturnsAsync(usuario);
        _repoMock.Setup(r => r.EmailExisteAsync(request.Email, 1, default)).ReturnsAsync(false);
        _repoMock.Setup(r => r.AtualizarAsync(It.IsAny<Usuario>(), default))
                 .ReturnsAsync((Usuario u, CancellationToken _) => u);

        var result = await _service.AtualizarAsync(1, request);

        Assert.Equal("Novo", result.Nome);
        Assert.Equal("novo@email.com", result.Email);
    }

    [Fact]
    public async Task Atualizar_UsuarioNaoExistente_LancaNotFoundException()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(999, default)).ReturnsAsync((Usuario?)null);
        var request = new AtualizarUsuarioRequest("A", "B", "a@b.com", Genero.Outro, null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.AtualizarAsync(999, request));
    }

    // ---------------------------------------------------------------
    // Remover
    // ---------------------------------------------------------------

    [Fact]
    public async Task Remover_UsuarioExistente_ChamaRemoverNoRepositorio()
    {
        var usuario = new Usuario("Del", "Me", "del@email.com", Genero.Feminino, null);
        _repoMock.Setup(r => r.ObterPorIdAsync(1, default)).ReturnsAsync(usuario);
        _repoMock.Setup(r => r.RemoverAsync(usuario, default)).Returns(Task.CompletedTask);

        await _service.RemoverAsync(1);

        _repoMock.Verify(r => r.RemoverAsync(usuario, default), Times.Once);
    }

    [Fact]
    public async Task Remover_UsuarioNaoExistente_LancaNotFoundException()
    {
        _repoMock.Setup(r => r.ObterPorIdAsync(42, default)).ReturnsAsync((Usuario?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.RemoverAsync(42));
    }
}
