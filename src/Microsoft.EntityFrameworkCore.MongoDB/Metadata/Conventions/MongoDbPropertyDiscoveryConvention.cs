using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using MongoDB.Bson;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class MongoDbPropertyDiscoveryConvention : PropertyDiscoveryConvention
    {
        protected override bool IsCandidatePrimitiveProperty(PropertyInfo propertyInfo)
            => propertyInfo.PropertyType == typeof(ObjectId) ||
                (propertyInfo.PropertyType.TryGetSequenceType() ?? propertyInfo.PropertyType).GetTypeInfo().IsDefined(typeof(ComplexTypeAttribute)) ||
                base.IsCandidatePrimitiveProperty(propertyInfo);
    }
}