// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer.ReverseEngineering
{
    public class SqlServerEntityTypeCodeGeneratorContext : EntityTypeCodeGenerator
    {
        public SqlServerEntityTypeCodeGeneratorContext(
            [NotNull]ReverseEngineeringGenerator generator,
            [NotNull]IEntityType entityType,
            [CanBeNull]string namespaceName)
            : base(generator, entityType, namespaceName)
        {
        }

        public override void GenerateEntityProperties(IndentedStringBuilder sb)
        {
            CSharpCodeGeneratorHelper.Instance.SingleLineComment(sb, "Properties");
            base.GenerateEntityProperties(sb);
        }

        public override void GenerateEntityProperty(IndentedStringBuilder sb, IProperty property)
        {
            GenerateEntityPropertyAttribues(sb, property);

            CSharpCodeGeneratorHelper.Instance.AddProperty(sb,
                AccessModifier.Public, VirtualModifier.None, property.PropertyType, property.Name);
        }

        public override void GenerateEntityNavigations(IndentedStringBuilder sb)
        {
            sb.AppendLine();
            CSharpCodeGeneratorHelper.Instance.SingleLineComment(sb, "Navigation Properties");

            var existingIdentifiers = new List<string>();
            existingIdentifiers.Add(EntityType.Name);
            existingIdentifiers.AddRange(EntityType.Properties.Select(p => p.Name));

            // construct navigations from foreign keys
            foreach (var otherEntityType in EntityType.Model.EntityTypes.Where(et => et != EntityType))
            {
                // set up the navigation properties for foreign keys from another EntityType which reference this EntityType
                foreach (var foreignKey in otherEntityType.ForeignKeys.Where(fk => fk.ReferencedEntityType == EntityType).Cast<ForeignKey>())
                {
                    var navigationPropertyName = foreignKey
                        .GetAnnotation(SqlServerMetadataModelProvider.AnnotationNamePrincipalEndNavPropName).Value;
                    CSharpCodeGeneratorHelper.Instance.AddProperty(sb,
                        AccessModifier.Public, VirtualModifier.Virtual, "ICollection<" + otherEntityType.Name + ">", navigationPropertyName);
                }
            }

            foreach (var foreignKey in EntityType.ForeignKeys.Cast<ForeignKey>())
            {
                // set up the navigation property on this end of foreign keys on this EntityType
                var navigationPropertyName = foreignKey
                    .GetAnnotation(SqlServerMetadataModelProvider.AnnotationNameDependentEndNavPropName).Value;
                CSharpCodeGeneratorHelper.Instance.AddProperty(sb,
                    AccessModifier.Public, VirtualModifier.Virtual, foreignKey.ReferencedEntityType.Name, navigationPropertyName);
            }
        }

        public virtual void GenerateEntityPropertyAttribues(IndentedStringBuilder sb, IProperty property)
        {
            //TODO: to use when we the runtime recognizes and uses DataAnnotations
        }
    }
}
