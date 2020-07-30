using Microsoft.Extensions.DependencyInjection;
using System;

namespace poc.client
{
    internal static class DepsInjSingleton
    {
        public interface IA
        {
            void ha();
        }

        public class A : IA
        {
            public A()
            {
            }

            public void ha()
            {
                Console.WriteLine("ha");
            }
        }

        public class B
        {
            private readonly IA a;

            public B(IA a)
            {
                this.a = a;
            }

            public void hb()
            {
                Console.WriteLine("hb");
                a.ha();
            }
        }

        static public void Play()
        {
            IServiceCollection con = new ServiceCollection();
            con.AddSingleton(typeof(IA), typeof(A));
            con.AddSingleton(typeof(B));
            IServiceProvider sp = con.BuildServiceProvider(validateScopes: true);
            B b = sp.GetService<B>();
            b.hb();
        }
    }
}