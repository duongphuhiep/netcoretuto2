using System;

namespace poc.client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                //MediatorDemo.Play001Async().Wait();
                //DepsInjSingleton.Play();
                DepsInjDemo.MsSample();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}