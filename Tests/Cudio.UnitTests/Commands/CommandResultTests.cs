using System;
using System.Security.Claims;
using FluentAssertions;
using Xunit;

namespace Cudio
{
    public class CommandResultTests
    {
        private static readonly ClaimsPrincipal DefaultUser = new(new ClaimsIdentity());

        private static readonly AuthorizationContext AuthDefault = new(DefaultUser);
        private static readonly AuthorizationContext AuthSuccess = new(DefaultUser);
        private static readonly AuthorizationContext AuthFailed = new(DefaultUser);

        private static readonly ValidationContext ValidationSuccess = new();
        private static readonly ValidationContext ValidationFailed = new();

        private static readonly TestCommand Command = new();

        static CommandResultTests()
        {
            AuthSuccess.Succeed();
            AuthFailed.Fail();
            ValidationFailed.AddError("key", "error");
        }

        [Fact]
        public void Ctor_WithValues_SetsProperties()
        {
            var result = new CommandResult(Command, ValidationSuccess, AuthDefault);

            result.Validation.Should().BeSameAs(ValidationSuccess);
            result.Authorization.Should().BeSameAs(AuthDefault);
            result.CommandId.Should().Be(Command.CommandId);
        }

        [Theory]
        [MemberData(nameof(TestData))]
        public void Ctor_WithValues_SetsBooleanProperties(
            AuthorizationContext authorizationContext,
            ValidationContext validationContext,
            bool expectedSuccessful,
            bool expectedUnauthorized,
            bool expectedValidationFailed)
        {
            var result = new CommandResult(Command, validationContext, authorizationContext);

            result.Successful.Should().Be(expectedSuccessful);
            result.Unauthorized.Should().Be(expectedUnauthorized);
            result.ValidationFailed.Should().Be(expectedValidationFailed);
        }

        public static readonly object[][] TestData =
        {
            new object[] { AuthDefault, ValidationSuccess, false, true, false },
            new object[] { AuthDefault, ValidationFailed, false, true, true },
            new object[] { AuthSuccess, ValidationSuccess, true, false, false },
            new object[] { AuthSuccess, ValidationFailed, false, false, true },
            new object[] { AuthFailed, ValidationSuccess, false, true, false },
            new object[] { AuthFailed, ValidationFailed, false, true, true },
        };

        private sealed class TestCommand : ICommand
        {
            public Guid CommandId { get; } = new("82a18736-b10f-451e-bfb8-1ee7703fd6c3");
        }
    }
}
