// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Marks a class, property or field with a comment to be set on the corresponding database table or column.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class CommentAttribute : Attribute
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="CommentAttribute" /> class.
        /// </summary>
        /// <param name="comment"> The comment. </param>
        public CommentAttribute(string comment)
        {
            Check.NotEmpty(comment, nameof(comment));

            Comment = comment;
        }

        /// <summary>
        ///     The comment to be configured.
        /// </summary>
        public string Comment { get; }
    }
}
