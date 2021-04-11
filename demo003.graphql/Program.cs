using demo003.graphql.GraphTypeFirst;
using System.Threading.Tasks;

namespace demo003.graphql
{
    static class Program
    {
        public static async Task Main(string[] args)
        {
            await GraphTypeFirstRunner.Run().ConfigureAwait(false);
        }
    }
}
