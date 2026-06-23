using FluentValidation;
using UsuariosAPI.Application.DTOs.Requests;
using UsuariosAPI.Domain.Enums;

namespace UsuariosAPI.Application.Validators;

public class CriarUsuarioValidator : AbstractValidator<CriarUsuarioRequest>
{
    public CriarUsuarioValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Sobrenome)
            .NotEmpty().WithMessage("Sobrenome é obrigatório.")
            .MaximumLength(100).WithMessage("Sobrenome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(150).WithMessage("E-mail deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Genero)
            .IsInEnum().WithMessage($"Gênero inválido. Valores aceitos: {string.Join(", ", Enum.GetNames<Genero>())}.");

        RuleFor(x => x.DataNascimento)
            .LessThanOrEqualTo(DateTime.Today)
            .When(x => x.DataNascimento.HasValue)
            .WithMessage("Data de nascimento não pode ser no futuro.");
    }
}

public class AtualizarUsuarioValidator : AbstractValidator<AtualizarUsuarioRequest>
{
    public AtualizarUsuarioValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Sobrenome)
            .NotEmpty().WithMessage("Sobrenome é obrigatório.")
            .MaximumLength(100).WithMessage("Sobrenome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório.")
            .EmailAddress().WithMessage("E-mail inválido.")
            .MaximumLength(150).WithMessage("E-mail deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Genero)
            .IsInEnum().WithMessage($"Gênero inválido. Valores aceitos: {string.Join(", ", Enum.GetNames<Genero>())}.");

        RuleFor(x => x.DataNascimento)
            .LessThanOrEqualTo(DateTime.Today)
            .When(x => x.DataNascimento.HasValue)
            .WithMessage("Data de nascimento não pode ser no futuro.");
    }
}

public class ListarUsuariosValidator : AbstractValidator<ListarUsuariosRequest>
{
    public ListarUsuariosValidator()
    {
        RuleFor(x => x.Pagina)
            .GreaterThanOrEqualTo(1).WithMessage("Página deve ser maior ou igual a 1.");

        RuleFor(x => x.TamanhoPagina)
            .InclusiveBetween(1, 100).WithMessage("Tamanho da página deve ser entre 1 e 100.");

        RuleFor(x => x.Nome)
            .MaximumLength(100).WithMessage("Filtro de nome deve ter no máximo 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Nome));

        RuleFor(x => x.Email)
            .MaximumLength(150).WithMessage("Filtro de e-mail deve ter no máximo 150 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
