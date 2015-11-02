// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Metadata.Internal
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Index : ConventionalAnnotatable, IMutableIndex
    {
        private ConfigurationSource _configurationSource;

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

        public virtual ConfigurationSource UpdateConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = _configurationSource.Max(configurationSource);

        public virtual bool? IsUnique { get; set; }
        protected virtual bool DefaultIsUnique => false;

        IReadOnlyList<IProperty> IIndex.Properties => Properties;
        IReadOnlyList<IMutableProperty> IMutableIndex.Properties => Properties;
        IEntityType IIndex.DeclaringEntityType => DeclaringEntityType;
        IMutableEntityType IMutableIndex.DeclaringEntityType => DeclaringEntityType;
        bool IIndex.IsUnique => IsUnique ?? DefaultIsUnique;

        [UsedImplicitly]
        private string DebuggerDisplay => Property.Format(Properties);
    }
}
