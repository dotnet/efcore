// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Marks a type or property with comment.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class CommentAttribute : Attribute
    {
        private readonly string _comment;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommentAttribute" /> class.
        /// </summary>
        public CommentAttribute()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CommentAttribute" /> class.
        /// </summary>
        /// <param name="comment">The comment.</param>
        public CommentAttribute([CanBeNull] string comment)
        {
            _comment = comment;
        }


        /// <summary>
        ///     The Comment
        /// </summary>
        public string Comment => _comment;
    }
}
