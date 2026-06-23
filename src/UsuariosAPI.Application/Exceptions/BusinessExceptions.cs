namespace UsuariosAPI.Application.Exceptions;

/// <summary>Exceção base para erros de negócio (4xx).</summary>
public abstract class BusinessException : Exception
{
    public int StatusCode { get; }

    protected BusinessException(string mensagem, int statusCode = 400)
        : base(mensagem)
    {
        StatusCode = statusCode;
    }
}

/// <summary>Recurso não encontrado (404).</summary>
public class NotFoundException : BusinessException
{
    public NotFoundException(string recurso, object id)
        : base($"{recurso} com id '{id}' não foi encontrado.", 404) { }
}

/// <summary>Conflito de dados (409).</summary>
public class ConflictException : BusinessException
{
    public ConflictException(string mensagem)
        : base(mensagem, 409) { }
}

/// <summary>Dados inválidos (422).</summary>
public class ValidationException : BusinessException
{
    public IEnumerable<string> Erros { get; }

    public ValidationException(IEnumerable<string> erros)
        : base("Um ou mais erros de validação ocorreram.", 422)
    {
        Erros = erros;
    }
}
