using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Transactions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Cudio
{
    public class CommandBusTests
    {
        [Fact]
        public async Task Execute_WithSucceedingFullCommandWithoutChanges_IsExecutedWithoutBuilders()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<IFullCommandHandler<TestCommand>>();
            cmdHandlerMock.Setup(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestCommand>()))
                .Callback<AuthorizationContext, TestCommand>((ctx, cmd) => ctx.Succeed());
            var bus = BuildBus(cmdHandlerMock.Object, builderMock.Object);

            var result = await bus.Execute(new TestCommand());

            result.Successful.Should().BeTrue();
            result.CommandId.Should().Be(TestCommand.Id);
            cmdHandlerMock.Verify(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestCommand>()), Times.Once);
            cmdHandlerMock.Verify(t => t.Validate(It.IsAny<ValidationContext>(), It.IsAny<TestCommand>()), Times.Once);
            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()), Times.Once);
            builderMock.Verify(t => t.Create(It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task Execute_WithSucceedingAuthorizedCommandWithoutChanges_IsExecutedWithoutBuilders()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<IAuthorizedCommandHandler<TestCommand>>();
            cmdHandlerMock.Setup(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestCommand>()))
                .Callback<AuthorizationContext, TestCommand>((ctx, cmd) => ctx.Succeed());
            var bus = BuildBus(cmdHandlerMock.Object, builderMock.Object);

            var result = await bus.Execute(new TestCommand());

            result.Successful.Should().BeTrue();
            result.CommandId.Should().Be(TestCommand.Id);
            cmdHandlerMock.Verify(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestCommand>()), Times.Once);
            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()), Times.Once);
            builderMock.Verify(t => t.Create(It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task Execute_WithSucceedingValidatedCommandWithoutChanges_IsExecutedWithoutBuilders()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<IValidatedCommandHandler<TestCommand>>();
            var bus = BuildBus(cmdHandlerMock.Object, builderMock.Object);

            var result = await bus.Execute(new TestCommand());

            result.Successful.Should().BeTrue();
            result.CommandId.Should().Be(TestCommand.Id);
            cmdHandlerMock.Verify(t => t.Validate(It.IsAny<ValidationContext>(), It.IsAny<TestCommand>()), Times.Once);
            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()), Times.Once);
            builderMock.Verify(t => t.Create(It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task Execute_WithSimpleCommandWithoutChanges_IsExecutedWithoutBuilders()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<ICommandHandler<TestCommand>>();
            var bus = BuildBus(cmdHandlerMock.Object, builderMock.Object);

            var result = await bus.Execute(new TestCommand());

            result.Successful.Should().BeTrue();
            result.CommandId.Should().Be(TestCommand.Id);
            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()), Times.Once);
            builderMock.Verify(t => t.Create(It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task Execute_WithSimpleCommandAndSubcommandWithoutChanges_IsExecutedWithoutBuilders()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<ICommandHandler<TestCommand>>();
            var subcmdHandlerMock = new Mock<ICommandHandler<TestSubcommand>>();
            cmdHandlerMock.Setup(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()))
                .Callback<ExecutionContext, TestCommand>((ctx, cmd) => ctx.ExecuteSubcommand(new TestSubcommand()));
            var bus = BuildBus(cmdHandlerMock.Object, subcmdHandlerMock.Object, builderMock.Object);

            var result = await bus.Execute(new TestCommand());

            result.Successful.Should().BeTrue();
            result.CommandId.Should().Be(TestCommand.Id);
            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()), Times.Once);
            subcmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestSubcommand>()), Times.Once);
            builderMock.Verify(t => t.Create(It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteForValue_WithSimpleCommandWithoutChanges_IsExecutedWithoutBuilders()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<IValueCommandHandler<TestModel>>();
            var bus = BuildBus(cmdHandlerMock.Object, builderMock.Object);

            var result = await bus.ExecuteForValue(new TestModel("Value"));

            result.Successful.Should().BeTrue();
            result.CommandId.Should().NotBe(Guid.Empty);
            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<ValueCommand<TestModel>>()), Times.Once);
            builderMock.Verify(t => t.Create(It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteDirect_WithSimpleCommandWithoutChanges_IsExecutedWithoutBuilders()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<ICommandHandler<TestCommand>>();
            var bus = BuildBus(cmdHandlerMock.Object, builderMock.Object);

            await bus.ExecuteDirect(new TestCommand());

            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()), Times.Once);
            builderMock.Verify(t => t.Create(It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteDirect_WithFullCommandWithoutChanges_IsExecutedWithoutBuilders()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<IFullCommandHandler<TestCommand>>();
            var bus = BuildBus(cmdHandlerMock.Object, builderMock.Object);

            await bus.ExecuteDirect(new TestCommand());

            cmdHandlerMock.Verify(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestCommand>()), Times.Never);
            cmdHandlerMock.Verify(t => t.Validate(It.IsAny<ValidationContext>(), It.IsAny<TestCommand>()), Times.Never);
            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()), Times.Once);
            builderMock.Verify(t => t.Create(It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task Execute_WithSucceedingFullCommandWithChanges_IsExecutedWithBuilders()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<IFullCommandHandler<TestCommand>>();
            cmdHandlerMock.Setup(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestCommand>()))
                .Callback<AuthorizationContext, TestCommand>((ctx, cmd) => ctx.Succeed());
            cmdHandlerMock.Setup(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()))
                .Callback<ExecutionContext, TestCommand>((ctx, cmd) => { ctx.RegisterCreate(new TestModel("New")); });
            var bus = BuildBus(cmdHandlerMock.Object, builderMock.Object);

            var result = await bus.Execute(new TestCommand());

            result.Successful.Should().BeTrue();
            result.CommandId.Should().Be(TestCommand.Id);
            cmdHandlerMock.Verify(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestCommand>()), Times.Once);
            cmdHandlerMock.Verify(t => t.Validate(It.IsAny<ValidationContext>(), It.IsAny<TestCommand>()), Times.Once);
            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()), Times.Once);
            builderMock.Verify(t => t.Create(It.IsAny<TestModel>()), Times.Once);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task Execute_WithSucceedingAuthorizedCommandWithChanges_IsExecutedWithBuilders()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<IAuthorizedCommandHandler<TestCommand>>();
            cmdHandlerMock.Setup(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestCommand>()))
                .Callback<AuthorizationContext, TestCommand>((ctx, cmd) => ctx.Succeed());
            cmdHandlerMock.Setup(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()))
                .Callback<ExecutionContext, TestCommand>((ctx, cmd) => { ctx.RegisterCreate(new TestModel("New")); });
            var bus = BuildBus(cmdHandlerMock.Object, builderMock.Object);

            var result = await bus.Execute(new TestCommand());

            result.Successful.Should().BeTrue();
            result.CommandId.Should().Be(TestCommand.Id);
            cmdHandlerMock.Verify(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestCommand>()), Times.Once);
            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()), Times.Once);
            builderMock.Verify(t => t.Create(It.IsAny<TestModel>()), Times.Once);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task Execute_WithSucceedingValidatedCommandWithChanges_IsExecutedWithBuilders()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<IValidatedCommandHandler<TestCommand>>();
            cmdHandlerMock.Setup(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()))
                .Callback<ExecutionContext, TestCommand>((ctx, cmd) => { ctx.RegisterCreate(new TestModel("New")); });
            var bus = BuildBus(cmdHandlerMock.Object, builderMock.Object);

            var result = await bus.Execute(new TestCommand());

            result.Successful.Should().BeTrue();
            result.CommandId.Should().Be(TestCommand.Id);
            cmdHandlerMock.Verify(t => t.Validate(It.IsAny<ValidationContext>(), It.IsAny<TestCommand>()), Times.Once);
            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()), Times.Once);
            builderMock.Verify(t => t.Create(It.IsAny<TestModel>()), Times.Once);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task Execute_WithSimpleCommandWithChanges_IsExecutedWithBuilders()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<ICommandHandler<TestCommand>>();
            cmdHandlerMock.Setup(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()))
                .Callback<ExecutionContext, TestCommand>((ctx, cmd) => { ctx.RegisterCreate(new TestModel("New")); });
            var bus = BuildBus(cmdHandlerMock.Object, builderMock.Object);

            var result = await bus.Execute(new TestCommand());

            result.Successful.Should().BeTrue();
            result.CommandId.Should().Be(TestCommand.Id);
            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()), Times.Once);
            builderMock.Verify(t => t.Create(new TestModel("New")), Times.Once);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task Execute_WithSimpleCommandAndSubcommandWithChanges_IsExecutedWithBuilders()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<ICommandHandler<TestCommand>>();
            cmdHandlerMock.Setup(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()))
                .Callback<ExecutionContext, TestCommand>((ctx, cmd) =>
                {
                    ctx.RegisterCreate(new TestModel("New"));
                    ctx.ExecuteSubcommand(new TestSubcommand());
                });
            var subcmdHandlerMock = new Mock<ICommandHandler<TestSubcommand>>();
            subcmdHandlerMock.Setup(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestSubcommand>()))
                .Callback<ExecutionContext, TestSubcommand>((ctx, cmd) => { ctx.RegisterCreate(new TestModel("SubNew")); });
            var bus = BuildBus(cmdHandlerMock.Object, subcmdHandlerMock.Object, builderMock.Object);

            var result = await bus.Execute(new TestCommand());

            result.Successful.Should().BeTrue();
            result.CommandId.Should().Be(TestCommand.Id);
            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()), Times.Once);
            subcmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestSubcommand>()), Times.Once);
            builderMock.Verify(t => t.Create(new TestModel("New")), Times.Once);
            builderMock.Verify(t => t.Create(new TestModel("SubNew")), Times.Once);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteDirect_WithSimpleCommandWithChanges_IsExecutedWithBuilders()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<ICommandHandler<TestCommand>>();
            cmdHandlerMock.Setup(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()))
                .Callback<ExecutionContext, TestCommand>((ctx, cmd) => { ctx.RegisterCreate(new TestModel("New")); });
            var bus = BuildBus(cmdHandlerMock.Object, builderMock.Object);

            await bus.ExecuteDirect(new TestCommand());

            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()), Times.Once);
            builderMock.Verify(t => t.Create(new TestModel("New")), Times.Once);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task ExecuteForValue_WithSimpleCommandWithChanges_IsExecutedWithBuilders()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<IValueCommandHandler<TestModel>>();
            cmdHandlerMock.Setup(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<ValueCommand<TestModel>>()))
                .Callback<ExecutionContext, ValueCommand<TestModel>>((ctx, cmd) => { ctx.RegisterCreate(new TestModel("New " + cmd.Value.Name)); });
            var bus = BuildBus(cmdHandlerMock.Object, builderMock.Object);

            var result = await bus.ExecuteForValue(new TestModel("Value"));

            result.Successful.Should().BeTrue();
            result.CommandId.Should().NotBe(Guid.Empty);
            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<ValueCommand<TestModel>>()), Times.Once);
            builderMock.Verify(t => t.Create(new TestModel("New Value")), Times.Once);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task Execute_WithCommandFailingAuthorization_IsRejected()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<IFullCommandHandler<TestCommand>>();
            cmdHandlerMock.Setup(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestCommand>()))
                .Callback<AuthorizationContext, TestCommand>((ctx, cmd) => ctx.Fail());
            var bus = BuildBus(cmdHandlerMock.Object, builderMock.Object);

            var result = await bus.Execute(new TestCommand());

            result.Successful.Should().BeFalse();
            result.CommandId.Should().Be(TestCommand.Id);
            cmdHandlerMock.Verify(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestCommand>()), Times.Once);
            cmdHandlerMock.Verify(t => t.Validate(It.IsAny<ValidationContext>(), It.IsAny<TestCommand>()), Times.Never);
            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()), Times.Never);
            builderMock.Verify(t => t.Create(It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        [Fact]
        public async Task Execute_WithCommandFailingValidation_IsRejected()
        {
            var builderMock = new Mock<TestBuilder1>();
            var cmdHandlerMock = new Mock<IFullCommandHandler<TestCommand>>();
            cmdHandlerMock.Setup(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestCommand>()))
                .Callback<AuthorizationContext, TestCommand>((ctx, cmd) => ctx.Succeed());
            cmdHandlerMock.Setup(t => t.Validate(It.IsAny<ValidationContext>(), It.IsAny<TestCommand>()))
                .Callback<ValidationContext, TestCommand>((ctx, cmd) => { ctx.AddError("key", "error"); });
            var bus = BuildBus(cmdHandlerMock.Object, builderMock.Object);

            var result = await bus.Execute(new TestCommand());

            result.Successful.Should().BeFalse();
            result.CommandId.Should().Be(TestCommand.Id);
            cmdHandlerMock.Verify(t => t.Authorize(It.IsAny<AuthorizationContext>(), It.IsAny<TestCommand>()), Times.Once);
            cmdHandlerMock.Verify(t => t.Validate(It.IsAny<ValidationContext>(), It.IsAny<TestCommand>()), Times.Once);
            cmdHandlerMock.Verify(t => t.Execute(It.IsAny<ExecutionContext>(), It.IsAny<TestCommand>()), Times.Never);
            builderMock.Verify(t => t.Create(It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Update(It.IsAny<TestModel>(), It.IsAny<TestModel>()), Times.Never);
            builderMock.Verify(t => t.Delete(It.IsAny<TestModel>()), Times.Never);
        }

        private static CommandBus BuildBus(
            ICommandHandler<TestCommand> cmdHandler,
            ITestBuilder builder)
        {
            return BuildBus(cmdHandler, null, null, new ITestBuilder[] { builder });
        }

        private static CommandBus BuildBus(
            ICommandHandler<TestCommand> cmdHandler,
            params ITestBuilder[] builders)
        {
            return BuildBus(cmdHandler, null, null, builders);
        }

        private static CommandBus BuildBus(
            IValueCommandHandler<TestModel> cmdHandler,
            ITestBuilder builder)
        {
            return BuildBus(null, cmdHandler, null, new ITestBuilder[] { builder });
        }

        private static CommandBus BuildBus(
            IValueCommandHandler<TestModel> cmdHandler,
            params ITestBuilder[] builders)
        {
            return BuildBus(null, cmdHandler, null, builders);
        }

        private static CommandBus BuildBus(
            ICommandHandler<TestCommand> cmdHandler,
            ICommandHandler<TestSubcommand>? subcmdHandler,
            params ITestBuilder[] builders)
        {
            return BuildBus(cmdHandler, null, subcmdHandler, builders);
        }

        private static CommandBus BuildBus(
            IValueCommandHandler<TestModel> valueCmdHandler,
            ICommandHandler<TestSubcommand>? subcmdHandler,
            params ITestBuilder[] builders)
        {
            return BuildBus(null, valueCmdHandler, subcmdHandler, builders);
        }

        private static CommandBus BuildBus(
            ICommandHandler<TestCommand>? cmdHandler,
            IValueCommandHandler<TestModel>? valueCmdHandler,
            ICommandHandler<TestSubcommand>? subcmdHandler,
            params ITestBuilder[] builders)
        {
            var services = new ServiceCollection();
            if (cmdHandler != null)
            {
                services.AddSingleton(cmdHandler);

                if (cmdHandler is ICommandAuthorizer<TestCommand> cmdAuth)
                {
                    services.AddSingleton(cmdAuth);
                }

                if (cmdHandler is ICommandValidator<TestCommand> cmdValidate)
                {
                    services.AddSingleton(cmdValidate);
                }
            }

            if (valueCmdHandler != null)
            {
                services.AddSingleton<ICommandHandler<ValueCommand<TestModel>>>(valueCmdHandler);

                if (valueCmdHandler is ICommandAuthorizer<ValueCommand<TestModel>> cmdAuth)
                {
                    services.AddSingleton(cmdAuth);
                }

                if (valueCmdHandler is ICommandValidator<ValueCommand<TestModel>> cmdValidate)
                {
                    services.AddSingleton(cmdValidate);
                }
            }

            if (subcmdHandler != null)
            {
                services.AddSingleton(subcmdHandler);
            }

            foreach (var builder in builders)
            {
                services.AddSingleton(builder.GetType(), builder);
            }

            return new CommandBus(
                services.BuildServiceProvider(),
                new DummyClaimsPrincipleProvider(),
                new DummyTransactionFactory(),
                new BuilderCollection(builders.Select(t => t.GetType())));
        }

        public sealed class TestCommand : ICommand
        {
            public static readonly Guid Id = new("82a18736-b10f-451e-bfb8-1ee7703fd6c3");

            public Guid CommandId { get; } = Id;
        }

        public sealed class TestSubcommand : ICommand
        {
            public static readonly Guid Id = new("a498bc87-670b-408f-8cce-6dfac9c1007a");

            public Guid CommandId { get; } = Id;
        }

        public sealed record TestModel(string Name);

        public interface ITestBuilder
        {
            void Create(TestModel value);
            void Update(TestModel? old, TestModel @new);
            void Delete(TestModel value);
        }

        public abstract class TestBuilder1 : ITestBuilder
        {
            public abstract void Create(TestModel value);
            public abstract void Update(TestModel? old, TestModel @new);
            public abstract void Delete(TestModel value);
        }

        public abstract class TestBuilder2 : ITestBuilder
        {
            public abstract void Create(TestModel value);
            public abstract void Update(TestModel? old, TestModel @new);
            public abstract void Delete(TestModel value);
        }
    }
}
