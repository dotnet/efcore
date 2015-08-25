// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public class EntityTypeCodeGeneratorHelper
    {
        private readonly ModelUtilities _modelUtilities;

        public EntityTypeCodeGeneratorHelper(
            [NotNull] EntityTypeGeneratorModel generatorModel,
            [NotNull] ModelUtilities modelUtilities)
        {
            Check.NotNull(generatorModel, nameof(generatorModel));
            Check.NotNull(modelUtilities, nameof(modelUtilities));

            GeneratorModel = generatorModel;
            _modelUtilities = modelUtilities;
        }

        public virtual EntityTypeGeneratorModel GeneratorModel { get; }

        public virtual IEnumerable<IProperty> OrderedEntityProperties
            => _modelUtilities.OrderedProperties(GeneratorModel.EntityType);

        public virtual string ErrorMessageAnnotation
            => (string)GeneratorModel
                .EntityType[RelationalMetadataModelProvider.AnnotationNameEntityTypeError];

        public virtual IEnumerable<NavigationPropertyInitializerConfiguration> NavPropInitializers
        {
            get
            {
                var navPropInitializers = new List<NavigationPropertyInitializerConfiguration>();

                foreach (var otherEntityType in GeneratorModel.EntityType.Model
                    .EntityTypes.Where(et => et != GeneratorModel.EntityType))
                {
                    // find navigation properties for foreign keys from another EntityType which reference this EntityType
                    foreach (var foreignKey in otherEntityType
                        .GetForeignKeys().Where(fk => fk.PrincipalEntityType == GeneratorModel.EntityType))
                    {
                        var navigationPropertyName =
                            (string)foreignKey[RelationalMetadataModelProvider.AnnotationNamePrincipalEndNavPropName];
                        if (((EntityType)otherEntityType)
                            .FindAnnotation(RelationalMetadataModelProvider.AnnotationNameEntityTypeError) == null)
                        {
                            if (!foreignKey.IsUnique)
                            {
                                navPropInitializers.Add(
                                    new NavigationPropertyInitializerConfiguration(
                                        navigationPropertyName, otherEntityType.Name));
                            }
                        }
                    }
                }

                return navPropInitializers;
            }
        }

        public virtual IEnumerable<NavigationPropertyConfiguration> NavigationProperties
        {
            get
            {
                var navProps = new List<NavigationPropertyConfiguration>();

                foreach (var otherEntityType in GeneratorModel.EntityType.Model
                    .EntityTypes.Where(et => et != GeneratorModel.EntityType))
                {
                    // set up the navigation properties for foreign keys from
                    // another EntityType which reference this EntityType
                    foreach (var foreignKey in otherEntityType
                        .GetForeignKeys().Where(fk => fk.PrincipalEntityType == GeneratorModel.EntityType))
                    {
                        if (((EntityType)otherEntityType)
                            .FindAnnotation(RelationalMetadataModelProvider.AnnotationNameEntityTypeError) != null)
                        {
                            navProps.Add(new NavigationPropertyConfiguration(
                                Strings.UnableToAddNavigationProperty(otherEntityType.Name)));
                        }
                        else
                        {
                            var referencedType = foreignKey.IsUnique
                                ? otherEntityType.Name
                                : "ICollection<" + otherEntityType.Name + ">";
                            navProps.Add(new NavigationPropertyConfiguration(
                                referencedType,
                                (string)foreignKey[RelationalMetadataModelProvider.AnnotationNamePrincipalEndNavPropName]));
                        }
                    }
                }

                foreach (var foreignKey in GeneratorModel.EntityType.GetForeignKeys())
                {
                    // set up the navigation property on this end of foreign keys owned by this EntityType
                    navProps.Add(new NavigationPropertyConfiguration(
                        foreignKey.PrincipalEntityType.Name,
                        (string)foreignKey[RelationalMetadataModelProvider.AnnotationNameDependentEndNavPropName]));

                    // set up the other navigation property for self-referencing foreign keys owned by this EntityType
                    if (((ForeignKey)foreignKey).IsSelfReferencing())
                    {
                        var referencedType = foreignKey.IsUnique
                            ? foreignKey.DeclaringEntityType.Name
                            : "ICollection<" + foreignKey.DeclaringEntityType.Name + ">";
                        navProps.Add(new NavigationPropertyConfiguration(
                            referencedType,
                            (string)foreignKey[RelationalMetadataModelProvider.AnnotationNamePrincipalEndNavPropName]));
                    }
                }

                return navProps;
            }
        }
    }
}
