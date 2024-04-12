using DotNext;
using Xunit.Abstractions;

namespace Experiment.Xunit;

public class UnitTest1(ITestOutputHelper testOutputHelper)
{

    [Fact]
    public void Test1()
    {
        Exception ex = new Exception("abcd");

        Func<string, int> parser = int.Parse;
        Result<int> result = parser.TryInvoke("42");
        if (result)  //successful
        {
            var i = (int)result;
        }
        else
        {
            throw result.Error;
        }
    }

    public Result<int> Divide(int n, int d)
    {
        if (d == 0)
        {
            return new(ExceptionFactory.Create("Divided by 0"));
        }
        return n / d;
    }

    public Result<int> DivideThenAddTo(int n, int d, int a)
    {
        var resu = Divide(n, d);
        if (!resu.IsSuccessful)
        {
            return new(ExceptionFactory.Create($"Failed DivideThenAddTo {a}", resu.Error));
        }
        return resu.Value + a;
    }

    [Fact]
    public void Test2()
    {
        var result = DivideThenAddTo(4, 0, 2);
        testOutputHelper.WriteLine(result.Display());
    }

}