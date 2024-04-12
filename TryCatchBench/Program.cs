using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using TryCatchBench;

var summary = BenchmarkRunner.Run<MyBenchmark>();


[MemoryDiagnoser]
public class MyBenchmark
{
    [Benchmark]
    public void Run_HandleExceptionSimple()
    {
        try
        {
            var r = HandleExceptionSimple.Run();
        }
        catch (Exception ex) { }
    }
    [Benchmark]
    public void Run_HandleErrorAsException()
    {
        try
        {
            var r = HandleErrorAsException.Run();
        }
        catch (Exception ex) { }
    }
    [Benchmark]
    public void Run_HandleErrorAsValue()
    {
        var r = HandleErrorAsValue.Run();
    }

    [Benchmark]
    public void Run_HandleExceptionSimple_AndGetStacktrace()
    {
        try
        {
            var r = HandleExceptionSimple.Run();
        }
        catch (Exception ex)
        {
            var s = ex.ToString();
        }
    }
    [Benchmark]
    public void Run_HandleErrorAsException_AndGetStacktrace()
    {
        try
        {
            var r = HandleErrorAsException.Run();
        }
        catch (Exception ex)
        {
            var s = ex.ToString();
        }
    }
    [Benchmark(Baseline = true)]
    public void Run_HandleErrorAsValue_AndGetStacktrace()
    {
        var r = HandleErrorAsValue.Run();
        var s = r.Display();
    }
}