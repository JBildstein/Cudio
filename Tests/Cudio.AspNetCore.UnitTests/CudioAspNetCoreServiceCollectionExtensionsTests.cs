using System.Linq;
using System.Reflection;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Cudio.AspNetCore
{
    public class CudioAspNetCoreServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddCudio_WithoutParameters_AddsRequiredServices()
        {
            var services = new ServiceCollection();

            services.AddCudio();

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.Invoking(t => t.GetRequiredService<IHttpContextAccessor>()).Should().NotThrow();
            scope.ServiceProvider.Invoking(t => t.GetRequiredService<IClaimsPrincipleProvider>()).Should().NotThrow().Which.Should().BeOfType<HttpContextClaimsPrincipalProvider>();
        }

        [Fact]
        public void AddCudio_WithAssembly_AddsRequiredServices()
        {
            var services = new ServiceCollection();

            services.AddCudio(this.GetType().Assembly);

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.Invoking(t => t.GetRequiredService<IHttpContextAccessor>()).Should().NotThrow();
            scope.ServiceProvider.Invoking(t => t.GetRequiredService<IClaimsPrincipleProvider>()).Should().NotThrow().Which.Should().BeOfType<HttpContextClaimsPrincipalProvider>();
        }

        [Fact]
        public void AddCudio_WithMultipleAssemblies_AddsRequiredServices()
        {
            var services = new ServiceCollection();

            services.AddCudio(this.GetType().Assembly, typeof(CudioAspNetCoreServiceCollectionExtensions).Assembly);

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.Invoking(t => t.GetRequiredService<IHttpContextAccessor>()).Should().NotThrow();
            scope.ServiceProvider.Invoking(t => t.GetRequiredService<IClaimsPrincipleProvider>()).Should().NotThrow().Which.Should().BeOfType<HttpContextClaimsPrincipalProvider>();
        }

        [Fact]
        public void AddCudio_WithEmptyTypes_AddsRequiredServices()
        {
            var services = new ServiceCollection();

            services.AddCudio(Enumerable.Empty<TypeInfo>());

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            scope.ServiceProvider.Invoking(t => t.GetRequiredService<IHttpContextAccessor>()).Should().NotThrow();
            scope.ServiceProvider.Invoking(t => t.GetRequiredService<IClaimsPrincipleProvider>()).Should().NotThrow().Which.Should().BeOfType<HttpContextClaimsPrincipalProvider>();
        }
    }
}
