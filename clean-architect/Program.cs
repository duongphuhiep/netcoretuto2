using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace poc.client
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                MediatorDemo.Play001Async().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        
    }
}
