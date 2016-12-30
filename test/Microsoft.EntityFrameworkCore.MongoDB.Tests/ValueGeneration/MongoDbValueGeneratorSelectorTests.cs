using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Xunit;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.ValueGeneration
{
    public class MongoDbValueGeneratorSelectorTests
    {
        [Fact]
        public void X()
        {
            var model = new Model();
            EntityType entityType = model.AddEntityType(typeof(SimpleRecord));
            Property property = entityType.AddProperty(typeof(SimpleRecord)
                .GetTypeInfo()
                .GetProperty(nameof(SimpleRecord.Id)));            

            var mongoDbValueGeneratorSelector = new MongoDbValueGeneratorSelector(new MongoDbValueGeneratorCache());
            ValueGenerator valueGenerator = mongoDbValueGeneratorSelector.Select(property, entityType);
            Assert.IsAssignableFrom(typeof(ObjectIdValueGenerator), valueGenerator);
        }
    }
}