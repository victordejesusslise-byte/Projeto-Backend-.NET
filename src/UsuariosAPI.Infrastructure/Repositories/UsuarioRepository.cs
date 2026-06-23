using Microsoft.EntityFrameworkCore;
using UsuariosAPI.Domain.Entities;
using UsuariosAPI.Domain.Interfaces.Repositories;
using UsuariosAPI.Infrastructure.Data.Context;

namespace UsuariosAPI.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<Usuario> Usuarios, int Total)> ObterTodosAsync(
        int pagina,
        int tamanhoPagina,
        string? nome = null,
        string? email = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Usuarios.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
            query = query.Where(u =>
                EF.Functions.Like(u.Nome, $"%{nome}%") ||
                EF.Functions.Like(u.Sobrenome, $"%{nome}%"));

        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(u => EF.Functions.Like(u.Email, $"%{email}%"));

        var total = await query.CountAsync(cancellationToken);

        var usuarios = await query
            .OrderBy(u => u.Nome)
            .ThenBy(u => u.Sobrenome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        return (usuarios, total);
    }

    public async Task<Usuario?> ObterPorIdAsync(long id, CancellationToken cancellationToken = default)
        => await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default)
        => await _context.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<Usuario> AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        await _context.Usuarios.AddAsync(usuario, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return usuario;
    }

    public async Task<Usuario> AtualizarAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync(cancellationToken);
        return usuario;
    }

    public async Task RemoverAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        _context.Usuarios.Remove(usuario);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> EmailExisteAsync(
        string email,
        long? ignorarId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Usuarios.AsNoTracking()
            .Where(u => u.Email == email);

        if (ignorarId.HasValue)
            query = query.Where(u => u.Id != ignorarId.Value);

        return await query.AnyAsync(cancellationToken);
    }
}
