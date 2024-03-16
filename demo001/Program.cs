using Microsoft.Extensions.Logging;

namespace demo001;

class Program
{
    static void Main(string[] args)
    {
        Global.Log.LogInformation("Hello, world");
        Console.ReadLine();
    }
}
