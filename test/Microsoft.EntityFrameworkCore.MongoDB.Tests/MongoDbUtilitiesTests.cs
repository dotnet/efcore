using Xunit;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests
{
    public class MongoDbUtilitiesTests
    {
        [Theory]
        [InlineData("monkey", "monkeys")]
        [InlineData("laptop", "laptops")]
        [InlineData("cpu", "cpus")]
        [InlineData("horse", "horses")]
        [InlineData("pony", "ponies")]
        public void Pluralize_singular_strings(string value, string expected)
            => Assert.Equal(expected, MongoDbUtilities.Pluralize(value));

        [Theory]
        [InlineData("monkeys")]
        [InlineData("horses")]
        [InlineData("ponies")]
        public void Pluralize_doesnt_change_plurals(string value)
            => Assert.Equal(value, MongoDbUtilities.Pluralize(value));

        [Theory]
        [InlineData("CPU", "cpu")]
        [InlineData("ETA", "eta")]
        [InlineData("EPA", "epa")]
        [InlineData("TLA", "tla")]
        public void Camel_case_uppercase_strings(string value, string expected)
            => Assert.Equal(expected, MongoDbUtilities.ToCamelCase(value));

        [Theory]
        [InlineData("EFTests", "efTests")]
        [InlineData("NYCity", "nyCity")]
        [InlineData("TLAcronym", "tlAcronym")]
        [InlineData("ThreeLetterAcronym", "threeLetterAcronym")]
        public void Camel_case_doesnt_change_trailing_words(string value, string expected)
            => Assert.Equal(expected, MongoDbUtilities.ToCamelCase(value));
    }
}