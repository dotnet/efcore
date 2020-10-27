// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Configures the property as capable of persisting unicode characters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public sealed class UnicodeAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="UnicodeAttribute" /> class.
        /// </summary>
        /// <param name="unicode"> A value indicating whether the property can contain unicode characters or not. </param>
        public UnicodeAttribute(bool unicode = true)
        {
            IsUnicode = unicode;
        }

        /// <summary>
        ///     A value indicating whether the property can contain unicode characters or not.
        /// </summary>
        public bool IsUnicode { get; }
    }
}
