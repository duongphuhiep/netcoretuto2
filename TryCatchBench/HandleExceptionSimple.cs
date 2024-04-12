namespace TryCatchBench;

public static class HandleExceptionSimple
{
    public static int Run()
    {
        var r = Fx2();
        return r;
    }
    private static int Fx2()
    {
        var r = Fx3();
        return r;
    }
    private static int Fx3()
    {
        var r = Fx4();
        return r;
    }
    private static int Fx4()
    {
        var r = Fx5();
        return r;
    }
    private static int Fx5()
    {
        throw new Exception("Fx5");
    }
}
