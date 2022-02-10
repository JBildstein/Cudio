using System;
using FluentAssertions;
using Xunit;

namespace Cudio
{
    public class CommandBaseTests
    {
        [Fact]
        public void Ctor_WithoutParameters_AssignsCommandId()
        {
            var command = new TestCommand();

            command.CommandId.Should().NotBe(Guid.Empty);
        }

        private class TestCommand : CommandBase
        {
        }
    }
}
