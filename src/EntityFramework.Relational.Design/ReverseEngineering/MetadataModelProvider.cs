// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.Logging;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public abstract class MetadataModelProvider
    {
        internal const string AnnotationPrefix = nameof(MetadataModelProvider) + ":";
        internal const string AnnotationNameDependentEndNavPropName = AnnotationPrefix + "DependentEndNavPropName";
        internal const string AnnotationNamePrincipalEndNavPropName = AnnotationPrefix + "PrincipalEndNavPropName";
        internal const string AnnotationNameEntityTypeError = AnnotationPrefix + "EntityTypeError";
        internal const string NavigationNameUniquifyingPattern = "{0}Navigation";
        internal const string SelfReferencingPrincipalEndNavigationNamePattern = "Inverse{0}";

        protected MetadataModelProvider([NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            Logger = loggerFactory.CreateCommandsLogger();
        }

        public abstract IModel GetModel([NotNull] string connectionString, [CanBeNull] TableSelectionSet tableSelectionSet);

        protected virtual ILogger Logger { get; }

        protected virtual void AddNavigationProperties([NotNull] IModel model)
        {
            // TODO perf cleanup can we do this in 1 loop instead of 2?
            var modelUtilities = new ModelUtilities();
            Check.NotNull(model, nameof(model));

            var entityTypeToExistingIdentifiers = new Dictionary<IEntityType, List<string>>();
            foreach (var entityType in model.EntityTypes)
            {
                var existingIdentifiers = new List<string>();
                entityTypeToExistingIdentifiers.Add(entityType, existingIdentifiers);
                existingIdentifiers.Add(entityType.Name);
                existingIdentifiers.AddRange(
                    modelUtilities.OrderedProperties(entityType).Select(p => p.Name));
            }

            foreach (var entityType in model.EntityTypes)
            {
                var dependentEndExistingIdentifiers = entityTypeToExistingIdentifiers[entityType];
                foreach (var foreignKey in entityType.GetForeignKeys())
                {
                    // set up the name of the navigation property on the dependent end of the foreign key
                    var dependentEndNavigationPropertyCandidateName =
                        modelUtilities.GetDependentEndCandidateNavigationPropertyName(foreignKey);
                    var dependentEndNavigationPropertyName =
                        CSharpUtilities.Instance.GenerateCSharpIdentifier(
                            dependentEndNavigationPropertyCandidateName,
                            dependentEndExistingIdentifiers,
                            NavigationUniquifier);
                    (foreignKey as ForeignKey)?.AddAnnotation(
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
                            : modelUtilities.GetPrincipalEndCandidateNavigationPropertyName(foreignKey);
                    var principalEndNavigationPropertyName =
                        CSharpUtilities.Instance.GenerateCSharpIdentifier(
                            principalEndNavigationPropertyCandidateName,
                            principalEndExistingIdentifiers,
                            NavigationUniquifier);
                    (foreignKey as ForeignKey)?.AddAnnotation(
                        AnnotationNamePrincipalEndNavPropName,
                        principalEndNavigationPropertyName);
                    principalEndExistingIdentifiers.Add(principalEndNavigationPropertyName);
                }
            }
        }

        // TODO use CSharpUniqueNamer
        private string NavigationUniquifier([NotNull] string proposedIdentifier, [CanBeNull] ICollection<string> existingIdentifiers)
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
    }
}
