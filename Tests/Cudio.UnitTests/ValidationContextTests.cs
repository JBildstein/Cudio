using System.Collections;
using FluentAssertions;
using Xunit;

namespace Cudio
{
    public class ValidationContextTests
    {
        [Fact]
        public void Ctor_WithoutParameters_HasNoErrors()
        {
            var ctx = new ValidationContext();

            ctx.HasErrors.Should().BeFalse();
            ctx.Errors.Should().BeEmpty();
        }

        [Fact]
        public void AddError_WithSingleError_AddsError()
        {
            var ctx = new ValidationContext();

            ctx.AddError("key", "Error");

            ctx.HasErrors.Should().BeTrue();
            ctx.Errors.Should().ContainSingle();
            ctx.Errors.Should().ContainKey("key")
                .WhoseValue.Should().ContainSingle()
                .Which.Should().Be("Error");
        }

        [Fact]
        public void AddError_WithTwoDifferentErrorKeys_AddsBothErrors()
        {
            var ctx = new ValidationContext();

            ctx.AddError("key1", "Error1");
            ctx.AddError("key2", "Error2");

            ctx.HasErrors.Should().BeTrue();
            ctx.Errors.Should().HaveCount(2);
            ctx.Errors.Should().ContainKey("key1")
                .WhoseValue.Should().ContainSingle()
                .Which.Should().Be("Error1");
            ctx.Errors.Should().ContainKey("key2")
                .WhoseValue.Should().ContainSingle()
                .Which.Should().Be("Error2");
        }

        [Fact]
        public void AddError_WithTwoErrorsOfSameKey_AddsBothErrors()
        {
            var ctx = new ValidationContext();

            ctx.AddError("key", "Error1");
            ctx.AddError("key", "Error2");

            ctx.HasErrors.Should().BeTrue();
            ctx.Errors.Should().ContainSingle();
            ctx.Errors.Should().ContainKey("key")
                .WhoseValue.Should().BeEquivalentTo("Error1", "Error2");
        }

        public void ValidationErrorCollectionCtor_WithKey_AssignsKey()
        {
            var collection = new ValidationContext.ValidationErrorCollection("SomeKey");

            collection.Key.Should().Be("SomeKey");
        }
    }
}
