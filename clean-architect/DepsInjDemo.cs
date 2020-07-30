using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace poc.client
{
    public interface IWalletRepository
    {
        public int Id { get; }

        void WriteToDB();
    }

    public interface IWalletService
    {
        public int Id { get; }

        void LockAmount();
    }

    public class WalletRepository : IWalletRepository
    {
        private static int IdCounter;
        private readonly string connectionString;

        public WalletRepository(string connectionString)
        {
            this.connectionString = connectionString;
            Id = IdCounter++;
        }

        public int Id { get; }

        public void WriteToDB()
        {
            Console.WriteLine($"WalletRepository #{Id} call {connectionString}");
        }
    }

    public class WalletService : IWalletService
    {
        private static int IdCounter;
        private readonly IWalletRepository walletRepository;

        public WalletService(IWalletRepository walletRepository)
        {
            this.walletRepository = walletRepository;
            Id = IdCounter++;
        }

        public int Id { get; }

        public void LockAmount()
        {
            Console.WriteLine($"WalletService #{Id} LockAmount");
            walletRepository.WriteToDB();
        }
    }

    public static class DepsInjDemo
    {
        static string connectionString = "mydb";
        public static void MsSample()
        {
            try
            {
                IServiceProviderFactory<IServiceCollection> serviceProviderFactory = new DefaultServiceProviderFactory();
                IServiceCollection serviceCollection = new ServiceCollection();

                serviceCollection.AddScoped(typeof(IWalletRepository), s => new WalletRepository(connectionString));
                serviceCollection.AddScoped(typeof(IWalletService), typeof(WalletService));

                IServiceProvider serviceProvider = serviceProviderFactory.CreateServiceProvider(serviceCollection);

                serviceProvider.GetService<IWalletService>().LockAmount(); // the connectionString is by default: "mydb"

                Console.WriteLine("-------------");

                using (var scope = serviceProvider.CreateScope())
                {
                    connectionString = "hiep";
                    serviceProvider.GetService<IWalletService>().LockAmount(); // the connectionString won't change here: "mydb"
                    scope.ServiceProvider.GetService<IWalletService>().LockAmount(); // the connectionString is "hiep"
                    scope.ServiceProvider.GetService<IWalletService>().LockAmount(); // the connectionString is "hiep"
                }

                Console.WriteLine("-------------");

                using (var scope = serviceProvider.CreateScope())
                {
                    connectionString = "nhu";
                    serviceProvider.GetService<IWalletService>().LockAmount(); // the connectionString won't change here: "mydb"
                    scope.ServiceProvider.GetService<IWalletService>().LockAmount(); // the connectionString is "nhu"
                    scope.ServiceProvider.GetService<IWalletService>().LockAmount(); // the connectionString is "nhu"
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public static void AutofacSample()
        {
            try
            {
                IServiceProviderFactory<ContainerBuilder> serviceProviderFactory = new AutofacServiceProviderFactory();

                #region Register all implementations / objects in the dependency graph

                IServiceCollection serviceCollection = new ServiceCollection();
                serviceCollection.AddScoped(typeof(IWalletRepository), svp => new WalletRepository("LocalDB_connection_string"));
                //serviceCollection.AddTransient(typeof(IWalletService), typeof(WalletService));

                #endregion Register all implementations / objects in the dependency graph

                ContainerBuilder containerBuilder = serviceProviderFactory.CreateBuilder(serviceCollection);
                // via assembly scan
                containerBuilder.RegisterAssemblyTypes(typeof(WalletService).GetTypeInfo().Assembly)
                    .Except<WalletRepository>()
                    .AsImplementedInterfaces();

                // or individually
                //containerBuilder.RegisterType<MyHandler>().AsImplementedInterfaces().InstancePerDependency();

                IServiceProvider serviceProvider = serviceProviderFactory.CreateServiceProvider(containerBuilder);

                //from here it is exactly the same as the above example

                using (var controller01Scope = serviceProvider.CreateScope())
                {
                    IServiceProvider scopedServiceProvider = controller01Scope.ServiceProvider;
                    {
                        IWalletService walletService = scopedServiceProvider.GetService<IWalletService>();
                        walletService.LockAmount();
                    }
                    {
                        IWalletService walletService = scopedServiceProvider.GetService<IWalletService>();
                        walletService.LockAmount();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}