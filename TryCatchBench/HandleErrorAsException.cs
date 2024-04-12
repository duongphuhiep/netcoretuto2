namespace TryCatchBench;

public static class HandleErrorAsException
{
    public static int Run()
    {
        try
        {
            var r = Fx2();
            return r;
        }
        catch (Exception ex)
        {
            throw new Exception("Fx1", ex);
        }
    }
    private static int Fx2()
    {
        try
        {
            var r = Fx3();
            return r;
        }
        catch (Exception ex)
        {
            throw new Exception("Fx2", ex);
        }
    }
    private static int Fx3()
    {
        try
        {
            var r = Fx4();
            return r;
        }
        catch (Exception ex)
        {
            throw new Exception("Fx3", ex);
        }
    }
    private static int Fx4()
    {
        try
        {
            var r = Fx5();
            return r;
        }
        catch (Exception ex)
        {
            throw new Exception("Fx4", ex);
        }
    }
    private static int Fx5()
    {
        throw new Exception("Fx5");
    }
}
