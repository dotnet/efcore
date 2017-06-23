// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Defines a user defined database function
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DbFunctionAttribute : Attribute
    {
        private string _functionName;

        /// <summary>
        /// Defines a user defined database function.  By convention uses the .NET method name as name of the database function and the default schema.
        /// </summary>
        public DbFunctionAttribute()
        {
        }

        /// <summary>
        /// Defines a user defined database function
        /// </summary>
        /// <param name="functionName">The name of the function in the underlying datastore.</param>
        /// <param name="schema">The schema where the function lives in the underlying datastore.</param>
        public DbFunctionAttribute([NotNull] string functionName, [CanBeNull] string schema = null)
        {
            Check.NotEmpty(functionName, nameof(functionName));

            Schema = schema;
            FunctionName = functionName;
        }

        /// <summary>
        ///     The name of the function in the underlying datastore.
        /// </summary>
        public virtual string FunctionName
        {
            get { return _functionName; }

            [param: NotNull]
            set
            {
                Check.NotEmpty(value, nameof(FunctionName));
                _functionName = value;
            }
        }

        /// <summary>
        ///     The schema where the function lives in the underlying datastore.
        /// </summary>
        public virtual string Schema { get; [param: CanBeNull] set; }
    }
}
