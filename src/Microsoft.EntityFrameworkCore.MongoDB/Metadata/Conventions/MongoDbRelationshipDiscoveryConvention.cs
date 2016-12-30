using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class MongoDbRelationshipDiscoveryConvention : RelationshipDiscoveryConvention
    {
        public override Type FindCandidateNavigationPropertyType(PropertyInfo propertyInfo)
            => propertyInfo.IsDefined(typeof(ComplexTypeAttribute)) ||
                (propertyInfo.PropertyType.TryGetSequenceType() ?? propertyInfo.PropertyType).GetTypeInfo().IsDefined(typeof(ComplexTypeAttribute))
                ? null
                : base.FindCandidateNavigationPropertyType(propertyInfo);
    }
}