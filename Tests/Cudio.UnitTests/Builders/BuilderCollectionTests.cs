using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Cudio
{
    public class BuilderCollectionTests
    {
        [Theory]
        [InlineData(ChangeType.Create, "Create")]
        [InlineData(ChangeType.Update, "Update")]
        [InlineData(ChangeType.Delete, "Delete")]
        public async Task GetHandlersFor_WithChangeType_ReturnsAllHandlersForType(ChangeType changeType, string methodName)
        {
            var builderMock = new Mock<IBuilder>();
            builderMock.Setup(t => t.Create(new("New")));
            builderMock.Setup(t => t.Update(new("Old"), new("New")));
            builderMock.Setup(t => t.Delete(new("New")));
            var invalidBuilderMock = new Mock<IInvalidBuilder>();
            var serviceProvider = new ServiceCollection()
                .AddSingleton(builderMock.Object)
                .AddSingleton(invalidBuilderMock.Object)
                .BuildServiceProvider();
            var collection = new BuilderCollection(new Type[] { typeof(IBuilder), typeof(IInvalidBuilder) });

            var handler = collection.GetHandlersFor<Model>(changeType).Should().ContainSingle().Subject;
            await handler.Handle(serviceProvider, new Change<Model>[] { new(new("Old"), new("New")) });

            builderMock.Verify();
            builderMock.Invocations.Should().ContainSingle().Which.Method.Name.Should().Be(methodName);
            invalidBuilderMock.Invocations.Should().BeEmpty();
        }

        [Fact]
        public void GetHandlersFor_WithoutHandlers_ReturnsEmptyList()
        {
            var builderMock = new Mock<IInvalidBuilder>();
            var serviceProvider = new ServiceCollection()
                .AddSingleton(builderMock.Object)
                .BuildServiceProvider();
            var collection = new BuilderCollection(new Type[] { typeof(IInvalidBuilder) });

            collection.GetHandlersFor<Model>(ChangeType.Create).Should().BeEmpty();
        }

        public record Model(string Name);

        public interface IBuilder
        {
            void Create(Model p1);
            void Update(Model p1, Model p2);
            void Delete(Model p1);
        }

        public interface IInvalidBuilder
        {
            void Create();
            void Update();
            void Delete();

            void Create(Model p1, int p2);
            void Update(Model p1, int p2);
            void Delete(Model p1, int p2);

            void Create(Model p1, Model p2, Model p3);
            void Update(Model p1, Model p2, Model p3);
            void Delete(Model p1, Model p2, Model p3);
        }
    }
}
