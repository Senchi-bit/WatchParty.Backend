using Microsoft.Extensions.DependencyInjection;

namespace WatchParty.Application.Common.Abstractions.CQRS;

internal sealed class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    public Task<TResponse> Dispatch<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken)
        where TCommand : ICommand<TResponse>
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();
        return handler.Handle(command, cancellationToken);
    }
}
