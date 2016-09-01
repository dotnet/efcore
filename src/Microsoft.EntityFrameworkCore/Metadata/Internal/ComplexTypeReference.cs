// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ComplexTypeReference : AccessibleProperty, IMutableComplexTypeReference
    {
        private ConfigurationSource _configurationSource;
        private ConfigurationSource? _isRequiredConfigurationSource;

        private bool? _isRequired;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexTypeReference(
            [NotNull] PropertyInfo property,
            [NotNull] StructuralType declaringType,
            [NotNull] ComplexType referencedComplexType,
            ConfigurationSource configurationSource)
            : base(Check.NotNull(property, nameof(property)).Name, property)
        {
            Check.NotNull(declaringType, nameof(declaringType));
            Check.NotNull(referencedComplexType, nameof(referencedComplexType));

            DeclaringType = declaringType;
            ReferencedComplexType = referencedComplexType;
            _configurationSource = configurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexTypeReference(
            [NotNull] string name,
            [NotNull] StructuralType declaringType,
            [NotNull] ComplexType referencedComplexType,
            ConfigurationSource configurationSource)
            : base(name, null)
        {
            Check.NotNull(declaringType, nameof(declaringType));
            Check.NotNull(referencedComplexType, nameof(referencedComplexType));

            DeclaringType = declaringType;
            ReferencedComplexType = referencedComplexType;
            _configurationSource = configurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexType ReferencedComplexType { get; }

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

        // Needed for a workaround before reference counting is implemented
        // Issue #214
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetConfigurationSource(ConfigurationSource configurationSource)
            => _configurationSource = configurationSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Type ClrType => ReferencedComplexType.ClrType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual StructuralType DeclaringType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsRequired
        {
            get{ return _isRequired ?? DefaultIsRequired; }
            set { SetIsRequired(value, ConfigurationSource.Explicit); }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SetIsRequired(bool required, ConfigurationSource configurationSource)
        {
            if (!required
                && !ClrType.IsNullableType())
            {
                // TODO: Message
                throw new InvalidOperationException(CoreStrings.CannotBeNullable(Name, DeclaringType.DisplayName(), ClrType.ShortDisplayName()));
            }

            UpdateIsRequiredConfigurationSource(configurationSource);

            _isRequired = required;
        }

        private bool DefaultIsRequired => true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetIsRequiredConfigurationSource() => _isRequiredConfigurationSource;

        private void UpdateIsRequiredConfigurationSource(ConfigurationSource configurationSource)
            => _isRequiredConfigurationSource = configurationSource.Max(_isRequiredConfigurationSource);

        IStructuralType IAccessibleProperty.DeclaringType => DeclaringType;
        IMutableStructuralType IMutableAccessibleProperty.DeclaringType => DeclaringType;
        IMutableStructuralType IMutableComplexTypeReference.DeclaringType => DeclaringType;

        IMutableComplexType IMutableComplexTypeReference.ReferencedComplexType => ReferencedComplexType;
        IComplexType IComplexTypeReference.ReferencedComplexType => ReferencedComplexType;

        // TODO:
        //public override string ToString() => this.ToDebugString();

        // TODO:
        //public virtual DebugView<Navigation> DebugView
        //    => new DebugView<Navigation>(this, m => m.ToDebugString(false));
    }
}
