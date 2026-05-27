namespace Windfyr.Domain.UseCases.Crop.Outputs;

public sealed record ListCropsOutput(IReadOnlyList<CropOutput> Crops);

public sealed record CropOutput(ulong Id, string Type);
