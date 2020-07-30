using Microsoft.Extensions.DependencyInjection;
using System;

namespace poc.client
{
    internal static class DepsInjCircular
    {
        public interface IA
        {
            void ha();
        }

        public interface IB
        {
            void hb();
        }

        public class A : IA
        {
            private readonly IB b;

            public A(IB b)
            {
                this.b = b;
            }

            public void ha()
            {
                Console.WriteLine("ha");
                b.hb();
            }
        }

        public class B : IB
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
            con.AddSingleton(typeof(IB), typeof(B));
            IServiceProvider sp = con.BuildServiceProvider(validateScopes: true);
            IA a = sp.GetService<IA>();
            a.ha();
        }
    }
}