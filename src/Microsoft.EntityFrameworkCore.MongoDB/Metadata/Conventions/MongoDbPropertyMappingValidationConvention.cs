using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using MongoDB.Bson;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class MongoDbPropertyMappingValidationConvention : PropertyMappingValidationConvention
    {
        public override Type FindCandidateNavigationPropertyType(PropertyInfo propertyInfo)
            => propertyInfo.IsDefined(typeof(ComplexTypeAttribute)) ||
                (propertyInfo.PropertyType.TryGetSequenceType() ?? propertyInfo.PropertyType).GetTypeInfo().IsDefined(typeof(ComplexTypeAttribute))
                ? null
                : base.FindCandidateNavigationPropertyType(propertyInfo);

        public override bool IsMappedPrimitiveProperty(Type clrType)
            => clrType == typeof(ObjectId) ||
                (clrType.TryGetSequenceType() ?? clrType).GetTypeInfo().IsDefined(typeof(ComplexTypeAttribute)) ||
                base.IsMappedPrimitiveProperty(clrType);
    }
}