namespace Windfyr.Domain.Common;

public interface IUseCase<in TInput, TOutput>
{
    Task<TOutput> ExecuteAsync(TInput input, CancellationToken ct);
}
