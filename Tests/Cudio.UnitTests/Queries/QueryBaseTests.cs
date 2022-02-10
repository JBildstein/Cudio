using System;
using FluentAssertions;
using Xunit;

namespace Cudio
{
    public class QueryBaseTests
    {
        [Fact]
        public void Ctor_WithoutParameters_AssignsQuerydId()
        {
            var query = new TestQuery();

            query.QueryId.Should().NotBe(Guid.Empty);
        }

        private class TestQuery : QueryBase<int>
        {
        }
    }
}
