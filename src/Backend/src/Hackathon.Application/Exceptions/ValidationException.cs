using FluentValidation.Results;

namespace Hackathon.Application.Exceptions;

public class ValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public ValidationException(IEnumerable<ValidationFailure> failures) : base(BuildErrorMessage(failures))
    {
        Errors = failures
        .GroupBy(f => f.PropertyName)
        .ToDictionary(
            d => d.Key,
            d => d.Select(e => e.ErrorMessage).ToArray()
        );
    }

    public static string BuildErrorMessage(IEnumerable<ValidationFailure> failures)
    {
        var errorArr = failures.Select(
            f => $"{Environment.NewLine} --{f.PropertyName}: {f.ErrorMessage} | Severity: {f.Severity}");
        return "ValidationFailed:" + string.Join(string.Empty, errorArr);
    }
}