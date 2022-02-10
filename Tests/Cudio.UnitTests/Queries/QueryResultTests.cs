using System;
using System.Security.Claims;
using FluentAssertions;
using Xunit;

namespace Cudio
{
    public class QueryResultTests
    {
        private static readonly ClaimsPrincipal DefaultUser = new(new ClaimsIdentity());

        private static readonly AuthorizationContext AuthDefault = new(DefaultUser);
        private static readonly AuthorizationContext AuthSuccess = new(DefaultUser);
        private static readonly AuthorizationContext AuthFailed = new(DefaultUser);

        private static readonly ValidationContext ValidationSuccess = new();
        private static readonly ValidationContext ValidationFailed = new();

        private static readonly TestQuery Query = new();

        static QueryResultTests()
        {
            AuthSuccess.Succeed();
            AuthFailed.Fail();
            ValidationFailed.AddError("key", "error");
        }

        [Fact]
        public void Ctor_WithValues_SetsProperties()
        {
            var result = new QueryResult<int>(Query, 42, ValidationSuccess, AuthDefault);

            result.Validation.Should().BeSameAs(ValidationSuccess);
            result.Authorization.Should().BeSameAs(AuthDefault);
            result.QueryId.Should().Be(Query.QueryId);
            result.Value.Should().Be(42);
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
            var result = new QueryResult<int>(Query, 42, validationContext, authorizationContext);

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

        private sealed class TestQuery : IQuery
        {
            public Guid QueryId { get; } = new("82a18736-b10f-451e-bfb8-1ee7703fd6c3");
        }
    }
}
