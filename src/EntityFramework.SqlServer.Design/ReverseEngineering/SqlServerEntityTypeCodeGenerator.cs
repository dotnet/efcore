// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerEntityTypeCodeGenerator : EntityTypeCodeGenerator
    {
        public SqlServerEntityTypeCodeGenerator(
            [NotNull] ReverseEngineeringGenerator generator,
            [NotNull] IEntityType entityType,
            [NotNull] string namespaceName)
            : base(generator, entityType, namespaceName)
        {
            Check.NotNull(generator, nameof(generator));
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(namespaceName, nameof(namespaceName));
        }

        public override void Generate(IndentedStringBuilder sb)
        {
            Check.NotNull(sb, nameof(sb));

            var errorMessageAnnotation =
                EntityType[SqlServerMetadataModelProvider.AnnotationNameEntityTypeError];
            if (errorMessageAnnotation != null)
            {
                GenerateCommentHeader(sb);
                Generator.CSharpCodeGeneratorHelper.SingleLineComment(errorMessageAnnotation, sb);
                Generator.Logger.LogWarning(
                    Strings.CannotGenerateEntityType(EntityType.FullName, errorMessageAnnotation));

                return;
            }

            base.Generate(sb);
        }

        public override void GenerateZeroArgConstructorContents(IndentedStringBuilder sb)
        {
            Check.NotNull(sb, nameof(sb));

            foreach (var otherEntityType in EntityType.Model.EntityTypes.Where(et => et != EntityType))
            {
                // find navigation properties for foreign keys from another EntityType which reference this EntityType
                foreach (var foreignKey in otherEntityType
                    .ForeignKeys.Where(fk => fk.ReferencedEntityType == EntityType))
                {
                    var navigationPropertyName =
                        foreignKey[SqlServerMetadataModelProvider.AnnotationNamePrincipalEndNavPropName];
                    if (((EntityType)otherEntityType)
                        .TryGetAnnotation(SqlServerMetadataModelProvider.AnnotationNameEntityTypeError) == null)
                    {
                        if (!foreignKey.IsUnique)
                        {
                            sb.Append(navigationPropertyName);
                            sb.Append(" = new HashSet<");
                            sb.Append(otherEntityType.FullName);
                            sb.AppendLine(">();");
                        }
                    }
                }
            }
        }

        public override void GenerateEntityProperties(IndentedStringBuilder sb)
        {
            Check.NotNull(sb, nameof(sb));

            sb.AppendLine();
            Generator.CSharpCodeGeneratorHelper.SingleLineComment("Properties", sb);
            base.GenerateEntityProperties(sb);
        }

        public override void GenerateEntityProperty(IProperty property, IndentedStringBuilder sb)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(sb, nameof(sb));

            GenerateEntityPropertyAttribues(property, sb);

            Generator.CSharpCodeGeneratorHelper.AddProperty(AccessModifier.Public,
                VirtualModifier.None, property.PropertyType, property.Name, sb);
        }

        public override void GenerateEntityNavigations(IndentedStringBuilder sb)
        {
            Check.NotNull(sb, nameof(sb));

            sb.AppendLine();
            Generator.CSharpCodeGeneratorHelper.SingleLineComment("Navigation Properties", sb);

            // construct navigations from foreign keys
            foreach (var otherEntityType in EntityType.Model.EntityTypes.Where(et => et != EntityType))
            {
                // set up the navigation properties for foreign keys from another EntityType which reference this EntityType
                foreach (var foreignKey in otherEntityType
                    .ForeignKeys.Where(fk => fk.ReferencedEntityType == EntityType))
                {
                    var navigationPropertyName =
                        foreignKey[SqlServerMetadataModelProvider.AnnotationNamePrincipalEndNavPropName];
                    if (((EntityType)otherEntityType)
                        .TryGetAnnotation(SqlServerMetadataModelProvider.AnnotationNameEntityTypeError) != null)
                    {
                        Generator.CSharpCodeGeneratorHelper.SingleLineComment("Unable to add a Navigation Property referencing type "
                            + otherEntityType.FullName + " because of errors generating that EntityType.",
                            sb);
                    }
                    else
                    {
                        if (foreignKey.IsUnique)
                        {
                            Generator.CSharpCodeGeneratorHelper.AddProperty(
                                AccessModifier.Public,
                                VirtualModifier.Virtual,
                                otherEntityType.FullName,
                                navigationPropertyName,
                                sb);
                        }
                        else
                        {
                            Generator.CSharpCodeGeneratorHelper.AddProperty(
                                AccessModifier.Public,
                                VirtualModifier.Virtual,
                                "ICollection<" + otherEntityType.FullName + ">",
                                navigationPropertyName,
                                sb);
                        }
                    }
                }
            }

            foreach (var foreignKey in EntityType.ForeignKeys)
            {
                // set up the navigation property on this end of foreign keys owned by this EntityType
                var navigationPropertyName =
                    foreignKey[SqlServerMetadataModelProvider.AnnotationNameDependentEndNavPropName];
                Generator.CSharpCodeGeneratorHelper.AddProperty(
                    AccessModifier.Public,
                    VirtualModifier.Virtual,
                    foreignKey.ReferencedEntityType.FullName,
                    navigationPropertyName,
                    sb);
            }
        }

        public virtual void GenerateEntityPropertyAttribues(
            [NotNull] IProperty property, [NotNull] IndentedStringBuilder sb)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(sb, nameof(sb));

            //TODO: to use when we the runtime recognizes and uses DataAnnotations
        }
    }
}
