// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
    public class ComplexTypeUsage : AccessibleProperty, IMutableComplexTypeUsage
    {
        private ConfigurationSource _configurationSource;
        private ConfigurationSource? _isRequiredConfigurationSource;

        private bool? _isRequired;

        // Warning: Never access these fields directly as access needs to be thread-safe
        private PropertyIndexes _indexes;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexTypeUsage(
            [NotNull] PropertyInfo property,
            [NotNull] ComplexTypeReference definingReference,
            [NotNull] EntityType declaringEntityType,
            ConfigurationSource configurationSource)
            : base(Check.NotNull(property, nameof(property)).Name, property)
        {
            Check.NotNull(definingReference, nameof(definingReference));
            Check.NotNull(declaringEntityType, nameof(declaringEntityType));

            DefiningReference = definingReference;
            DeclaringEntityType = declaringEntityType;
            _configurationSource = configurationSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ComplexTypeUsage(
            [NotNull] string name,
            [NotNull] ComplexTypeReference definingReference,
            [NotNull] EntityType declaringEntityType,
            ConfigurationSource configurationSource)
            : base(name, null)
        {
            Check.NotNull(definingReference, nameof(definingReference));
            Check.NotNull(declaringEntityType, nameof(declaringEntityType));

            DefiningReference = definingReference;
            DeclaringEntityType = declaringEntityType;
            _configurationSource = configurationSource;
        }

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
        public override Type ClrType => DefiningReference.ClrType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeReference DefiningReference { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public new virtual StructuralType DeclaringType => DefiningReference.DeclaringType;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual EntityType DeclaringEntityType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual PropertyIndexes PropertyIndexes
        {
            get
            {
                return NonCapturingLazyInitializer.EnsureInitialized(ref _indexes, this,
                    property => property.DeclaringEntityType.CalculateIndexes(property));
            }

            [param: CanBeNull]
            set
            {
                if (value == null)
                {
                    // This path should only kick in when the model is still mutable and therefore access does not need
                    // to be thread-safe.
                    _indexes = null;
                }
                else
                {
                    NonCapturingLazyInitializer.EnsureInitialized(ref _indexes, value);
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsRequired
        {
            get { return _isRequired ?? DefaultIsRequired; }
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

        private bool DefaultIsRequired => DefiningReference.IsRequired;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConfigurationSource? GetIsRequiredConfigurationSource() => _isRequiredConfigurationSource;

        private void UpdateIsRequiredConfigurationSource(ConfigurationSource configurationSource)
            => _isRequiredConfigurationSource = configurationSource.Max(_isRequiredConfigurationSource);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<IMutableComplexPropertyUsage> GetProperties()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IMutableComplexPropertyUsage FindProperty(string name)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IEnumerable<ComplexTypeUsage> GetComplexTypeUsages()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ComplexTypeUsage FindComplexTypeUsage(string name)
        {
            throw new NotImplementedException();
        }

        IEntityType IPropertyBase.DeclaringEntityType => DeclaringEntityType;

        IStructuralType IAccessibleProperty.DeclaringType => DeclaringType;
        IMutableStructuralType IMutableAccessibleProperty.DeclaringType => DeclaringType;

        IMutableComplexTypeReference IMutableComplexTypeUsage.DefiningReference => DefiningReference;
        IComplexTypeReference IComplexTypeUsage.DefiningReference => DefiningReference;

        IComplexPropertyUsage IComplexTypeUsage.FindProperty(string name) => FindProperty(name);
        IEnumerable<IComplexPropertyUsage> IComplexTypeUsage.GetProperties() => GetProperties();

        IComplexTypeUsage IComplexTypeUsage.FindComplexTypeUsage(string name) => FindComplexTypeUsage(name);
        IEnumerable<IComplexTypeUsage> IComplexTypeUsage.GetComplexTypeUsages() => GetComplexTypeUsages();

        IMutableComplexTypeUsage IMutableComplexTypeUsage.FindComplexTypeUsage(string name) => FindComplexTypeUsage(name);
        IEnumerable<IMutableComplexTypeUsage> IMutableComplexTypeUsage.GetComplexTypeUsages() => GetComplexTypeUsages();

        // TODO:
        //public override string ToString() => this.ToDebugString();

        // TODO:
        //public virtual DebugView<Navigation> DebugView
        //    => new DebugView<Navigation>(this, m => m.ToDebugString(false));
    }
}
