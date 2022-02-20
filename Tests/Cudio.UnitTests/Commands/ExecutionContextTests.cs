using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Cudio
{
    public class ExecutionContextTests
    {
        [Fact]
        public async Task RegisterCreate_WithValue_RegistersCreatedValue()
        {
            var commandBus = CreateBus();
            var builderMock = new Mock<ITestBuilder>();
            var builders = new BuilderCollection(new Type[] { typeof(ITestBuilder) });
            var serviceProvider = new ServiceCollection().AddSingleton(builderMock.Object).BuildServiceProvider();
            var ctx = new BuilderExecutionContext(commandBus, builders);

            ctx.RegisterCreate(new TestModel("Create"));

            await ctx.ApplyBuilders(serviceProvider);
            builderMock.Verify(t => t.Create(null, new TestModel("Create")), Times.Once);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task RegisterUpdate_WithValue_RegistersUpdatedValue()
        {
            var commandBus = CreateBus();
            var builderMock = new Mock<ITestBuilder>();
            var builders = new BuilderCollection(new Type[] { typeof(ITestBuilder) });
            var serviceProvider = new ServiceCollection().AddSingleton(builderMock.Object).BuildServiceProvider();
            var ctx = new BuilderExecutionContext(commandBus, builders);

            ctx.RegisterUpdate(new TestModel("Old"), new TestModel("New"));

            await ctx.ApplyBuilders(serviceProvider);
            builderMock.Verify(t => t.Create(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Update(new TestModel("Old"), new TestModel("New")), Times.Once);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task RegisterDelete_WithValue_RegistersDeletedValue()
        {
            var commandBus = CreateBus();
            var builderMock = new Mock<ITestBuilder>();
            var builders = new BuilderCollection(new Type[] { typeof(ITestBuilder) });
            var serviceProvider = new ServiceCollection().AddSingleton(builderMock.Object).BuildServiceProvider();
            var ctx = new BuilderExecutionContext(commandBus, builders);

            ctx.RegisterDelete(new TestModel("Delete"));

            await ctx.ApplyBuilders(serviceProvider);
            builderMock.Verify(t => t.Create(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(null, new TestModel("Delete")), Times.Once);
        }

        private static CommandBus CreateBus()
        {
            return new CommandBus(
                Mock.Of<IServiceProvider>(),
                Mock.Of<IClaimsPrincipalProvider>(),
                Mock.Of<ITransactionFactory>(),
                Mock.Of<IBuilderCollection>());
        }

        public sealed record TestModel(string Name);

        public interface ITestBuilder
        {
            void Create(TestModel? old, TestModel @new);
            void Update(TestModel? old, TestModel @new);
            void Delete(TestModel? old, TestModel @new);
        }
    }
}
