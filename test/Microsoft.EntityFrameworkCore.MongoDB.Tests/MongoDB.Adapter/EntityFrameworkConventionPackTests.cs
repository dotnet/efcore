#if !(NET451 && DRIVER_NOT_SIGNED)
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Annotations;
using Microsoft.EntityFrameworkCore.MongoDB.Adapter;
using MongoDB.Bson.Serialization.Conventions;
using Xunit;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests.MongoDB.Adapter
{
    public class EntityFrameworkConventionPackTests
    {
        [Theory]
        [InlineData(typeof(AbstractClassMapConvention))]
        [InlineData(typeof(BsonClassMapAttributeConvention<DerivedTypeAttribute>))]
        [InlineData(typeof(KeyAttributeConvention))]
        [InlineData(typeof(IgnoreEmptyEnumerablesConvention))]
        [InlineData(typeof(IgnoreNullOrEmptyStringsConvention))]
        public void Singleton_contains_default_convention_set(Type conventionType)
        {
            ConventionPack conventionPack = EntityFrameworkConventionPack.Instance;
            Assert.Contains(conventionPack, conventionType.GetTypeInfo().IsInstanceOfType);
        }
    }
}
#endif //!(NET451 && DRIVER_NOT_SIGNED)