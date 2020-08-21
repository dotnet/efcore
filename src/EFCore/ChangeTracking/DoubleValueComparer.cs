// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     Defines value comparison for <see cref="double" /> which correctly takes <see cref="double.NaN" />
    ///     into account.
    /// </summary>
    public class DoubleValueComparer : ValueComparer<double>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DoubleValueComparer" /> class.
        /// </summary>
        public DoubleValueComparer() : base(
            (x, y) => double.IsNaN(x) ? double.IsNaN(y) : x.Equals(y),
            d => d.GetHashCode())
        {
        }
    }
}
