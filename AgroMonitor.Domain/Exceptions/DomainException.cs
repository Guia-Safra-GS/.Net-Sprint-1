namespace AgroMonitor.Domain.Exceptions;

/// <summary>
/// Exceção de regra de negócio do domínio. É traduzida para HTTP 400
/// pelo tratador global de exceções da API.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
