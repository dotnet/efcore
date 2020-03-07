// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;

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
        ///     This may be <c>null</c> for shadow properties or if the backing field for the property is not known.
        /// </summary>
        new FieldInfo FieldInfo { get; [param: CanBeNull] set; }
    }
}
