using System.Runtime.CompilerServices;
using System.Text;

namespace Experiment.Xunit;

public struct Error
{
    public string Message { get; private set; }
    public string? FilePath { get; private set; }
    public int? LineNumber { get; private set; }
    public string? MemberName { get; private set; }
    public string? StackTrace { get; private set; }

    public static Error New(string message, Error? innerError = null,
        [CallerFilePath] string? filePath = null,
        [CallerLineNumber] int? lineNumber = null,
        [CallerMemberName] string? memberName = null)
        => new Error
        {
            Message = message,
            StackTrace = innerError.ToString(),
            FilePath = filePath,
            LineNumber = lineNumber,
            MemberName = memberName,
        };
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder(Message);
        sb.AppendLine($" //at {MemberName} in {FilePath} line:{LineNumber}");
        sb.Append(" ---> ");
        sb.Append(StackTrace);
        return sb.ToString();
    }
}