using DotNext;
using Xunit.Abstractions;

namespace Experiment.Xunit;
public class StackTraceTests(ITestOutputHelper testOutputHelper)
{
    #region Fx
    private Result<int> Fx1()
    {
        var r = Fx2();
        if (!r.IsSuccessful)
            return new(ExceptionFactory.Create("Fx1", r.Error));
        return 1;
    }
    private Result<int> Fx2()
    {
        var r = Fx3();
        if (!r.IsSuccessful)
            return new(ExceptionFactory.Create("Fx2", r.Error));
        return 1;
    }
    private Result<int> Fx3()
    {
        Exception ex = ExceptionFactory.Create("Fx3");
        return new(ex);
    }

    [Fact]
    public void TestFx()
    {
        var r = Fx1();
        testOutputHelper.WriteLine(r.Display());
    }
    #endregion


}
