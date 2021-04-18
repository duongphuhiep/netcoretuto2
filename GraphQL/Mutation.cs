using ConferencePlanner.GraphQL.Data;
using HotChocolate;
using System.Threading.Tasks;

namespace ConferencePlanner.GraphQL
{
    public class Mutation
    {
        public async Task<AddSpeakerPayload> AddSpeakerAsync(
            AddSpeakerInput input,
            [Service] ApplicationDbContext context)
        {
            var speaker = new Speaker
            {
                Name = input.Name,
                Bio = input.Bio,
                WebSite = input.WebSite
            };

            context.Speakers.Add(speaker);
            await context.SaveChangesAsync().ConfigureAwait(false);

            return new AddSpeakerPayload(speaker);
        }
    }
}
