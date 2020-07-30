using Microsoft.Extensions.DependencyInjection;
using System;

namespace poc.client
{
    public interface INameId
    {
        public Type RuntimeType { get; }
        public int Id { get; }
    }

    public interface IFoo : INameId
    {
        string DescribeFoo();
    }

    public interface IBar : INameId
    {
        string DescribeBar();
    }

    public class Foo1 : IFoo
    {
        private static int IdCounter;

        public Foo1()
        {
            Id = IdCounter++;
        }

        public Type RuntimeType => typeof(Foo1);
        public int Id { get; }

        public string DescribeFoo() => $"Run {RuntimeType.Name} #{Id}";
    }

    public class Foo2 : IFoo
    {
        private static int IdCounter;

        public Foo2()
        {
            Id = IdCounter++;
        }

        public Type RuntimeType => typeof(Foo2);
        public int Id { get; }

        public string DescribeFoo() => $"Run {RuntimeType.Name} #{Id}";
    }

    public class Bar1 : IBar
    {
        private static int IdCounter;

        public Bar1()
        {
            Id = IdCounter++;
        }

        public Type RuntimeType => typeof(Bar1);
        public int Id { get; }

        public string DescribeBar() => $"Run {RuntimeType.Name} #{Id}";
    }

    public class FooBar1 : IFoo, IBar
    {
        private static int IdCounter;

        public FooBar1()
        {
            Id = IdCounter++;
        }

        public Type RuntimeType => typeof(FooBar1);
        public int Id { get; }

        public string DescribeBar() => $"Run Bar in {RuntimeType.Name} #{Id}";

        public string DescribeFoo() => $"Run Foo in {RuntimeType.Name} #{Id}";
    }

    public static class MsDepsInjDemo
    {
        public static void ScrutonSample01()
        {
            IServiceProviderFactory<IServiceCollection> serviceProviderFactory = new DefaultServiceProviderFactory();

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.Scan(scan => scan.FromCallingAssembly()
                .AddClasses(classes => classes.AssignableTo<IBar>())
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            );

            IServiceProvider serviceProvider = serviceProviderFactory.CreateServiceProvider(serviceCollection);

            foreach (var s in serviceProvider.GetServices<IFoo>())
            {
                Console.WriteLine(s.DescribeFoo());
            }
            foreach (var s in serviceProvider.GetServices<IFoo>())
            {
                Console.WriteLine(s.DescribeFoo());
            }

            foreach (var s in serviceProvider.GetServices<IBar>())
            {
                Console.WriteLine(s.DescribeBar());
            }
            foreach (var s in serviceProvider.GetServices<IBar>())
            {
                Console.WriteLine(s.DescribeBar());
            }

            //using (var controller01Scope = serviceProvider.CreateScope())
            //{
            //    IWalletService walletService = controller01Scope.ServiceProvider.GetService<IWalletService>();
            //    walletService.LockAmount();
            //}
        }
    }
}