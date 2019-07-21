// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Maps a static CLR method to a database function so that the CLR method may be used in LINQ queries.
    ///     By convention uses the .NET method name as name of the database function and the default schema.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
#pragma warning disable CA1813 // Avoid unsealed attributes
    // Already shipped unsealed
    public class DbFunctionAttribute : Attribute
#pragma warning restore CA1813 // Avoid unsealed attributes
    {
        private string _name;
        private string _schema;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbFunctionAttribute" /> class.
        /// </summary>
        public DbFunctionAttribute()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbFunctionAttribute" /> class.
        /// </summary>
        /// <param name="name">The name of the function in the database.</param>
        /// <param name="schema">The schema of the function in the database.</param>
        public DbFunctionAttribute([NotNull] string name, [CanBeNull] string schema = null)
        {
            Check.NotEmpty(name, nameof(name));

            _name = name;
            _schema = schema;
        }

        /// <summary>
        ///     The name of the function in the database.
        /// </summary>
        public virtual string Name
        {
            get => _name;
            [param: NotNull]
            set
            {
                Check.NotEmpty(value, nameof(value));

                _name = value;
            }
        }

        /// <summary>
        ///     The schema of the function in the database.
        /// </summary>
        public virtual string Schema
        {
            get => _schema;
            [param: CanBeNull] set => _schema = value;
        }
    }
}
