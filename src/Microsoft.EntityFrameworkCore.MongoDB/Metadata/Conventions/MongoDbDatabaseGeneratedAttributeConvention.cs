using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    public class MongoDbDatabaseGeneratedAttributeConvention : DatabaseGeneratedAttributeConvention
    {
        public override InternalPropertyBuilder Apply(
            InternalPropertyBuilder propertyBuilder,
            DatabaseGeneratedAttribute attribute,
            MemberInfo clrMember)
        {
            if (attribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
            {
                propertyBuilder.Metadata
                    .DeclaringEntityType
                    .Builder
                    .MongoDb(ConfigurationSource.Convention)
                    .AssignIdOnInsert(assignIdOnInsert: true);
            }
            return base.Apply(propertyBuilder, attribute, clrMember);
        }
    }
}