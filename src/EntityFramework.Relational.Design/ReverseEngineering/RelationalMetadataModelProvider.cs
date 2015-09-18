// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public abstract class RelationalMetadataModelProvider : IDatabaseMetadataModelProvider
    {
        public static IReadOnlyList<string> IgnoredAnnotations { get; } = new List<string>
        {
            CoreAnnotationNames.OriginalValueIndexAnnotation,
            CoreAnnotationNames.ShadowIndexAnnotation
        };

        public const string AnnotationPrefix = nameof(RelationalMetadataModelProvider) + ":";
        public const string AnnotationNameDependentEndNavPropName = AnnotationPrefix + "DependentEndNavPropName";
        public const string AnnotationNamePrincipalEndNavPropName = AnnotationPrefix + "PrincipalEndNavPropName";
        public const string AnnotationNameEntityTypeError = AnnotationPrefix + "EntityTypeError";

        public const string NavigationNameUniquifyingPattern = "{0}Navigation";
        public const string SelfReferencingPrincipalEndNavigationNamePattern = "Inverse{0}";

        private readonly Dictionary<EntityType, EntityType> _relationalToCodeGenEntityTypeMap =
            new Dictionary<EntityType, EntityType>();
        private readonly Dictionary<Property, Property> _relationalToCodeGenPropertyMap =
            new Dictionary<Property, Property>();

        public virtual ILogger Logger { get; }
        public virtual CSharpUtilities CSharpUtilities { get; }
        public virtual ModelUtilities ModelUtilities { get; }

        protected abstract IRelationalMetadataExtensionProvider ExtensionsProvider { get; }

        protected RelationalMetadataModelProvider([NotNull] ILogger logger,
            [NotNull] ModelUtilities modelUtilities, [NotNull] CSharpUtilities cSharpUtilities)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(modelUtilities, nameof(modelUtilities));

            Logger = logger;
            CSharpUtilities = cSharpUtilities;
            ModelUtilities = modelUtilities;
        }

        public virtual IModel GenerateMetadataModel([NotNull] string connectionString)
        {
            Check.NotEmpty(connectionString, nameof(connectionString));

            var relationalModel = ConstructRelationalModel(connectionString);

            var nameMapper = GetNameMapper(relationalModel);

            return ConstructCodeGenModel(relationalModel, nameMapper);
        }

        public virtual MetadataModelNameMapper GetNameMapper([NotNull] IModel relationalModel)
        {
            Check.NotNull(relationalModel, nameof(relationalModel));

            return new MetadataModelNameMapper(
                relationalModel,
                entity => ExtensionsProvider.For(entity).TableName,
                property => ExtensionsProvider.For(property).ColumnName);
        }

        /// <summary>
        /// Constructs an <see cref="IModel" /> directly representing the database consisting
        /// of <see cref="EntityType" />, <see cref="Property" />, <see cref="Key" /> and
        /// <see cref="ForeignKey" /> objects.
        /// This class expects that the EntityType will have the names of the underlying schema
        /// and table name as annotations on that EntityType. Similarly for the ColumnName annotation
        /// on each Property. The model does not contain <see cref="Navigation" /> objects as
        /// adding Navigations requires that the underlying EntityType have an underlying
        /// CLR type which is not possible here. Instead they will be constructed from the ForeignKeys.
        /// Errors generating EntityTypes can be attached to those EntityTypes using an
        /// annotation of name <see cref="RelationalMetadataModelProvider.AnnotationNameEntityTypeError"/>.
        /// Such EntityTypes will have files generated for them but the file will only contain
        /// the error message as a comment.
        /// </summary>
        public abstract IModel ConstructRelationalModel([NotNull] string connectionString);

        //TODO: investigate doing this as a builder pattern
        public virtual IModel ConstructCodeGenModel(
            [NotNull] IModel relationalModel, [NotNull] MetadataModelNameMapper nameMapper)
        {
            Check.NotNull(relationalModel, nameof(relationalModel));
            Check.NotNull(nameMapper, nameof(nameMapper));

            var codeGenModel = new Model();
            foreach (var relationalEntityType in relationalModel.EntityTypes.Cast<EntityType>())
            {
                var codeGenEntityType = codeGenModel
                    .AddEntityType(nameMapper.EntityTypeToClassNameMap[relationalEntityType]);
                _relationalToCodeGenEntityTypeMap[relationalEntityType] = codeGenEntityType;
                codeGenEntityType.Relational().TableName = ExtensionsProvider.For(relationalEntityType).TableName;
                codeGenEntityType.Relational().Schema = ExtensionsProvider.For(relationalEntityType).Schema;
                var errorMessage = relationalEntityType[AnnotationNameEntityTypeError];
                if (errorMessage != null)
                {
                    codeGenEntityType.AddAnnotation(AnnotationNameEntityTypeError, 
                        Strings.UnableToGenerateEntityType(codeGenEntityType.Name, errorMessage));
                }

                foreach (var relationalProperty in relationalEntityType.Properties)
                {
                    var codeGenProperty = codeGenEntityType.AddProperty(
                        nameMapper.PropertyToPropertyNameMap[relationalProperty],
                        ((IProperty)relationalProperty).ClrType);
                    _relationalToCodeGenPropertyMap[relationalProperty] = codeGenProperty;
                    CopyPropertyFacets(relationalProperty, codeGenProperty);
                }


                var primaryKey = relationalEntityType.FindPrimaryKey();
                if (primaryKey != null)
                {
                    codeGenEntityType.SetPrimaryKey(
                        primaryKey.Properties
                            .Select(p => _relationalToCodeGenPropertyMap[p])
                            .ToList());
                }
            } // end of loop over all relational EntityTypes

            AddForeignKeysToCodeGenModel(relationalModel, codeGenModel);

            return codeGenModel;
        }

        public virtual void AddForeignKeysToCodeGenModel([NotNull] IModel relationalModel, [NotNull] IModel codeGenModel)
        {
            Check.NotNull(codeGenModel, nameof(codeGenModel));

            foreach (var relationalEntityType in relationalModel.EntityTypes.Cast<EntityType>())
            {
                var codeGenEntityType = _relationalToCodeGenEntityTypeMap[relationalEntityType];
                foreach (var relationalForeignKey in relationalEntityType.GetForeignKeys())
                {
                    var foreignKeyCodeGenProperties =
                        relationalForeignKey
                        .Properties
                        .Select(relationalProperty =>
                        {
                            Property codeGenProperty;
                            return _relationalToCodeGenPropertyMap
                                    .TryGetValue(relationalProperty, out codeGenProperty)
                                    ? codeGenProperty
                                    : null;
                        })
                        .ToList();
                    var targetRelationalEntityType = relationalForeignKey.PrincipalEntityType;
                    var targetCodeGenEntityType = _relationalToCodeGenEntityTypeMap[targetRelationalEntityType];
                    var targetPrimaryKey = targetCodeGenEntityType.GetPrimaryKey();

                    var codeGenForeignKey = codeGenEntityType
                        .GetOrAddForeignKey(foreignKeyCodeGenProperties, targetPrimaryKey, targetCodeGenEntityType);
                    codeGenForeignKey.IsUnique = relationalForeignKey.IsUnique;
                }
            }

            AddDependentAndPrincipalNavigationPropertyAnnotations(codeGenModel);
        }

        private void AddDependentAndPrincipalNavigationPropertyAnnotations([NotNull] IModel codeGenModel)
        {
            Check.NotNull(codeGenModel, nameof(codeGenModel));

            var entityTypeToExistingIdentifiers = new Dictionary<IEntityType, List<string>>();
            foreach (var entityType in codeGenModel.EntityTypes)
            {
                var existingIdentifiers = new List<string>();
                entityTypeToExistingIdentifiers.Add(entityType, existingIdentifiers);
                existingIdentifiers.Add(entityType.Name);
                existingIdentifiers.AddRange(
                    ModelUtilities.OrderedProperties(entityType).Select(p => p.Name));
            }

            foreach (var entityType in codeGenModel.EntityTypes)
            {
                var dependentEndExistingIdentifiers = entityTypeToExistingIdentifiers[entityType];
                foreach (var foreignKey in entityType.GetForeignKeys().Cast<ForeignKey>())
                {
                    // set up the name of the navigation property on the dependent end of the foreign key
                    var dependentEndNavigationPropertyCandidateName =
                        ModelUtilities.GetDependentEndCandidateNavigationPropertyName(foreignKey);
                    var dependentEndNavigationPropertyName =
                        CSharpUtilities.GenerateCSharpIdentifier(
                            dependentEndNavigationPropertyCandidateName,
                            dependentEndExistingIdentifiers,
                            NavigationUniquifier);
                    foreignKey.AddAnnotation(
                        AnnotationNameDependentEndNavPropName,
                        dependentEndNavigationPropertyName);
                    dependentEndExistingIdentifiers.Add(dependentEndNavigationPropertyName);

                    // set up the name of the navigation property on the principal end of the foreign key
                    var principalEndExistingIdentifiers =
                        entityTypeToExistingIdentifiers[foreignKey.PrincipalEntityType];
                    var principalEndNavigationPropertyCandidateName =
                        foreignKey.IsSelfReferencing()
                            ? string.Format(
                                CultureInfo.CurrentCulture,
                                SelfReferencingPrincipalEndNavigationNamePattern,
                                dependentEndNavigationPropertyName)
                            : ModelUtilities.GetPrincipalEndCandidateNavigationPropertyName(foreignKey);
                    var principalEndNavigationPropertyName =
                        CSharpUtilities.GenerateCSharpIdentifier(
                            principalEndNavigationPropertyCandidateName,
                            principalEndExistingIdentifiers,
                            NavigationUniquifier);
                    foreignKey.AddAnnotation(
                        AnnotationNamePrincipalEndNavPropName,
                        principalEndNavigationPropertyName);
                    principalEndExistingIdentifiers.Add(principalEndNavigationPropertyName);
                }
            }
        }

        public static string ConstructIdForCombinationOfColumns([NotNull] IEnumerable<string> listOfColumnIds)
        {
            Check.NotNull(listOfColumnIds, nameof(listOfColumnIds));

            return string.Join(string.Empty, listOfColumnIds.OrderBy(columnId => columnId));
        }

        public virtual string NavigationUniquifier(
            [NotNull] string proposedIdentifier, [CanBeNull] ICollection<string> existingIdentifiers)
        {
            if (existingIdentifiers == null
                || !existingIdentifiers.Contains(proposedIdentifier))
            {
                return proposedIdentifier;
            }

            var finalIdentifier =
                string.Format(CultureInfo.CurrentCulture, NavigationNameUniquifyingPattern, proposedIdentifier);
            var suffix = 1;
            while (existingIdentifiers.Contains(finalIdentifier))
            {
                finalIdentifier = proposedIdentifier + suffix;
                suffix++;
            }

            return finalIdentifier;
        }

        public virtual void CopyPropertyFacets(
            [NotNull] Property relationalProperty, [NotNull] Property codeGenProperty)
        {
            Check.NotNull(relationalProperty, nameof(relationalProperty));
            Check.NotNull(codeGenProperty, nameof(codeGenProperty));

            foreach(var annotation in 
                relationalProperty.Annotations.Where(a => !IgnoredAnnotations.Contains(a.Name)))
            {
                codeGenProperty.AddAnnotation(annotation.Name, annotation.Value);
            }

            codeGenProperty.IsNullable = relationalProperty.IsNullable;
            codeGenProperty.ValueGenerated = relationalProperty.ValueGenerated;
        }
    }
}
