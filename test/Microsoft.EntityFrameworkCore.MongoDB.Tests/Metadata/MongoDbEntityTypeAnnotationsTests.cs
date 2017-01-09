#if !(NET451 && DRIVER_NOT_SIGNED)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain;
using Xunit;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.Metadata
{
    public class MongoDbEntityTypeAnnotationsTests
    {
        [Fact]
        public void Collection_name_is_pluralized_camel_cased_entity_type_by_default()
        {
            var model = new Model();
            var entityType = new EntityType(typeof(RootType), model, ConfigurationSource.Explicit);
            var mongoDbEntityTypeAnnotations = new MongoDbEntityTypeAnnotations(entityType);
            Assert.Equal(MongoDbUtilities.Pluralize(MongoDbUtilities.ToCamelCase(nameof(RootType))),
                mongoDbEntityTypeAnnotations.CollectionName);
        }

        [Fact]
        public void Can_write_collection_name()
        {
            var collectionName = "myCollection";
            var model = new Model();
            var entityType = new EntityType(typeof(RootType), model, ConfigurationSource.Explicit);
            var mongoDbEntityTypeAnnotations = new MongoDbEntityTypeAnnotations(entityType)
            {
                CollectionName = collectionName
            };
            Assert.Equal(collectionName, mongoDbEntityTypeAnnotations.CollectionName);
        }

        [Fact]
        public void Discriminator_is_type_name_by_default()
        {
            var model = new Model();
            var entityType = new EntityType(typeof(RootType), model, ConfigurationSource.Explicit);
            var mongoDbEntityTypeAnnotations = new MongoDbEntityTypeAnnotations(entityType);
            Assert.Equal(typeof(RootType).Name, mongoDbEntityTypeAnnotations.Discriminator);
        }

        [Fact]
        public void Can_write_discriminator()
        {
            var discriminator = "discriminator";
            var model = new Model();
            var entityType = new EntityType(typeof(RootType), model, ConfigurationSource.Explicit);
            var mongoDbEntityTypeAnnotations = new MongoDbEntityTypeAnnotations(entityType)
            {
                Discriminator = discriminator
            };
            Assert.Equal(discriminator, mongoDbEntityTypeAnnotations.Discriminator);
        }
    }
}
#endif //!(NET451 && DRIVER_NOT_SIGNED)