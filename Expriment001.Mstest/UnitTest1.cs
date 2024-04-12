namespace Expriment001.Mstest;

[TestClass]
public class UnitTest1
{
    [ClassInitialize]
    public static void MySetup(TestContext testContext)
    {
        Console.WriteLine($"class init {testContext.TestName}");
        testContext.Properties.Add("myNum", 10);
    }

    private int a = 1;
    public UnitTest1()
    {
        Console.WriteLine("constructor");
        a = 3;
    }
    [TestInitialize]
    public void Setup()
    {
        Console.WriteLine("setup");
        a = 2;
    }
    [TestMethod]
    public void TestMethod1(TestContext testContext)
    {
        Console.WriteLine("test 1");
        Assert.AreEqual(2, a);
        a += 5;
        Assert.AreEqual(7, a);

        Assert.Equals(10, testContext.Properties["myNum"]);
    }
    [TestMethod]
    public void TestMethod2(TestContext testContext)
    {
        Console.WriteLine("test 2");
        Assert.AreEqual(2, a);
        a += 5;
        Assert.AreEqual(7, a);
        Assert.Equals(10, testContext.Properties["myNum"]);
    }
}