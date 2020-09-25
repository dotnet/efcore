// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    /// <summary>
    ///     Generates an array bytes from <see cref="Guid.NewGuid()" />.
    ///     The generated values are non-temporary, meaning they will be saved to the database.
    /// </summary>
    public class BinaryValueGenerator : ValueGenerator<byte[]>
    {
        /// <inheritdoc />
        public override bool GeneratesTemporaryValues
            => false;

        /// <inheritdoc />
        public override byte[] Next(EntityEntry entry)
            => Guid.NewGuid().ToByteArray();
    }
}
