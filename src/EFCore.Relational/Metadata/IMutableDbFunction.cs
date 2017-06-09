// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    public interface IMutableDbFunction : IDbFunction
    {
        /// <summary>
        ///     The schema where the function lives in the underlying datastore.
        /// </summary>
        new string Schema { get; [param: CanBeNull] set; }

        /// <summary>
        ///     The name of the function in the underlying datastore.
        /// </summary>
        new string Name { get; [param: NotNull] set;}

        /// <summary>
        ///     The return type of the mapped .Net method
        /// </summary>
        new Type ReturnType { get; [param: NotNull] set;}

        /// <summary>
        ///    A translate callback for converting a method call into a sql function
        /// </summary>
        new Func<IReadOnlyCollection<Expression>, IDbFunction, SqlFunctionExpression> TranslateCallback { get; [param: CanBeNull] set; }

        /// <summary>
        ///    Add a dbFunctionParameter to this DbFunction
        /// </summary>
        DbFunctionParameter AddParameter([NotNull] string name);
    }
}
