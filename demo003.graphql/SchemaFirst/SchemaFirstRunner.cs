using GraphQL.NewtonsoftJson;
using GraphQL.Types;
using System;
using System.Threading.Tasks;

namespace demo003.graphql.SchemaFirst
{
    public static class SchemaFirstRunner
    {
        public static async Task Run()
        {
            var schema = Schema.For(@"
                  type Droid {
                    id: String!
                    name: String!
                  }

                  type Query {
                    hero: Droid
                  }
            ", _ =>
            {
                _.Types.Include<Query>();
            });

            var json = await schema.ExecuteAsync(_ =>
            {
                _.Query = "{ hero { id name } }";
            }).ConfigureAwait(false);

            Console.WriteLine(json);
        }
    }
}
