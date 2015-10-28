// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Represents the values assigned to a key of an entity. This type is mostly used to test the equivalence of entities
    ///         based on their key values.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IKeyValue
    {
        /// <summary>
        ///     They key that these values belong to (may be the primary key or an alternate key).
        /// </summary>
        IKey Key { get; }

        /// <summary>
        ///     The values assigned to the properties that make up the key. If they key has a single value, the actual value is returned.
        ///     If it is a composite key, then an array containing the property values is returned.
        /// </summary>
        object Value { get; }

        bool IsInvalid { get; }
    }
}
