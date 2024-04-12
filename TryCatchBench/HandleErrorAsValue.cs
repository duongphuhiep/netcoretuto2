using DotNext;

namespace TryCatchBench;

public static class HandleErrorAsValue
{
    public static Result<int> Run()
    {
        var r = Fx2();
        if (!r.IsSuccessful)
            return new(Error.Create("Fx1", r.Error));
        return r;
    }
    private static Result<int> Fx2()
    {
        var r = Fx3();
        if (!r.IsSuccessful)
            return new(Error.Create("Fx2", r.Error));
        return r;
    }
    private static Result<int> Fx3()
    {
        var r = Fx4();
        if (!r.IsSuccessful)
            return new(Error.Create("Fx3", r.Error));
        return r;
    }
    private static Result<int> Fx4()
    {
        var r = Fx5();
        if (!r.IsSuccessful)
            return new(Error.Create("Fx4", r.Error));
        return r;
    }
    private static Result<int> Fx5()
    {
        Exception ex = Error.Create("Fx5");
        return new(ex);
    }
}
