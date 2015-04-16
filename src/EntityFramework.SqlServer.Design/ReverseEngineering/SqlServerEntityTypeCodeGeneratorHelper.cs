// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
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
            [NotNull]EntityTypeGeneratorModel generatorModel)
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
                        var navigationPropertyName =
                            (string)foreignKey[SqlServerMetadataModelProvider.AnnotationNamePrincipalEndNavPropName];
                        if (((EntityType)otherEntityType)
                            .FindAnnotation(SqlServerMetadataModelProvider.AnnotationNameEntityTypeError) != null)
                        {
                            navProps.Add(new SqlServerNavigationProperty(
                                Strings.UnableToAddNavigationProperty(otherEntityType.Name)));
                        }
                        else
                        {
                            if (foreignKey.IsUnique)
                            {
                                navProps.Add(new SqlServerNavigationProperty(
                                    otherEntityType.Name, navigationPropertyName));
                            }
                            else
                            {
                                navProps.Add(new SqlServerNavigationProperty(
                                    "ICollection<" + otherEntityType.Name + ">", navigationPropertyName));
                            }
                        }
                    }
                }

                foreach (var foreignKey in GeneratorModel.EntityType.GetForeignKeys())
                {
                    // set up the navigation property on this end of foreign keys owned by this EntityType
                    var navigationPropertyName =
                        (string)foreignKey[SqlServerMetadataModelProvider.AnnotationNameDependentEndNavPropName];
                    navProps.Add(new SqlServerNavigationProperty(
                        foreignKey.PrincipalEntityType.Name, navigationPropertyName));
                }

                return navProps;
            }
        }
    }
}