using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Cudio
{
    public class CudioServiceDiscoveryTests
    {
        [Fact]
        public void AddAll_WithoutTypes_AddsDefaultRequiredServices()
        {
            var services = new ServiceCollection();
            services.AddScoped<IClaimsPrincipalProvider, DummyClaimsPrincipalProvider>();
            services.AddScoped<ITransactionFactory, DummyTransactionFactory>();
            var discovery = new CudioServiceDiscovery(Enumerable.Empty<TypeInfo>());

            discovery.AddAll(services);

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<ICommandBus>();
            scope.ServiceProvider.GetRequiredService<IQueryBus>();
            scope.ServiceProvider.GetRequiredService<IBuilderCollection>();
        }

        [Fact]
        public void AddAll_WithInvalidTypes_DoesNotAddThem()
        {
            var types = new TypeInfo[]
            {
                typeof(TestCommand).GetTypeInfo(),

                typeof(StructHandler).GetTypeInfo(),
                typeof(AbstractHandler).GetTypeInfo(),
                GetMockType<ICommandHandler<TestCommand>>(),
            };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.AddAll(services);

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetService(typeof(StructHandler)).Should().BeNull();
            scope.ServiceProvider.GetService<AbstractHandler>().Should().BeNull();
        }

        [Fact]
        public void AddAll_WithValidTypes_AddsThem()
        {
            var hydratorType = GetMockType<IHydrator<TestTable>>();
            var builderType = GetMockType<IReadModelBuilder<TestTable>>();

            var types = new TypeInfo[]
            {
                typeof(TestCommand).GetTypeInfo(),
                typeof(TestQuery).GetTypeInfo(),
                typeof(TestTable).GetTypeInfo(),

                GetMockType<ICommandHandler<TestCommand>>(),
                GetMockType<IQueryHandler<TestQuery, int>>(),
                hydratorType,
                builderType,
            };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.AddAll(services);

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<ICommandHandler<TestCommand>>();
            scope.ServiceProvider.GetRequiredService<IQueryHandler<TestQuery, int>>();
            scope.ServiceProvider.GetRequiredService(builderType);
            scope.ServiceProvider.GetRequiredService<IHydrator<TestTable>>();
            var hydrators = scope.ServiceProvider.GetServices<IHydrator>().Should().ContainSingle().Which.Should().BeOfType(hydratorType);
        }

        [Fact]
        public void AddAll_WithFullCommandHandler_AddsAllCommandHandlers()
        {
            var types = new TypeInfo[]
            {
                typeof(TestCommand).GetTypeInfo(),

                GetMockType<IFullCommandHandler<TestCommand>>(),
            };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.AddAll(services);

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<ICommandHandler<TestCommand>>();
            scope.ServiceProvider.GetRequiredService<ICommandAuthorizer<TestCommand>>();
            scope.ServiceProvider.GetRequiredService<ICommandValidator<TestCommand>>();
        }

        [Fact]
        public void AddAll_WithSeparateCommandHandler_AddsAllCommandHandlers()
        {
            var types = new TypeInfo[]
            {
                typeof(TestCommand).GetTypeInfo(),

                GetMockType<ICommandHandler<TestCommand>>(),
                GetMockType<ICommandAuthorizer<TestCommand>>(),
                GetMockType<ICommandValidator<TestCommand>>(),
            };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.AddAll(services);

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<ICommandHandler<TestCommand>>();
            scope.ServiceProvider.GetRequiredService<ICommandAuthorizer<TestCommand>>();
            scope.ServiceProvider.GetRequiredService<ICommandValidator<TestCommand>>();
        }

        [Fact]
        public void AddAll_WithFullQueryHandler_AddsAllQueryHandlers()
        {
            var types = new TypeInfo[]
            {
                typeof(TestQuery).GetTypeInfo(),

                GetMockType<IFullQueryHandler<TestQuery, int>>(),
            };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.AddAll(services);

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<IQueryHandler<TestQuery, int>>();
            scope.ServiceProvider.GetRequiredService<IQueryAuthorizer<TestQuery>>();
            scope.ServiceProvider.GetRequiredService<IQueryValidator<TestQuery>>();
        }

        [Fact]
        public void AddAll_WithSeparateQueryHandler_AddsAllQueryHandlers()
        {
            var types = new TypeInfo[]
            {
                typeof(TestQuery).GetTypeInfo(),

                GetMockType<IQueryHandler<TestQuery, int>>(),
                GetMockType<IQueryAuthorizer<TestQuery>>(),
                GetMockType<IQueryValidator<TestQuery>>(),
            };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.AddAll(services);

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.GetRequiredService<IQueryHandler<TestQuery, int>>();
            scope.ServiceProvider.GetRequiredService<IQueryAuthorizer<TestQuery>>();
            scope.ServiceProvider.GetRequiredService<IQueryValidator<TestQuery>>();
        }

        [Fact]
        public void AddCommandHandlers_WithMultipleHandlersForSameCommand_ThrowsException()
        {
            var types = new TypeInfo[]
            {
                typeof(TestCommand).GetTypeInfo(),
                GetMockType<ICommandHandler<TestCommand>>(),
                GetMockType<ICommandHandler<TestCommand>>(),
            };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.Invoking(t => t.AddCommandHandlers(services)).Should().ThrowExactly<AmbiguousMatchException>();
        }

        [Fact]
        public void AddCommandHandlers_WithMultipleHandlersForSameValueCommand_ThrowsException()
        {
            var types = new TypeInfo[]
            {
                typeof(TestCommand).GetTypeInfo(),
                GetMockType<IValueCommandHandler<TestTable>>(),
                GetMockType<IValueCommandHandler<TestTable>>(),
            };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.Invoking(t => t.AddCommandHandlers(services)).Should().ThrowExactly<AmbiguousMatchException>();
        }

        [Fact]
        public void AddCommandHandlers_WithMultipleHandlersForCudValueCommand_ThrowsException()
        {
            var types = new TypeInfo[]
            {
                typeof(TestCommand).GetTypeInfo(),
                GetMockType<ICudCommandHandler<TestTable>>(),
                GetMockType<ICudCommandHandler<TestTable>>(),
            };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.Invoking(t => t.AddCommandHandlers(services)).Should().ThrowExactly<AmbiguousMatchException>();
        }

        [Fact]
        public void AddCommandHandlers_WithMultipleAuthorizersForSameCommand_ThrowsException()
        {
            var types = new TypeInfo[]
            {
                typeof(TestCommand).GetTypeInfo(),
                GetMockType<ICommandHandler<TestCommand>>(),
                GetMockType<ICommandAuthorizer<TestCommand>>(),
                GetMockType<ICommandAuthorizer<TestCommand>>(),
            };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.Invoking(t => t.AddCommandHandlers(services)).Should().ThrowExactly<AmbiguousMatchException>();
        }

        [Fact]
        public void AddCommandHandlers_WithMultipleValidatorsForSameCommand_ThrowsException()
        {
            var types = new TypeInfo[]
            {
                typeof(TestCommand).GetTypeInfo(),
                GetMockType<ICommandHandler<TestCommand>>(),
                GetMockType<ICommandValidator<TestCommand>>(),
                GetMockType<ICommandValidator<TestCommand>>(),
            };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.Invoking(t => t.AddCommandHandlers(services)).Should().ThrowExactly<AmbiguousMatchException>();
        }

        [Fact]
        public void AddQueryHandlers_WithMultipleHandlersForSameQuery_ThrowsException()
        {
            var types = new TypeInfo[]
            {
                typeof(TestQuery).GetTypeInfo(),
                GetMockType<IQueryHandler<TestQuery, int>>(),
                GetMockType<IQueryHandler<TestQuery, int>>(),
            };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.Invoking(t => t.AddQueryHandlers(services)).Should().ThrowExactly<AmbiguousMatchException>();
        }

        [Fact]
        public void AddQueryHandlers_WithMultipleAuthorizersForSameQuery_ThrowsException()
        {
            var types = new TypeInfo[]
            {
                typeof(TestQuery).GetTypeInfo(),
                GetMockType<IQueryHandler<TestQuery, int>>(),
                GetMockType<IQueryAuthorizer<TestQuery>>(),
                GetMockType<IQueryAuthorizer<TestQuery>>(),
            };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.Invoking(t => t.AddQueryHandlers(services)).Should().ThrowExactly<AmbiguousMatchException>();
        }

        [Fact]
        public void AddQueryHandlers_WithMultipleValidatorsForSameQuery_ThrowsException()
        {
            var types = new TypeInfo[]
            {
                typeof(TestQuery).GetTypeInfo(),
                GetMockType<IQueryHandler<TestQuery, int>>(),
                GetMockType<IQueryValidator<TestQuery>>(),
                GetMockType<IQueryValidator<TestQuery>>(),
            };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.Invoking(t => t.AddQueryHandlers(services)).Should().ThrowExactly<AmbiguousMatchException>();
        }

        [Fact]
        public void AddReadModelBuilders_WithMultipleReadModelBuildersForSameType_ThrowsException()
        {
            var types = new TypeInfo[]
            {
                GetMockType<IReadModelBuilder<TestTable>>(),
                GetMockType<IReadModelBuilder<TestTable>>(),
            };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.Invoking(t => t.AddReadModelBuilders(services)).Should().ThrowExactly<AmbiguousMatchException>();
        }

        [Fact]
        public void AddReadModelHydrators_WithMultipleReadModelHydratorsForSameType_ThrowsException()
        {
            var types = new TypeInfo[]
            {
                GetMockType<IHydrator<TestTable>>(),
                GetMockType<IHydrator<TestTable>>(),
            };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.Invoking(t => t.AddReadModelHydrators(services)).Should().ThrowExactly<AmbiguousMatchException>();
        }

        [Fact]
        public void AddAll_WithoutQueryHandler_ThrowsException()
        {
            var types = new TypeInfo[] { typeof(TestQuery).GetTypeInfo(), };
            var services = new ServiceCollection();
            var discovery = new CudioServiceDiscovery(types);

            discovery.Invoking(t => t.AddAll(services)).Should().ThrowExactly<InvalidOperationException>();
        }

        private static TypeInfo GetMockType<T>()
            where T : class
        {
            return Mock.Of<T>()
                .GetType()
                .GetTypeInfo();
        }

        public sealed class TestCommand : CommandBase
        {
        }

        public sealed class TestQuery : QueryBase<int>
        {
        }

        public sealed class TestTable
        {
        }

        private struct StructHandler : ICommandHandler<TestCommand>
        {
            public Task Execute(ExecutionContext context, TestCommand command)
            {
                throw new NotImplementedException();
            }
        }

        private abstract class AbstractHandler : ICommandHandler<TestCommand>
        {
            public Task Execute(ExecutionContext context, TestCommand command)
            {
                throw new NotImplementedException();
            }
        }
    }
}
