using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Microsoft.AspNetCore.Http;
using Xunit;
using Microsoft.AspNetCore.Http.Features;
using System.Threading;
using System.Security.Claims;
using System.Security.Principal;
using FluentAssertions;

namespace Cudio.AspNetCore
{
    public class HttpContextClaimsPrincipalProviderTests
    {
        [Fact]
        public void GetClaimsPrincipal_WithHttpContextUser_ReturnsHttpContextClaimsPrincipal()
        {
            var httpContextProvider = new Mock<IHttpContextAccessor>();
            var httpContext = new Mock<HttpContext>();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new GenericIdentity("John Doe", "TestAuth")));
            httpContext.Setup(t => t.User).Returns(claimsPrincipal);
            httpContextProvider.Setup(t => t.HttpContext).Returns(httpContext.Object);
            var provider = new HttpContextClaimsPrincipalProvider(httpContextProvider.Object);

            var result = provider.GetClaimsPrincipal();

            result.Identity.Should().NotBeNull();
            result.Identity!.Name.Should().Be("John Doe");
        }

        [Fact]
        public void GetClaimsPrincipal_WithoutHttpContextUser_ReturnsDefaultClaimsPrincipal()
        {
            var httpContextProvider = new Mock<IHttpContextAccessor>();
            var httpContext = new Mock<HttpContext>();
            httpContext.Setup(t => t.User).Returns(new ClaimsPrincipal());
            httpContextProvider.Setup(t => t.HttpContext).Returns(httpContext.Object);
            var provider = new HttpContextClaimsPrincipalProvider(httpContextProvider.Object);

            var result = provider.GetClaimsPrincipal();

            result.Identity.Should().BeNull();
        }

        [Fact]
        public void GetClaimsPrincipal_WithoutHttpContext_ReturnsDefaultClaimsPrincipal()
        {
            var httpContextProvider = new Mock<IHttpContextAccessor>();
            httpContextProvider.Setup(t => t.HttpContext).Returns((HttpContext?)null);
            var provider = new HttpContextClaimsPrincipalProvider(httpContextProvider.Object);

            var result = provider.GetClaimsPrincipal();

            result.Identity.Should().BeNull();
        }
    }
}
