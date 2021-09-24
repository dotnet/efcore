// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    /// <summary>
    ///     Generates an array bytes from <see cref="Guid.NewGuid()" />.
    ///     The generated values are non-temporary, meaning they will be saved to the database.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information.
    /// </remarks>
    public class BinaryValueGenerator : ValueGenerator<byte[]>
    {
        /// <summary>
        ///     Gets a value indicating whether the values generated are temporary or permanent. This implementation
        ///     always returns false, meaning the generated values will be saved to the database.
        /// </summary>
        public override bool GeneratesTemporaryValues
            => false;

        /// <summary>
        ///     Gets a value to be assigned to a property.
        /// </summary>
        /// <returns>The value to be assigned to a property.</returns>
        public override byte[] Next(EntityEntry entry)
            => Guid.NewGuid().ToByteArray();
    }
}
