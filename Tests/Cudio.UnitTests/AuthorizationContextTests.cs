using System.Security.Claims;
using FluentAssertions;
using Xunit;

namespace Cudio
{
    public class AuthorizationContextTests
    {
        private static readonly ClaimsPrincipal DefaultUser = new(new ClaimsIdentity());

        [Fact]
        public void Ctor_WithDefaultuser_HasNotSucceededOrFailed()
        {
            var ctx = new AuthorizationContext(DefaultUser);

            ctx.HasSucceeded.Should().BeFalse();
            ctx.HasFailed.Should().BeFalse();
        }

        [Fact]
        public void Succeed_WithoutParameters_SetsHasSucceededTrueAndHasFailedStaysFalse()
        {
            var ctx = new AuthorizationContext(DefaultUser);

            ctx.Succeed();

            ctx.HasSucceeded.Should().BeTrue();
            ctx.HasFailed.Should().BeFalse();
        }

        [Fact]
        public void Succeed_AfterCallingFail_KeepsHasSucceededFalseAndHasFailedTrue()
        {
            var ctx = new AuthorizationContext(DefaultUser);
            ctx.Fail();

            ctx.Succeed();

            ctx.HasSucceeded.Should().BeFalse();
            ctx.HasFailed.Should().BeTrue();
        }

        [Fact]
        public void Fail_WithoutParameters_SetsHasFailedTrueAndHasSucceededFalse()
        {
            var ctx = new AuthorizationContext(DefaultUser);

            ctx.Fail();

            ctx.HasFailed.Should().BeTrue();
            ctx.HasSucceeded.Should().BeFalse();
        }

        [Fact]
        public void Fail_AfterCallingSucceed_SetsHasFailedTrueAndHasSucceededFalse()
        {
            var ctx = new AuthorizationContext(DefaultUser);
            ctx.Succeed();

            ctx.Fail();

            ctx.HasFailed.Should().BeTrue();
            ctx.HasSucceeded.Should().BeFalse();
        }
    }
}
