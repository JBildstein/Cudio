using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Cudio
{
    internal sealed class HandlerInfo<TBuilder, TModel> : HandlerInfo<TModel>
    {
        private readonly IHandler handler;

        public HandlerInfo(MethodInfo method)
        {
            var parameters = method.GetParameters();
            if (parameters.Length == 0)
            {
                string message = $"Method does not have any parameters and cannot be used as a handler: {typeof(TBuilder).Name}.{method.Name}";
                throw new ArgumentException(message, nameof(method));
            }

            bool parameterTypesCorrect = parameters.All(t => t.ParameterType == typeof(TModel));
            if (!parameterTypesCorrect)
            {
                string message = $"Method has wrong parameter type(s) to be used as a handler: {typeof(TBuilder).Name}.{method.Name}";
                throw new ArgumentException(message, nameof(method));
            }

            MethodType methodType;
            if (parameters.Length == 1) { methodType = MethodType.OnlyNew; }
            else if (parameters.Length == 2) { methodType = MethodType.Both; }
            else
            {
                string message = $"Method has too many parameters to be used as a handler: {typeof(TBuilder).Name}.{method.Name}";
                throw new ArgumentException(message, nameof(method));
            }

            if (method.ReturnType == typeof(Task)) { methodType |= MethodType.Async; }
            else if (method.ReturnType != typeof(void))
            {
                string message = $"Builder handling method must have Task or void as return type: {typeof(TBuilder).Name}.{method.Name}";
                throw new ArgumentException(message, nameof(method));
            }

            handler = methodType switch
            {
                MethodType.OnlyNew => new Sync1Handler(method),
                MethodType.Both => new Sync2Handler(method),
                MethodType.OnlyNew | MethodType.Async => new Async1Handler(method),
                MethodType.Both | MethodType.Async => new Async2Handler(method),
                _ => throw new ArgumentException($"Unable to create builder method handler for {typeof(TBuilder).Name}.{method.Name}"),
            };
        }

        public override async Task Handle(IServiceProvider serviceProvider, IEnumerable<Change<TModel>> changes)
        {
            var builder = serviceProvider.GetRequiredService<TBuilder>();
            await handler.Handle(builder, changes);
        }

        private enum MethodType
        {
            OnlyNew = 1 << 0,
            Both = 1 << 1,
            Async = 1 << 2,
        }

        private interface IHandler
        {
            Task Handle(TBuilder builder, IEnumerable<Change<TModel>> changes);
        }

        private abstract class HandlerBase<T>
            where T : Delegate
        {
            protected readonly T handler;

            public HandlerBase(MethodInfo method)
            {
                handler = method.CreateDelegate<T>();
            }
        }

        private sealed class Async1Handler : HandlerBase<Func<TBuilder, TModel, Task>>, IHandler
        {
            public Async1Handler(MethodInfo method)
                : base(method)
            {
            }

            public async Task Handle(TBuilder builder, IEnumerable<Change<TModel>> changes)
            {
                foreach (var change in changes)
                {
                    await handler(builder, change.NewValue);
                }
            }
        }

        private sealed class Async2Handler : HandlerBase<Func<TBuilder, TModel?, TModel, Task>>, IHandler
        {
            public Async2Handler(MethodInfo method)
                : base(method)
            {
            }

            public async Task Handle(TBuilder builder, IEnumerable<Change<TModel>> changes)
            {
                foreach (var change in changes)
                {
                    await handler(builder, change.OldValue, change.NewValue);
                }
            }
        }

        private sealed class Sync1Handler : HandlerBase<Action<TBuilder, TModel>>, IHandler
        {
            public Sync1Handler(MethodInfo method)
                : base(method)
            {
            }

            public Task Handle(TBuilder builder, IEnumerable<Change<TModel>> changes)
            {
                foreach (var change in changes)
                {
                    handler(builder, change.NewValue);
                }

                return Task.CompletedTask;
            }
        }

        private sealed class Sync2Handler : HandlerBase<Action<TBuilder, TModel?, TModel>>, IHandler
        {
            public Sync2Handler(MethodInfo method)
                : base(method)
            {
            }

            public Task Handle(TBuilder builder, IEnumerable<Change<TModel>> changes)
            {
                foreach (var change in changes)
                {
                    handler(builder, change.OldValue, change.NewValue);
                }

                return Task.CompletedTask;
            }
        }
    }
}
