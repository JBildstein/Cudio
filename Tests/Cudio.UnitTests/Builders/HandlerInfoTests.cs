using System;
using System.Reflection;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Cudio
{
    public class HandlerInfoTests
    {
        public static readonly Model HandleParam1 = new("Value 1");
        public static readonly Model HandleParam2 = new("Value 2");

        public static readonly object?[][] HandleTestData =
        {
            new object[] { nameof(IBuilder.HandlerVoid1), new Model[] { HandleParam2 } },
            new object[] { nameof(IBuilder.HandlerVoid2), new Model[] { HandleParam1, HandleParam2 } },
            new object[] { nameof(IBuilder.HandlerTask1), new Model[] { HandleParam2 } },
            new object[] { nameof(IBuilder.HandlerTask2), new Model[] { HandleParam1, HandleParam2 } },
        };

        [Theory]
        [InlineData(nameof(IBuilder.HandlerVoid1))]
        [InlineData(nameof(IBuilder.HandlerVoid2))]
        [InlineData(nameof(IBuilder.HandlerTask1))]
        [InlineData(nameof(IBuilder.HandlerTask2))]
        public void From_WithValidValues_ReturnsHandlerInfo(string methodName)
        {
            var handlerInfo = HandlerInfo.From(typeof(IBuilder), typeof(Model), GetMethod(methodName));

            handlerInfo.Should().NotBeNull().And.BeOfType<HandlerInfo<IBuilder, Model>>();
        }

        [Theory]
        [InlineData(nameof(IBuilder.HandlerVoid1))]
        [InlineData(nameof(IBuilder.HandlerVoid2))]
        [InlineData(nameof(IBuilder.HandlerTask1))]
        [InlineData(nameof(IBuilder.HandlerTask2))]
        public void Ctor_WithValidValues_ReturnsHandlerInfo(string methodName)
        {
            Action act = () => new HandlerInfo<IBuilder, Model>(GetMethod(methodName));

            act.Should().NotThrow();
        }

        [Theory]
        [InlineData(nameof(IBuilder.InvalidHandlerReturnType))]
        [InlineData(nameof(IBuilder.InvalidHandlerVoid0))]
        [InlineData(nameof(IBuilder.InvalidHandlerVoid1))]
        [InlineData(nameof(IBuilder.InvalidHandlerVoid2))]
        [InlineData(nameof(IBuilder.InvalidHandlerVoid3))]
        [InlineData(nameof(IBuilder.InvalidHandlerTask0))]
        [InlineData(nameof(IBuilder.InvalidHandlerTask1))]
        [InlineData(nameof(IBuilder.InvalidHandlerTask2))]
        [InlineData(nameof(IBuilder.InvalidHandlerTask3))]
        public void Ctor_WithInvalidValues_ThrowsException(string methodName)
        {
            Action act = () => new HandlerInfo<IBuilder, Model>(GetMethod(methodName));

            act.Should().Throw<ArgumentException>();
        }

        [Theory]
        [MemberData(nameof(HandleTestData))]
        public async Task Handle_WithValidValues_CallsExpectedMethod(string methodName, Model[] expected)
        {
            var handlerInfo = new HandlerInfo<IBuilder, Model>(GetMethod(methodName));
            var builder = new Mock<IBuilder>();
            var serviceProvider = new ServiceCollection().AddSingleton(builder.Object).BuildServiceProvider();

            await handlerInfo.Handle(serviceProvider, new Change<Model>[] { new(HandleParam1, HandleParam2) });

            builder.Invocations.Should().ContainSingle().Which.Arguments.Should().Equal(expected);
        }

        private static MethodInfo GetMethod(string name)
        {
            return typeof(IBuilder).GetMethod(name, BindingFlags.Public | BindingFlags.Instance)!;
        }

        public record Model(string Name);

        public interface IBuilder
        {
            void HandlerVoid1(Model model);
            void HandlerVoid2(Model old, Model @new);

            Task HandlerTask1(Model model);
            Task HandlerTask2(Model old, Model @new);

            void InvalidHandlerVoid0();
            int InvalidHandlerReturnType(Model model);
            void InvalidHandlerVoid1(int model);
            void InvalidHandlerVoid2(Model old, int @new);
            void InvalidHandlerVoid3(Model old, Model @new, Model invalid);

            Task InvalidHandlerTask0();
            Task InvalidHandlerTask1(int model);
            Task InvalidHandlerTask2(Model old, int @new);
            Task InvalidHandlerTask3(Model old, Model @new, Model invalid);
        }
    }
}
