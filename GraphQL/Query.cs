using ConferencePlanner.GraphQL.Data;
using HotChocolate;
using System.Linq;

namespace ConferencePlanner.GraphQL
{
    public class Query
    {
        public IQueryable<Speaker> GetSpeakers([Service] ApplicationDbContext context) =>
            context.Speakers;
    }
}
