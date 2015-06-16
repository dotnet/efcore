// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering.Configuration;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerEntityTypeCodeGeneratorHelper : EntityTypeCodeGeneratorHelper
    {
        public SqlServerEntityTypeCodeGeneratorHelper(
            [NotNull] EntityTypeGeneratorModel generatorModel)
            : base(generatorModel)
        {
        }

        public virtual string ErrorMessageAnnotation
        {
            get
            {
                return (string)GeneratorModel
                    .EntityType[SqlServerMetadataModelProvider.AnnotationNameEntityTypeError];
            }
        }

        public virtual IEnumerable<SqlServerNavigationPropertyInitializer> NavPropInitializers
        {
            get
            {
                var navPropInitializers = new List<SqlServerNavigationPropertyInitializer>();

                foreach (var otherEntityType in GeneratorModel.EntityType.Model
                    .EntityTypes.Where(et => et != GeneratorModel.EntityType))
                {
                    // find navigation properties for foreign keys from another EntityType which reference this EntityType
                    foreach (var foreignKey in otherEntityType
                        .GetForeignKeys().Where(fk => fk.PrincipalEntityType == GeneratorModel.EntityType))
                    {
                        var navigationPropertyName =
                            (string)foreignKey[SqlServerMetadataModelProvider.AnnotationNamePrincipalEndNavPropName];
                        if (((EntityType)otherEntityType)
                            .FindAnnotation(SqlServerMetadataModelProvider.AnnotationNameEntityTypeError) == null)
                        {
                            if (!foreignKey.IsUnique)
                            {
                                navPropInitializers.Add(
                                    new SqlServerNavigationPropertyInitializer(
                                        navigationPropertyName, otherEntityType.Name));
                            }
                        }
                    }
                }

                return navPropInitializers;
            }
        }

        public virtual IEnumerable<SqlServerNavigationProperty> NavigationProperties
        {
            get
            {
                var navProps = new List<SqlServerNavigationProperty>();

                foreach (var otherEntityType in GeneratorModel.EntityType.Model
                    .EntityTypes.Where(et => et != GeneratorModel.EntityType))
                {
                    // set up the navigation properties for foreign keys from
                    // another EntityType which reference this EntityType
                    foreach (var foreignKey in otherEntityType
                        .GetForeignKeys().Where(fk => fk.PrincipalEntityType == GeneratorModel.EntityType))
                    {
                        if (((EntityType)otherEntityType)
                            .FindAnnotation(SqlServerMetadataModelProvider.AnnotationNameEntityTypeError) != null)
                        {
                            navProps.Add(new SqlServerNavigationProperty(
                                Strings.UnableToAddNavigationProperty(otherEntityType.Name)));
                        }
                        else
                        {
                            var referencedType = foreignKey.IsUnique
                                ? otherEntityType.Name
                                : "ICollection<" + otherEntityType.Name + ">";
                            navProps.Add(new SqlServerNavigationProperty(
                                referencedType,
                                (string)foreignKey[SqlServerMetadataModelProvider.AnnotationNamePrincipalEndNavPropName]));
                        }
                    }
                }

                foreach (var foreignKey in GeneratorModel.EntityType.GetForeignKeys())
                {
                    // set up the navigation property on this end of foreign keys owned by this EntityType
                    navProps.Add(new SqlServerNavigationProperty(
                        foreignKey.PrincipalEntityType.Name,
                        (string)foreignKey[SqlServerMetadataModelProvider.AnnotationNameDependentEndNavPropName]));

                    // set up the other navigation property for self-referencing foreign keys owned by this EntityType
                    if (((ForeignKey)foreignKey).IsSelfReferencing())
                    {
                        var referencedType = foreignKey.IsUnique
                            ? foreignKey.EntityType.Name
                            : "ICollection<" + foreignKey.EntityType.Name + ">";
                        navProps.Add(new SqlServerNavigationProperty(
                            referencedType,
                            (string)foreignKey[SqlServerMetadataModelProvider.AnnotationNamePrincipalEndNavPropName]));
                    }
                }

                return navProps;
            }
        }
    }
}
