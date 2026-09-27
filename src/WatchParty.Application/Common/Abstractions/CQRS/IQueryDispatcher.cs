namespace WatchParty.Application.Common.Abstractions.CQRS;

public interface IQueryDispatcher
{
    Task<TResponse> Dispatch<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken)
        where TQuery : IQuery<TResponse>;
}
