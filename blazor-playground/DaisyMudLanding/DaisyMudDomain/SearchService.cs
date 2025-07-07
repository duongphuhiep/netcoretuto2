using Microsoft.Extensions.Logging;

namespace DaisyMudDomain;

public class SearchService(ILogger<SearchService> _logger)
{
    public async ValueTask<FoundItem[]> QuerySearchResultFromDatabase(string searchTerm,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation($"QuerySearchResultFromDatabase '{searchTerm}'");
        await Task.Delay(1000, cancellationToken);
        var resu = new List<FoundItem>();

        resu.AddRange([
            new FoundItem
            {
                Title = "Installation",
                Description = "Lorem ipsum dolor sit amet consectetur adipisicing edivt.",
                Url = "/"
            },
            new FoundItem
            {
                Title = "Wireframes",
                Description = "Amet deserunt nostrud do moldivt consequat proident ipsum magna incididunt.",
                Url = "/"
            },
            new FoundItem
            {
                Title = "Table",
                Description =
                    "Sit sit excepteur aute culpa nulla incididunt fugiat dolore nisi et consectetur labore fugiat commodo.",
                Url = "/"
            },
            new FoundItem
            {
                Title = "Grid",
                Description =
                    " Incididunt commodo pariatur excepteur consequat id. Lorem ipsum dolor sit amet consectetur adipisicing edivt. Esse molestias soluta quisquam nam labore odio tempore numquam repellat quod, sed, ipsa cum dolorum. Dicta optio simidivque obcaecati quaerat deleniti divbero?",
                Url = "/"
            }
        ]);

        if (string.IsNullOrEmpty(searchTerm)) return resu.ToArray();

        return resu.Prepend(new FoundItem
        {
            Title = "From Sever " + Guid.NewGuid(),
            Description = Guid.NewGuid().ToString(),
            Url = "/"
        }).ToArray();
    }
}