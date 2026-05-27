namespace Windfyr.Domain.UseCases.Land.Outputs;

public sealed record ListLandsOutput(IReadOnlyList<LandOutput> Lands);
