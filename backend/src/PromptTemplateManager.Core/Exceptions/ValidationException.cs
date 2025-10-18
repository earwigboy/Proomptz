namespace PromptTemplateManager.Core.Exceptions;

public class ValidationException : DomainException
{
    public IEnumerable<string> Errors { get; }

    public ValidationException(string message, IEnumerable<string> errors)
        : base(message)
    {
        Errors = errors;
    }

    public ValidationException(string message) : base(message)
    {
        Errors = new List<string>();
    }
}
