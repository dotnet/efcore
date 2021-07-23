// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a navigation property which can be used to navigate a relationship.
    /// </summary>
    public class RuntimeNavigation : RuntimePropertyBase, INavigation
    {
        // Warning: Never access these fields directly as access needs to be thread-safe
        private IClrCollectionAccessor? _collectionAccessor;
        private bool _collectionAccessorInitialized;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public RuntimeNavigation(
            string name,
            Type clrType,
            PropertyInfo? propertyInfo,
            FieldInfo? fieldInfo,
            RuntimeForeignKey foreignKey,
            PropertyAccessMode propertyAccessMode,
            bool eagerLoaded)
            : base(name, propertyInfo, fieldInfo, propertyAccessMode)
        {
            ClrType = clrType;
            ForeignKey = foreignKey;
            if (eagerLoaded)
            {
                SetAnnotation(CoreAnnotationNames.EagerLoaded, true);
            }
        }

        /// <summary>
        ///     Gets the type of value that this navigation holds.
        /// </summary>
        protected override Type ClrType { get; }

        /// <summary>
        ///     Gets the foreign key that defines the relationship this navigation property will navigate.
        /// </summary>
        public virtual RuntimeForeignKey ForeignKey { get; }

        /// <summary>
        ///     Gets the entity type that this navigation property belongs to.
        /// </summary>
        public override RuntimeEntityType DeclaringEntityType
        {
            [DebuggerStepThrough]
            get => ((IReadOnlyNavigation)this).IsOnDependent ? ForeignKey.DeclaringEntityType : ForeignKey.PrincipalEntityType;
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        public override string ToString()
            => ((IReadOnlyNavigation)this).ToDebugString(MetadataDebugStringOptions.SingleLineDefault);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public virtual DebugView DebugView
            => new(
                () => ((IReadOnlyNavigation)this).ToDebugString(MetadataDebugStringOptions.ShortDefault),
                () => ((IReadOnlyNavigation)this).ToDebugString(MetadataDebugStringOptions.LongDefault));

        /// <inheritdoc/>
        IReadOnlyForeignKey IReadOnlyNavigation.ForeignKey
        {
            [DebuggerStepThrough]
            get => ForeignKey;
        }

        /// <inheritdoc/>
        [DebuggerStepThrough]
        IClrCollectionAccessor? INavigationBase.GetCollectionAccessor()
            => NonCapturingLazyInitializer.EnsureInitialized(
                ref _collectionAccessor,
                ref _collectionAccessorInitialized,
                this,
                static navigation =>
                {
                    navigation.EnsureReadOnly();
                    return new ClrCollectionAccessorFactory().Create(navigation);
                });
    }
}
