#if !(NET451 && DRIVER_NOT_SIGNED)
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Bson;
using Xunit;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.ValueGeneration
{
    public class TemporaryObjectIdValueGeneratorTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Generates_temporary_values_returns_constructor_parameter(bool generatesTemporaryValues)
        {
            var tmeporaryObjectIdValueGenerator = new ObjectIdValueGenerator(generatesTemporaryValues);
            Assert.Equal(generatesTemporaryValues, tmeporaryObjectIdValueGenerator.GeneratesTemporaryValues);
        }

        [Theory]
        public void Generates_temporary_values_defaults_to_false()
        {
            var tmeporaryObjectIdValueGenerator = new ObjectIdValueGenerator();
            Assert.False(tmeporaryObjectIdValueGenerator.GeneratesTemporaryValues);
        }

        [Fact]
        public void Does_not_generate_empty_object_id()
        {
            var tmeporaryObjectIdValueGenerator = new ObjectIdValueGenerator();
            Assert.NotEqual(ObjectId.Empty, tmeporaryObjectIdValueGenerator.Next(entry: null));
        }

        [Fact]
        public void Generates_unique_object_ids()
        {
            var tmeporaryObjectIdValueGenerator = new ObjectIdValueGenerator();
            ObjectId id = ObjectId.Empty;
            for (var i = 0; i < 100; i++)
            {
                Assert.NotEqual(id, id = tmeporaryObjectIdValueGenerator.Next(entry: null));
            }
        }
    }
}
#endif //!(NET451 && DRIVER_NOT_SIGNED)