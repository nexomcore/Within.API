using WithinAPI.Models;
using WithinAPI.Services;

namespace WithinAPI.Endpoints;

public static class MarketFitEndpoints
{
    public static IEndpointRouteBuilder MapMarketFitEndpoints(this IEndpointRouteBuilder app)
    {
        var marketFit = app.MapGroup("/api/market-fit");

        marketFit.MapPost("/submissions", async (MarketFitSubmissionDto request, IMarketFitSubmissionService service, CancellationToken cancellationToken) =>
        {
            var response = await service.CreateAsync(request, cancellationToken);
            return response is null
                ? Results.BadRequest(new { message = "Audience must be user, provider, or interest. Name and contact are required." })
                : Results.Created($"/api/market-fit/submissions/{response.Id}", response);
        });

        return app;
    }
}
