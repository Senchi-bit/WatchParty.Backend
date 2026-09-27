namespace WatchParty.Application.Common.Abstractions.CQRS;

public interface ICommandDispatcher
{
    Task<TResponse> Dispatch<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken)
        where TCommand : ICommand<TResponse>;
}
