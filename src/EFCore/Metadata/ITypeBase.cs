// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a type in an <see cref="IModel" />.
    /// </summary>
    public interface ITypeBase : IAnnotatable
    {
        /// <summary>
        ///     Gets the model that this type belongs to.
        /// </summary>
        IModel Model { get; }

        /// <summary>
        ///     Gets the name of this type.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     <para>
        ///         Gets the CLR class that is used to represent instances of this type.
        ///         Returns <see langword="null" /> if the type does not have a corresponding CLR class (known as a shadow type).
        ///     </para>
        ///     <para>
        ///         Shadow types are not currently supported in a model that is used at runtime with a <see cref="DbContext" />.
        ///         Therefore, shadow types will only exist in migration model snapshots, etc.
        ///     </para>
        /// </summary>
        Type ClrType { get; }

        /// <summary>
        ///     Gets whether this entity type can share its ClrType with other entities.
        /// </summary>
        bool HasSharedClrType { get; }

        /// <summary>
        ///     Gets whether this entity type has an indexer which is able to contain arbitrary properties.
        /// </summary>
        bool IsPropertyBag { get; }
    }
}
