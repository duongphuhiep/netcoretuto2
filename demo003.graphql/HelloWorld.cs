using GraphQL;
using GraphQL.NewtonsoftJson;
using GraphQL.Types;
using System;
using System.Threading.Tasks;

namespace demo003.graphql
{
    public static class HelloWorld
    {
        public static async Task Run()
        {
            var schema = Schema.For(@"
              type Query {
                hello: String
              }
            ");

            var json = await schema.ExecuteAsync(_ =>
            {
                _.Query = "{ hello }";
                _.Root = new { Hello = "Hello World!" };
            }).ConfigureAwait(false);

            Console.WriteLine(json);
        }
    }
}
