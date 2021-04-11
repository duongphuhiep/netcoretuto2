using GraphQL;

namespace demo003.graphql.SchemaFirst
{
    public class Droid
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Query
    {
        [GraphQLMetadata("hero")]
        public Droid GetHero()
        {
            return new Droid { Id = "1", Name = "R2-D2" };
        }
    }
}
