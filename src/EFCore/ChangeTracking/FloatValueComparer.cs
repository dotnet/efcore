// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.ChangeTracking
{
    /// <summary>
    ///     Defines value comparison for <see cref="float" /> which correctly takes <see cref="float.NaN" />
    ///     into account.
    /// </summary>
    public class FloatValueComparer : ValueComparer<float>
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="FloatValueComparer" /> class.
        /// </summary>
        public FloatValueComparer() : base(
            (x, y) => float.IsNaN(x) ? float.IsNaN(y) : x.Equals(y),
            d => d.GetHashCode())
        {
        }
    }
}
