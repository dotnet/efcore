// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Specifies an index to be generated in the database.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IndexAttribute" /> class.
        /// </summary>
        /// <param name="memberNames"> The members which constitute the index, in order (there must be at least one). </param>
        public IndexAttribute(params string[] memberNames)
        {
            Check.NotEmpty(memberNames, nameof(memberNames));
            MemberNames = memberNames;
        }

        /// <summary>
        ///     The members which constitute the index, in order.
        /// </summary>
        public virtual string[] MemberNames { get; }

        /// <summary>
        ///     The name of the index in the database.
        /// </summary>
        public virtual string Name { get; [param: NotNull] set; }


        /// <summary>
        ///     Whether the index is unique.
        /// </summary>
        public virtual bool IsUnique { get; set; }
    }
}
