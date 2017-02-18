#if !(NET451 && DRIVER_NOT_SIGNED)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Annotations;
using Microsoft.EntityFrameworkCore.MongoDB.Adapter;
using Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain;
using MongoDB.Bson.Serialization;
using Xunit;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.MongoDB.Adapter
{
    public class BsonClassMapAttributeConventionTests
    {
        [Fact]
        public void Processes_all_instances_of_bson_class_map_attribute()
        {
            var derivedTypeClassMapAttribute = new BsonClassMapAttributeConvention<DerivedTypeAttribute>();
            var bsonClassMap = new BsonClassMap<RootType>();
            derivedTypeClassMapAttribute.Apply(bsonClassMap);
            IList<Type> derivedTypes = typeof(RootType)
                .GetTypeInfo()
                .GetCustomAttributes<DerivedTypeAttribute>()
                .Select(derivedTypeAttribute => derivedTypeAttribute.DerivedType)
                .ToList();
            Assert.True(derivedTypes.Count > 1);
            Assert.All(derivedTypes, derivedType => Assert.True(bsonClassMap.KnownTypes.Contains(derivedType)));
        }
    }
}
#endif //!(NET451 && DRIVER_NOT_SIGNED)