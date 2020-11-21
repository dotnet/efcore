// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Configures the precision of data that is allowed in this property.
    ///     For example, if the property is a <see cref="decimal" />
    ///     then this is the maximum number of digits.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class PrecisionAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PrecisionAttribute" /> class.
        /// </summary>
        /// <param name="precision"> The precision of the property. </param>
        /// <param name="scale"> The scale of the property. </param>
        public PrecisionAttribute(int precision, int scale)
        {
            Precision = precision;
            Scale = scale;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="PrecisionAttribute" /> class.
        /// </summary>
        /// <param name="precision"> The precision of the property. </param>
        public PrecisionAttribute(int precision)
        {
            Precision = precision;
        }

        /// <summary>
        ///     The precision of the property.
        /// </summary>
        public int Precision { get; }

        /// <summary>
        ///     The scale of the property.
        /// </summary>
        public int? Scale { get; }
    }
}
