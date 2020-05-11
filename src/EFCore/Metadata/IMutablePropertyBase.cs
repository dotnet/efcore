// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Base type for navigation and scalar properties.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IPropertyBase" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IMutablePropertyBase : IPropertyBase, IMutableAnnotatable
    {
        /// <summary>
        ///     Gets the type that this property belongs to.
        /// </summary>
        new IMutableTypeBase DeclaringType { get; }

        /// <summary>
        ///     Gets or sets the underlying CLR field for this property.
        ///     This may be <see langword="null" /> for shadow properties or if the backing field for the property is not known.
        /// </summary>
        new FieldInfo FieldInfo { get; [param: CanBeNull] set; }

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
        ///         <see cref="MutablePropertyBaseExtensions.SetPropertyAccessMode" />.
        ///     </para>
        /// </summary>
        /// <param name="fieldName"> The name of the field to use. </param>
        void SetField([CanBeNull] string fieldName)
            => this.AsPropertyBase().SetField(fieldName, ConfigurationSource.Explicit);
    }
}
