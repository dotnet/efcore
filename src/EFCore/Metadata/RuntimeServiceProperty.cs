// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a property on an entity type that represents an
    ///     injected service from the <see cref="DbContext" />.
    /// </summary>
    public class RuntimeServiceProperty : RuntimePropertyBase, IServiceProperty
    {
        private ServiceParameterBinding? _parameterBinding;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public RuntimeServiceProperty(
            string name,
            PropertyInfo? propertyInfo,
            FieldInfo? fieldInfo,
            RuntimeEntityType declaringEntityType,
            PropertyAccessMode propertyAccessMode)
            : base(name, propertyInfo, fieldInfo, propertyAccessMode)
        {
            Check.NotNull(declaringEntityType, nameof(declaringEntityType));

            DeclaringEntityType = declaringEntityType;
            ClrType = (propertyInfo?.PropertyType ?? fieldInfo?.FieldType)!;
        }

        /// <summary>
        ///     Gets the type that this property-like object belongs to.
        /// </summary>
        public override RuntimeEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     Gets the type of value that this property-like object holds.
        /// </summary>
        protected override Type ClrType { get; }

        /// <summary>
        ///     The <see cref="ServiceParameterBinding" /> for this property.
        /// </summary>
        public virtual ServiceParameterBinding ParameterBinding
        {
            get => NonCapturingLazyInitializer.EnsureInitialized(ref _parameterBinding, (IServiceProperty)this, static property =>
                {
                    var entityType = property.DeclaringEntityType;
                    var factory = entityType.Model.GetModelDependencies().ParameterBindingFactories.FindFactory(property.ClrType, property.Name)!;
                    return (ServiceParameterBinding)factory.Bind(entityType, property.ClrType, property.Name);
                });

            [DebuggerStepThrough]
            set => _parameterBinding = value;
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        public override string ToString()
            => ((IServiceProperty)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public virtual DebugView DebugView
            => new(
                () => ((IServiceProperty)this).ToDebugString(MetadataDebugStringOptions.ShortDefault),
                () => ((IServiceProperty)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

        /// <inheritdoc />
        IReadOnlyEntityType IReadOnlyServiceProperty.DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => DeclaringEntityType;
        }

        /// <inheritdoc />
        IEntityType IServiceProperty.DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => DeclaringEntityType;
        }
    }
}
