using Windfyr.Domain.Repositories.Interfaces;
using Windfyr.Domain.UseCases.Land.Inputs;
using Windfyr.Domain.UseCases.Land.Interfaces;
using Windfyr.Domain.UseCases.Land.Outputs;

namespace Windfyr.Domain.UseCases.Land.Implementations;

public sealed class DeleteLandUseCase(ILandRepository landRepository) : IDeleteLandUseCase
{
    public async Task<DeleteLandOutput> ExecuteAsync(DeleteLandInput input, CancellationToken ct)
    {
        var land = await landRepository.GetByIdAsync(input.UserId, input.LandId, ct);
        if (land is null)
        {
            throw new KeyNotFoundException();
        }

        await landRepository.DeleteAsync(land, ct);

        return new DeleteLandOutput();
    }
}
