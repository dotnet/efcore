// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    public class InternalIndexBuilder : InternalMetadataItemBuilder<Index>
    {
        private ConfigurationSource? _isUniqueConfigurationSource;

        public InternalIndexBuilder([NotNull] Index index, [NotNull] InternalModelBuilder modelBuilder)
            : base(index, modelBuilder)
        {
        }

        public virtual bool IsUnique(bool unique, ConfigurationSource configurationSource)
            => IsUnique(unique, configurationSource, runConventions: true);

        public virtual bool IsUnique(bool unique, ConfigurationSource configurationSource, bool runConventions)
        {
            if (!CanSetUnique(unique, configurationSource))
            {
                return false;
            }

            var foreignKeys = Metadata.DeclaringEntityType.FindForeignKeys(Metadata.Properties);
            foreach (var foreignKey in foreignKeys)
            {
                var relationshipBuilder = foreignKey.Builder;
                if (foreignKey.PrincipalToDependent != null
                    && !Navigation.IsCompatible(
                        foreignKey.PrincipalToDependent.Name,
                        foreignKey.PrincipalEntityType,
                        foreignKey.DeclaringEntityType,
                        shouldBeCollection: !unique,
                        shouldThrow: false))
                {
                    relationshipBuilder.Navigation(null, /* pointsToPrincipal: */ false, configurationSource, runConventions);
                }
            }

            var newIndexBuilder = Metadata.DeclaringEntityType.Builder.HasIndex(Metadata.Properties.Select(p => p.Name).ToList(), ConfigurationSource.Convention);

            if (newIndexBuilder.Metadata != Metadata)
            {
                return newIndexBuilder.IsUnique(unique, configurationSource);
            }

            if (_isUniqueConfigurationSource == null
                    && Metadata.IsUnique != null)
            {
                _isUniqueConfigurationSource = ConfigurationSource.Explicit;
            }
            else
            {
                _isUniqueConfigurationSource = configurationSource.Max(_isUniqueConfigurationSource);
            }
            Metadata.IsUnique = unique;
            return true;
        }

        public virtual bool CanSetUnique(bool unique, ConfigurationSource configurationSource)
        {
            if (((IIndex)Metadata).IsUnique == unique)
            {
                return true;
            }

            if (!configurationSource.CanSet(_isUniqueConfigurationSource, Metadata.IsUnique.HasValue))
            {
                return false;
            }

            var foreignKeys = Metadata.DeclaringEntityType.FindForeignKeys(Metadata.Properties);
            foreach (var foreignKey in foreignKeys)
            {
                var relationshipBuilder = foreignKey.Builder;
                if (foreignKey.PrincipalToDependent != null
                    && !relationshipBuilder.CanSetNavigation(null, /* pointsToPrincipal */ false, configurationSource)
                    && !Navigation.IsCompatible(
                        foreignKey.PrincipalToDependent.Name,
                        foreignKey.PrincipalEntityType,
                        foreignKey.DeclaringEntityType,
                        shouldBeCollection: !unique,
                        shouldThrow: false))
                {
                    return false;
                }
            }

            return true;
        }

        public virtual InternalIndexBuilder Attach(ConfigurationSource configurationSource)
        {
            var entityTypeBuilder = ModelBuilder.Entity(Metadata.DeclaringEntityType.Name, ConfigurationSource.Convention);
            var newIndexBuilder = entityTypeBuilder.HasIndex(Metadata.Properties.Select(p => p.Name).ToList(), configurationSource);

            newIndexBuilder.MergeAnnotationsFrom(this);

            if (_isUniqueConfigurationSource.HasValue && Metadata.IsUnique.HasValue)
            {
                newIndexBuilder.IsUnique(Metadata.IsUnique.Value, _isUniqueConfigurationSource.Value);
            }

            return newIndexBuilder;
        }
    }
}
