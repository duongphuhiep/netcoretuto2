using DotNext;
using System.Runtime.CompilerServices;
using System.Text;

namespace TryCatchBench;

public static class Error
{
    public static Exception Create(string message, Exception? innerException = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int? lineNumber = null,
        [CallerMemberName] string? memberName = null
        )
    {
        var ex = new Exception($"{message}", innerException);
        ex.Source = $"{memberName} in {filePath} line:{lineNumber}";
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
