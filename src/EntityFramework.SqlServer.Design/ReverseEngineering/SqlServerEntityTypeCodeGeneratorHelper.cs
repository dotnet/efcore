// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Utilities;

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

        public virtual IEnumerable<SqlServerNavPropInitializer> NavPropInitializers
        {
            get
            {
                var navPropInitializers = new List<SqlServerNavPropInitializer>();

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
                                    new SqlServerNavPropInitializer(navigationPropertyName, otherEntityType.Name));
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

    public class SqlServerNavigationProperty
    {
        public SqlServerNavigationProperty([NotNull]string errorAnnotation)
        {
            Check.NotEmpty(errorAnnotation, nameof(errorAnnotation));

            ErrorAnnotation = errorAnnotation;
        }

        public SqlServerNavigationProperty([NotNull]string type, [NotNull]string name)
        {
            Check.NotNull(type, nameof(type));
            Check.NotEmpty(name, nameof(name));

            Type = type;
            Name = name;
        }

        public virtual string ErrorAnnotation { get;[param: NotNull]private set; }
        public virtual string Type { get;[param: NotNull]private set; }
        public virtual string Name { get;[param: NotNull]private set; }
    }

    public class SqlServerNavPropInitializer
    {
        public SqlServerNavPropInitializer([NotNull]string navPropName, [NotNull]string principalEntityTypeName)
        {
            Check.NotEmpty(navPropName, nameof(navPropName));
            Check.NotEmpty(principalEntityTypeName, nameof(principalEntityTypeName));

            NavigationPropertyName = navPropName;
            PrincipalEntityTypeName = principalEntityTypeName;
        }

        public virtual string NavigationPropertyName { get;[param: NotNull]private set; }
        public virtual string PrincipalEntityTypeName { get;[param: NotNull]private set; }
    }
}