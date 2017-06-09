// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Defines a user defined database function
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DbFunctionAttribute : Attribute
    {
        private string _name;

        /// <summary>
        /// Defines a user defined database function
        /// </summary>
        public DbFunctionAttribute()
        {
        }

        /// <summary>
        /// Defines a user defined database function
        /// </summary>
        /// <param name="name">The name of the function in the underlying datastore.</param>
        /// <param name="schema">The schema where the function lives in the underlying datastore.</param>
        public DbFunctionAttribute([NotNull] string name, [CanBeNull] string schema)
        {
            Check.NotEmpty(name, nameof(name));

            Schema = schema;
            Name = name;
        }

        /// <summary>
        ///     The name of the function in the underlying datastore.
        /// </summary>
        public string Name
        {
            get { return _name; }

            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(Name));
                _name = value;
            }
        }

        /// <summary>
        ///     The schema where the function lives in the underlying datastore.
        /// </summary>
        public string Schema { get; [param: CanBeNull] set; }
    }
}
