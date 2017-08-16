using Xunit;

namespace Microsoft.EntityFrameworkCore.Tools
{
    public class JsonTest
    {
        [Fact]
        public void Literal_escapes()
        {
            Assert.Equal("\"test\\\\test\\\"test\"", Json.Literal("test\\test\"test"));
        }

        [Fact]
        public void Literal_handles_null()
        {
            Assert.Equal("null", Json.Literal(null));
        }
    }
}
