// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class Index : ConventionalAnnotatable, IMutableIndex
    {
        private bool? _isUnique;

        private ConfigurationSource _configurationSource;
        private ConfigurationSource? _isUniqueConfigurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
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

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<Property> Properties { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType DeclaringEntityType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual InternalIndexBuilder Builder { get; [param: CanBeNull] set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource GetConfigurationSource() => _configurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdateConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = _configurationSource.Max(configurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsUnique
        {
            get { return _isUnique ?? DefaultIsUnique; }
            set { SetIsUnique(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsUnique(bool unique, ConfigurationSource configurationSource)
        {
            var isChanging = IsUnique != unique;
            _isUnique = unique;
            UpdateIsUniqueConfigurationSource(configurationSource);

            if (isChanging)
            {
                DeclaringEntityType.Model.ConventionDispatcher.OnIndexUniquenessChanged(Builder);
            }
        }

        private bool DefaultIsUnique => false;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetIsUniqueConfigurationSource() => _isUniqueConfigurationSource;

        private void UpdateIsUniqueConfigurationSource(ConfigurationSource configurationSource)
            => _isUniqueConfigurationSource = configurationSource.Max(_isUniqueConfigurationSource);

        IReadOnlyList<IProperty> IIndex.Properties => Properties;
        IReadOnlyList<IMutableProperty> IMutableIndex.Properties => Properties;
        IEntityType IIndex.DeclaringEntityType => DeclaringEntityType;
        IMutableEntityType IMutableIndex.DeclaringEntityType => DeclaringEntityType;

        [UsedImplicitly]
        private string DebuggerDisplay => Property.Format(Properties);
    }
}
