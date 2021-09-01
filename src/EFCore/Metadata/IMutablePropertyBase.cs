// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Base type for navigation and scalar properties.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IReadOnlyPropertyBase" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see> for more information.
    /// </remarks>
    public interface IMutablePropertyBase : IReadOnlyPropertyBase, IMutableAnnotatable
    {
        /// <summary>
        ///     Gets the type that this property belongs to.
        /// </summary>
        new IMutableTypeBase DeclaringType { get; }

        /// <summary>
        ///     Gets or sets the underlying CLR field for this property.
        ///     This may be <see langword="null" /> for shadow properties or if the backing field for the property is not known.
        /// </summary>
        new FieldInfo? FieldInfo { get; set; }

        /// <summary>
        ///     <para>
        ///         Sets the underlying CLR field that this property should use.
        ///     </para>
        ///     <para>
        ///         Backing fields are normally found by convention as described
        ///         here: http://go.microsoft.com/fwlink/?LinkId=723277.
        ///         This method is useful for setting backing fields explicitly in cases where the
        ///         correct field is not found by convention.
        ///     </para>
        ///     <para>
        ///         By default, the backing field, if one is found or has been specified, is used when
        ///         new objects are constructed, typically when entities are queried from the database.
        ///         Properties are used for all other accesses. This can be changed by calling
        ///         <see cref="SetPropertyAccessMode" />.
        ///     </para>
        /// </summary>
        /// <param name="fieldName"> The name of the field to use. </param>
        void SetField(string? fieldName);

        /// <summary>
        ///     Sets the <see cref="PropertyAccessMode" /> to use for this property.
        /// </summary>
        /// <param name="propertyAccessMode">
        ///     The <see cref="PropertyAccessMode" />, or <see langword="null" />
        ///     to clear the mode set.
        /// </param>
        void SetPropertyAccessMode(PropertyAccessMode? propertyAccessMode);
    }
}
