using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cudio
{
    /// <summary>
    /// Context for executing a command.
    /// </summary>
    internal sealed class BuilderExecutionContext : ExecutionContext
    {
        private readonly CommandBus commandBus;
        private readonly IBuilderCollection builders;
        private readonly Dictionary<ChangeKey, IChangeCollection> changes = new();

        public BuilderExecutionContext(CommandBus commandBus, IBuilderCollection builders)
        {
            this.commandBus = commandBus;
            this.builders = builders;
        }

        public override async Task ExecuteSubcommand<T>(T command)
        {
            await commandBus.ExecuteSubcommand(this, command);
        }

        protected override void RegisterChange<T>(T? oldValue, T newValue, ChangeType changeType)
            where T : class
        {
            var key = ChangeKey.For<T>(changeType);
            if (!changes.TryGetValue(key, out var collection))
            {
                collection = new ChangeCollection<T>(changeType);
                changes.Add(key, collection);
            }

            ((ChangeCollection<T>)collection).Add(oldValue, newValue);
        }

        public async Task ApplyBuilders(IServiceProvider serviceProvider)
        {
            foreach (var change in changes.Values)
            {
                await change.ApplyBuilders(serviceProvider, builders);
            }
        }

        private interface IChangeCollection
        {
            ChangeType ChangeType { get; }

            Task ApplyBuilders(IServiceProvider serviceProvider, IBuilderCollection builders);
        }

        private sealed class ChangeCollection<T> : IChangeCollection
        {
            public ChangeType ChangeType { get; }

            private readonly List<Change<T>> changes = new();

            public ChangeCollection(ChangeType changeType)
            {
                ChangeType = changeType;
            }

            public void Add(T? oldValue, T newValue)
            {
                changes.Add(new Change<T>(oldValue, newValue));
            }

            public async Task ApplyBuilders(IServiceProvider serviceProvider, IBuilderCollection builders)
            {
                var handlers = builders.GetHandlersFor<T>(ChangeType);
                foreach (var handler in handlers)
                {
                    await handler.Handle(serviceProvider, changes);
                }
            }
        }
    }
}
