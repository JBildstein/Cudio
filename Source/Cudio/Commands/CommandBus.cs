using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Cudio
{
    /// <summary>
    /// A bus to handle commands.
    /// </summary>
    public class CommandBus : ICommandBus
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IClaimsPrincipleProvider claimsPrincipalProvider;
        private readonly ITransactionFactory transactionFactory;
        private readonly IBuilderCollection builderCollection;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandBus"/> class.
        /// </summary>
        /// <param name="serviceProvider">Service provider for DI.</param>
        /// <param name="claimsPrincipalProvider">The claims principal provider to authorize commands.</param>
        /// <param name="transactionFactory">The transaction factory.</param>
        /// <param name="builderCollection">The read table builder collection.</param>
        public CommandBus(
            IServiceProvider serviceProvider,
            IClaimsPrincipleProvider claimsPrincipalProvider,
            ITransactionFactory transactionFactory,
            IBuilderCollection builderCollection)
        {
            this.serviceProvider = serviceProvider;
            this.claimsPrincipalProvider = claimsPrincipalProvider;
            this.transactionFactory = transactionFactory;
            this.builderCollection = builderCollection;
        }

        /// <inheritdoc/>
        public async Task<CommandResult> Execute<TCommand>(TCommand command)
            where TCommand : ICommand
        {
            var authorizor = serviceProvider.GetService<ICommandAuthorizer<TCommand>>();
            var validator = serviceProvider.GetService<ICommandValidator<TCommand>>();

            var authContext = new AuthorizationContext(claimsPrincipalProvider.GetClaimsPrincipal());
            var validationContext = new ValidationContext();

            if (authorizor == null) { authContext.Succeed(); }
            else { await authorizor.Authorize(authContext, command); }

            if (authContext.HasSucceeded)
            {
                if (validator != null) { await validator.Validate(validationContext, command); }

                if (!validationContext.HasErrors) { await ExecuteDirect(command); }
            }

            return new CommandResult(command, validationContext, authContext);
        }

        /// <inheritdoc/>
        public async Task<CommandResult> ExecuteForValue<T>(T value)
        {
            return await Execute(new ValueCommand<T>(value));
        }

        /// <inheritdoc/>
        public async Task<CommandResult> ExecuteForCud<T>(T value, ChangeType changeType)
        {
            return await Execute(new CudCommand<T>(value, changeType));
        }

        /// <inheritdoc/>
        public async Task ExecuteDirect<TCommand>(TCommand command)
            where TCommand : ICommand
        {
            var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
            var executionContext = new BuilderExecutionContext(this, builderCollection);

            await using var transaction = await transactionFactory.OpenTransaction();
            await handler.Execute(executionContext, command);
            await executionContext.ApplyBuilders(serviceProvider);
            await transaction.Commit();
        }

        internal async Task ExecuteSubcommand<TCommand>(ExecutionContext executionContext, TCommand command)
            where TCommand : ICommand
        {
            var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
            await handler.Execute(executionContext, command);
        }
    }
}
