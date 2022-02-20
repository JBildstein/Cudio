using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Cudio
{
    public class QueryBusTests
    {
        [Fact]
        public async Task Execute_WithSucceedingFullQuery_IsExecuted()
        {
            var queryHandlerMock = new Mock<IFullQueryHandler<TestQuery, int>>();
            queryHandlerMock.Setup(t => t.Execute(It.IsAny<TestQuery>())).Returns(Task.FromResult(42));
            queryHandlerMock.Setup(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestQuery>()))
                .Callback<AuthorizationContext, TestQuery>((ctx, cmd) => ctx.Succeed());
            var bus = BuildBus(queryHandlerMock.Object);

            var result = await bus.Execute(new TestQuery());

            result.Successful.Should().BeTrue();
            result.QueryId.Should().Be(TestQuery.Id);
            result.Value.Should().Be(42);
            queryHandlerMock.Verify(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestQuery>()), Times.Once);
            queryHandlerMock.Verify(t => t.Validate(It.IsAny<ValidationContext>(), It.IsAny<TestQuery>()), Times.Once);
            queryHandlerMock.Verify(t => t.Execute(It.IsAny<TestQuery>()), Times.Once);
        }

        [Fact]
        public async Task Execute_WithSucceedingAuthorizedQuery_IsExecuted()
        {
            var queryHandlerMock = new Mock<IAuthorizedQueryHandler<TestQuery, int>>();
            queryHandlerMock.Setup(t => t.Execute(It.IsAny<TestQuery>())).Returns(Task.FromResult(42));
            queryHandlerMock.Setup(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestQuery>()))
                .Callback<AuthorizationContext, TestQuery>((ctx, cmd) => ctx.Succeed());
            var bus = BuildBus(queryHandlerMock.Object);

            var result = await bus.Execute(new TestQuery());

            result.Successful.Should().BeTrue();
            result.QueryId.Should().Be(TestQuery.Id);
            result.Value.Should().Be(42);
            queryHandlerMock.Verify(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestQuery>()), Times.Once);
            queryHandlerMock.Verify(t => t.Execute(It.IsAny<TestQuery>()), Times.Once);
        }

        [Fact]
        public async Task Execute_WithSucceedingValidatedQuery_IsExecuted()
        {
            var queryHandlerMock = new Mock<IValidatedQueryHandler<TestQuery, int>>();
            queryHandlerMock.Setup(t => t.Execute(It.IsAny<TestQuery>())).Returns(Task.FromResult(42));
            var bus = BuildBus(queryHandlerMock.Object);

            var result = await bus.Execute(new TestQuery());

            result.Successful.Should().BeTrue();
            result.QueryId.Should().Be(TestQuery.Id);
            result.Value.Should().Be(42);
            queryHandlerMock.Verify(t => t.Validate(It.IsAny<ValidationContext>(), It.IsAny<TestQuery>()), Times.Once);
            queryHandlerMock.Verify(t => t.Execute(It.IsAny<TestQuery>()), Times.Once);
        }

        [Fact]
        public async Task Execute_WithSimpleQuery_IsExecuted()
        {
            var queryHandlerMock = new Mock<IQueryHandler<TestQuery, int>>();
            queryHandlerMock.Setup(t => t.Execute(It.IsAny<TestQuery>())).Returns(Task.FromResult(42));
            var bus = BuildBus(queryHandlerMock.Object);

            var result = await bus.Execute(new TestQuery());

            result.Successful.Should().BeTrue();
            result.QueryId.Should().Be(TestQuery.Id);
            result.Value.Should().Be(42);
            queryHandlerMock.Verify(t => t.Execute(It.IsAny<TestQuery>()), Times.Once);
        }

        [Fact]
        public async Task ExecuteDirect_WithSimpleQuery_IsExecuted()
        {
            var queryHandlerMock = new Mock<IQueryHandler<TestQuery, int>>();
            queryHandlerMock.Setup(t => t.Execute(It.IsAny<TestQuery>())).Returns(Task.FromResult(42));
            var bus = BuildBus(queryHandlerMock.Object);

            var result = await bus.ExecuteDirect(new TestQuery());

            result.Should().Be(42);
            queryHandlerMock.Verify(t => t.Execute(It.IsAny<TestQuery>()), Times.Once);
        }

        [Fact]
        public async Task Execute_WithQueryFailingAuthorization_IsRejected()
        {
            var queryHandlerMock = new Mock<IFullQueryHandler<TestQuery, int>>();
            queryHandlerMock.Setup(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestQuery>()))
                .Callback<AuthorizationContext, TestQuery>((ctx, cmd) => ctx.Fail());
            var bus = BuildBus(queryHandlerMock.Object);

            var result = await bus.Execute(new TestQuery());

            result.Successful.Should().BeFalse();
            result.QueryId.Should().Be(TestQuery.Id);
            result.Value.Should().Be(0);
            queryHandlerMock.Verify(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestQuery>()), Times.Once);
            queryHandlerMock.Verify(t => t.Validate(It.IsAny<ValidationContext>(), It.IsAny<TestQuery>()), Times.Never);
            queryHandlerMock.Verify(t => t.Execute(It.IsAny<TestQuery>()), Times.Never);
        }

        [Fact]
        public async Task Execute_WithQueryFailingValidation_IsRejected()
        {
            var queryHandlerMock = new Mock<IFullQueryHandler<TestQuery, int>>();
            queryHandlerMock.Setup(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestQuery>()))
                .Callback<AuthorizationContext, TestQuery>((ctx, cmd) => ctx.Succeed());
            queryHandlerMock.Setup(t => t.Validate(It.IsAny<ValidationContext>(), It.IsAny<TestQuery>()))
                .Callback<ValidationContext, TestQuery>((ctx, cmd) => { ctx.AddError("key", "error"); });
            var bus = BuildBus(queryHandlerMock.Object);

            var result = await bus.Execute(new TestQuery());

            result.Successful.Should().BeFalse();
            result.QueryId.Should().Be(TestQuery.Id);
            result.Value.Should().Be(0);
            queryHandlerMock.Verify(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestQuery>()), Times.Once);
            queryHandlerMock.Verify(t => t.Validate(It.IsAny<ValidationContext>(), It.IsAny<TestQuery>()), Times.Once);
            queryHandlerMock.Verify(t => t.Execute(It.IsAny<TestQuery>()), Times.Never);
        }

        private static QueryBus BuildBus(IQueryHandler<TestQuery, int> queryHandler)
        {
            var services = new ServiceCollection();
            services.AddSingleton(queryHandler);

            if (queryHandler is IQueryAuthorizer<TestQuery> queryAuth)
            {
                services.AddSingleton(queryAuth);
            }

            if (queryHandler is IQueryValidator<TestQuery> queryValidate)
            {
                services.AddSingleton(queryValidate);
            }

            return new QueryBus(services.BuildServiceProvider(), new DummyClaimsPrincipalProvider());
        }

        public sealed class TestQuery : IQuery<int>
        {
            public static readonly Guid Id = new("82a18736-b10f-451e-bfb8-1ee7703fd6c3");

            public Guid QueryId { get; } = Id;
        }
    }
}
