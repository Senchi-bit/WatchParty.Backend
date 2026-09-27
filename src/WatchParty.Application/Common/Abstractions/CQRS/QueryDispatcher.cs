using Microsoft.Extensions.DependencyInjection;

namespace WatchParty.Application.Common.Abstractions.CQRS;

internal sealed class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    public Task<TResponse> Dispatch<TQuery, TResponse>(TQuery query, CancellationToken cancellationToken)
        where TQuery : IQuery<TResponse>
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        return handler.Handle(query, cancellationToken);
    }
}
