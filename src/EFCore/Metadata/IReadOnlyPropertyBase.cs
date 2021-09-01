// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Base type for navigations and properties.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information.
    /// </remarks>
    public interface IReadOnlyPropertyBase : IReadOnlyAnnotatable
    {
        /// <summary>
        ///     Gets the name of this property-like object.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the type that this property-like object belongs to.
        /// </summary>
        IReadOnlyTypeBase DeclaringType { get; }

        /// <summary>
        ///     Gets the type of value that this property-like object holds.
        /// </summary>
        Type ClrType { get; }

        /// <summary>
        ///     Gets the <see cref="PropertyInfo" /> for the underlying CLR property for this property-like object.
        ///     This may be <see langword="null" /> for shadow properties or if mapped directly to a field.
        /// </summary>
        PropertyInfo? PropertyInfo { get; }

        /// <summary>
        ///     Gets the <see cref="FieldInfo" /> for the underlying CLR field for this property-like object.
        ///     This may be <see langword="null" /> for shadow properties or if the backing field is not known.
        /// </summary>
        FieldInfo? FieldInfo { get; }

        /// <summary>
        ///     Gets the name of the backing field for this property, or <see langword="null" /> if the backing field
        ///     is not known.
        /// </summary>
        /// <returns> The name of the backing field, or <see langword="null" />. </returns>
        string? GetFieldName()
            => FieldInfo?.GetSimpleMemberName();

        /// <summary>
        ///     Gets a value indicating whether this is a shadow property. A shadow property is one that does not have a
        ///     corresponding property in the entity class. The current value for the property is stored in
        ///     the <see cref="ChangeTracker" /> rather than being stored in instances of the entity class.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the property is a shadow property, otherwise <see langword="false" />.
        /// </returns>
        bool IsShadowProperty() => this.GetIdentifyingMemberInfo() == null;

        /// <summary>
        ///     Gets a value indicating whether this is an indexer property. An indexer property is one that is accessed through
        ///     an indexer on the entity class.
        /// </summary>
        /// <returns>
        ///     <see langword="true" /> if the property is an indexer property, otherwise <see langword="false" />.
        /// </returns>
        bool IsIndexerProperty()
            => this.GetIdentifyingMemberInfo() is PropertyInfo propertyInfo
                && propertyInfo == DeclaringType.FindIndexerPropertyInfo();

        /// <summary>
        ///     Gets the <see cref="PropertyAccessMode" /> being used for this property-like object.
        /// </summary>
        /// <returns> The access mode being used. </returns>
        PropertyAccessMode GetPropertyAccessMode();
    }
}
