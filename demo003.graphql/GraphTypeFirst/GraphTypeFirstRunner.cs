using GraphQL.NewtonsoftJson;
using GraphQL.Types;
using System;
using System.Threading.Tasks;

namespace demo003.graphql.GraphTypeFirst
{
    public static class GraphTypeFirstRunner
    {
        public static async Task Run()
        {
            using (var schema = new Schema { Query = new StarWarsQuery() })
            {
                var json = await schema.ExecuteAsync(_ =>
                {
                    _.Query = @"
                        { 
                            two: hero(id:""2"") { 
                                id 
                                name 
                            },
                            three: hero(id:""3"") { 
                                id 
                                name 
                            },
                        }";
                }).ConfigureAwait(false);

                Console.WriteLine(json);
            }
        }
    }
}
