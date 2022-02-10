using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Xunit;

namespace Cudio.AspNetCore
{
    public class ModelStateExtensionsTests
    {
        [Fact]
        public void ToModelState_WithSingleError_AppliesErrorToModelState()
        {
            var ctx = new ValidationContext();
            ctx.AddError("key", "Error");
            var state = new ModelStateDictionary();

            ctx.ToModelState(state);

            state.ErrorCount.Should().Be(1);
            state.Should().ContainKey("key")
                .WhoseValue!.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("Error");
        }

        [Fact]
        public void ToModelState_WithMultipleErrors_AppliesErrorsToModelState()
        {
            var ctx = new ValidationContext();
            ctx.AddError("key1", "Error 1");
            ctx.AddError("key2", "Error 2");
            var state = new ModelStateDictionary();

            ctx.ToModelState(state);

            state.ErrorCount.Should().Be(2);
            state.Should().ContainKey("key1")
                .WhoseValue!.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("Error 1");
            state.Should().ContainKey("key2")
                .WhoseValue!.Errors.Should().ContainSingle()
                .Which.ErrorMessage.Should().Be("Error 2");
        }

        [Fact]
        public void ToModelState_WithMultipleErrorsWithSameKey_AppliesErrorsToModelState()
        {
            var ctx = new ValidationContext();
            ctx.AddError("key", "Error 1");
            ctx.AddError("key", "Error 2");
            var state = new ModelStateDictionary();

            ctx.ToModelState(state);

            state.ErrorCount.Should().Be(2);
            state.Should().ContainKey("key")
                .WhoseValue!.Errors.Should().SatisfyRespectively(
                    t => t.ErrorMessage.Should().Be("Error 1"),
                    t => t.ErrorMessage.Should().Be("Error 2"));
        }
    }
}
