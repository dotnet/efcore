// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
    public abstract class PropertyBase : ConventionalAnnotatable, IPropertyBase
    {
        // Warning: Never access these fields directly as access needs to be thread-safe
        private IClrPropertyGetter _getter;
        private IClrPropertySetter _setter;
        private PropertyAccessors _accessors;
        private PropertyIndexes _indexes;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected PropertyBase(
            [NotNull] string name,
            [CanBeNull] PropertyInfo propertyInfo)
        {
            Check.NotEmpty(name, nameof(name));

            Name = name;
            PropertyInfo = propertyInfo;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public abstract EntityType DeclaringEntityType { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual PropertyInfo PropertyInfo { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IClrPropertyGetter Getter
            => NonCapturingLazyInitializer.EnsureInitialized(ref _getter, PropertyInfo, p => new ClrPropertyGetterFactory().Create(p));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IClrPropertySetter Setter
            => NonCapturingLazyInitializer.EnsureInitialized(ref _setter, PropertyInfo, p => new ClrPropertySetterFactory().Create(p));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used 
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual PropertyAccessors Accessors
            => NonCapturingLazyInitializer.EnsureInitialized(ref _accessors, this, p => new PropertyAccessorsFactory().Create(p));

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

        IEntityType IPropertyBase.DeclaringEntityType => DeclaringEntityType;
    }
}
