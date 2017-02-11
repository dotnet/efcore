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
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class DbFunctionAttribute : Attribute
    {
        /// <summary>
        /// Defines a user defined database function
        /// </summary>
        public DbFunctionAttribute()
        {
        }

        /// <summary>
        /// Defines a user defined database function
        /// </summary>
        /// <param name="schema">The schema where the function lives in the underlying datastore.</param>
        /// <param name="name">The name of the function in the underlying datastore.</param>
        public DbFunctionAttribute([CanBeNull] string schema, [NotNull] string name)
        {
            Check.NotEmpty(name, nameof(name));

            Schema = schema;
            Name = name;
        }

        /// <summary>
        ///     The name of the function in the underlying datastore.
        /// </summary>
        public string Name { get; [param: NotNull] set; }

        /// <summary>
        ///     The schema where the function lives in the underlying datastore.
        /// </summary>
        public string Schema { get; [param: CanBeNull] set; }
    }

    /// <summary>
    ///     Define a parameter for a user defined database function
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class DbFunctionParameterAttribute : Attribute
    {
        /// <summary>
        ///      Sets if this parameter is a database identifier.  Identifiers are inserted directly into into the underlying datastore command without modification.
        /// </summary>
        public bool IsIdentifier { get; [param: NotNull] set; }

        /// <summary>
        ///    Sets the index order for this parameter on the parent function
        /// </summary>
        public int ParameterIndex { get; [param: NotNull] set; }

        /// <summary>
        ///    Sets the constant value for this parameter.
        /// </summary>
        public object Value { get; [param: NotNull] set; }
    }
}
