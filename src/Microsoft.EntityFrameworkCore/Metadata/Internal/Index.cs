// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Index : ConventionalAnnotatable, IMutableIndex
    {
        private bool? _isUnique;

        private ConfigurationSource _configurationSource;
        private ConfigurationSource? _isUniqueConfigurationSource;

        public Index(
            [NotNull] IReadOnlyList<Property> properties,
            [NotNull] EntityType declaringEntityType,
            ConfigurationSource configurationSource)
        {
            Check.NotEmpty(properties, nameof(properties));
            Check.HasNoNulls(properties, nameof(properties));
            Check.NotNull(declaringEntityType, nameof(declaringEntityType));

            Properties = properties;
            DeclaringEntityType = declaringEntityType;
            _configurationSource = configurationSource;

            Builder = new InternalIndexBuilder(this, declaringEntityType.Model.Builder);
        }

        public virtual IReadOnlyList<Property> Properties { get; }
        public virtual EntityType DeclaringEntityType { get; }
        public virtual InternalIndexBuilder Builder { get; [param: CanBeNull] set; }

        public virtual ConfigurationSource GetConfigurationSource() => _configurationSource;

        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = _configurationSource.Max(configurationSource);

        public virtual bool IsUnique
        {
            get { return _isUnique ?? DefaultIsUnique; }
            set { SetIsUnique(value, ConfigurationSource.Explicit); }
        }

        public virtual void SetIsUnique(bool unique, ConfigurationSource configurationSource)
        {
            _isUnique = unique;
            UpdateIsUniqueConfigurationSource(configurationSource);
        }

        private bool DefaultIsUnique => false;
        public virtual ConfigurationSource? GetIsUniqueConfigurationSource() => _isUniqueConfigurationSource;

        private void UpdateIsUniqueConfigurationSource(ConfigurationSource configurationSource)
            => _isUniqueConfigurationSource = configurationSource.Max(_isUniqueConfigurationSource);

        IReadOnlyList<IProperty> IIndex.Properties => Properties;
        IReadOnlyList<IMutableProperty> IMutableIndex.Properties => Properties;
        IEntityType IIndex.DeclaringEntityType => DeclaringEntityType;
        IMutableEntityType IMutableIndex.DeclaringEntityType => DeclaringEntityType;

        [UsedImplicitly]
        private string DebuggerDisplay => Property.Format(Properties);

        public virtual bool IsInUse() => DeclaringEntityType.FindForeignKeysInHierarchy(Properties).Any();
    }
}
