// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    /// <summary>
    ///     Generates <see cref="string" /> values using a string representation of <see cref="Guid.NewGuid()" />.
    ///     The generated values are non-temporary, meaning they will be saved to the database.
    /// </summary>
    public class StringValueGenerator : ValueGenerator<string>
    {
        /// <inheritdoc />
        public override bool GeneratesTemporaryValues
            => false;

        /// <inheritdoc />
        public override string Next(EntityEntry entry)
            => Guid.NewGuid().ToString();
    }
}
