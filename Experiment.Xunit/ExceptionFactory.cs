using DotNext;
using System.Runtime.CompilerServices;
using System.Text;

namespace Experiment.Xunit;

public static class ExceptionFactory
{
    private static string CodeLocation(string? filePath, int? lineNumber, string? memberName) => $"{memberName} in {filePath} line:{lineNumber}";

    public static Exception Create(string message, Exception? innerException = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int? lineNumber = null,
        [CallerMemberName] string? memberName = null
        )
    {
        var ex = new Exception($"{message}", innerException);
        ex.Source = CodeLocation(filePath, lineNumber, memberName);
        return ex;
    }

    public static string Display(this Exception ex)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(ex.Message);
        if (ex.Source is not null)
        {
            sb.Append($" //at {ex.Source}");
        }
        if (ex.InnerException is not null)
        {
            sb.AppendLine();
            sb.Append(" ---> ");
            sb.Append(ex.InnerException.Display());
        }
        return sb.ToString();
    }

    public static string Display<T>(this Result<T> result) => result.Error?.Display() ?? result.Value?.ToString() ?? "<NULL>";
}
