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
            [NotNull]ReverseEngineeringGenerator generator,
            [NotNull]IEntityType entityType,
            [CanBeNull]string namespaceName)
            : base(generator, entityType, namespaceName)
        {
        }

        public override void Generate(IndentedStringBuilder sb)
        {
            var errorMessageAnnotation =
                EntityType[SqlServerMetadataModelProvider.AnnotationNameEntityTypeError];
            if (errorMessageAnnotation != null)
            {
                GenerateCommentHeader(sb);
                CSharpCodeGeneratorHelper.Instance.SingleLineComment(errorMessageAnnotation, sb);
                Generator.Logger.WriteWarning("The SQL Server EntityType CodeGenerator"
                    + " is unable to generate EntityType " + EntityType.Name
                    + ". Error message: " + errorMessageAnnotation);

                return;
            }

            base.Generate(sb);
        }

        public override void GenerateZeroArgConstructorContents(IndentedStringBuilder sb)
        {
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
                            sb.Append(otherEntityType.Name);
                            sb.AppendLine(">();");
                        }
                    }
                }
            }
        }

        public override void GenerateEntityProperties(IndentedStringBuilder sb)
        {
            sb.AppendLine();
            CSharpCodeGeneratorHelper.Instance.SingleLineComment("Properties", sb);
            base.GenerateEntityProperties(sb);
        }

        public override void GenerateEntityProperty(IProperty property, IndentedStringBuilder sb)
        {
            GenerateEntityPropertyAttribues(sb, property);

            CSharpCodeGeneratorHelper.Instance.AddProperty(AccessModifier.Public,
                VirtualModifier.None, property.PropertyType, property.Name, sb);
        }

        public override void GenerateEntityNavigations(IndentedStringBuilder sb)
        {
            sb.AppendLine();
            CSharpCodeGeneratorHelper.Instance.SingleLineComment("Navigation Properties", sb);

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
                        CSharpCodeGeneratorHelper.Instance.SingleLineComment("Unable to add a Navigation Property referencing type "
                            + otherEntityType.Name + " because of errors generating that EntityType.",
                            sb);
                    }
                    else
                    {
                        if (foreignKey.IsUnique)
                        {
                            CSharpCodeGeneratorHelper.Instance.AddProperty(
                                AccessModifier.Public,
                                VirtualModifier.Virtual,
                                otherEntityType.Name,
                                navigationPropertyName,
                                sb);
                        }
                        else
                        {
                            CSharpCodeGeneratorHelper.Instance.AddProperty(
                                AccessModifier.Public,
                                VirtualModifier.Virtual,
                                "ICollection<" + otherEntityType.Name + ">",
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
                CSharpCodeGeneratorHelper.Instance.AddProperty(
                    AccessModifier.Public,
                    VirtualModifier.Virtual,
                    foreignKey.ReferencedEntityType.Name,
                    navigationPropertyName,
                    sb);
            }
        }

        public virtual void GenerateEntityPropertyAttribues(IndentedStringBuilder sb, IProperty property)
        {
            //TODO: to use when we the runtime recognizes and uses DataAnnotations
        }
    }
}
