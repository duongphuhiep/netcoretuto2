using GraphQL;
using GraphQL.Types;

namespace demo003.graphql.GraphTypeFirst
{
    public class Droid
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class DroidType : ObjectGraphType<Droid>
    {
        public DroidType()
        {
            Field(x => x.Id).Description("The Id of the Droid.");
            Field(x => x.Name).Description("The name of the Droid.");
        }
    }

    public class StarWarsQuery : ObjectGraphType
    {
        public StarWarsQuery()
        {
            Field<DroidType>(
              "hero",
              arguments: new QueryArguments(new QueryArgument<IdGraphType> { Name = "id" }),
              resolve: context =>
              {
                  var id = context.GetArgument<string>("id");
                  return new Droid { Id = id, Name = "R2-D2" };
              }
            );
        }
    }
}
