// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a db function in an <see cref="IModel" />.
    /// </summary>
    public interface IDbFunction : IAnnotatable
    {
        /// <summary>
        ///     The schema where the function lives in the underlying datastore.
        /// </summary>
        string Schema { get; }

        /// <summary>
        ///     The name of the function in the underlying datastore.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     The list of parameters which are passed to the underlying datastores function.
        /// </summary>
        IReadOnlyList<DbFunctionParameter> Parameters { get; }

        /// <summary>
        ///     The .Net method which maps to the function in the underlying datastore
        /// </summary>
        MethodInfo MethodInfo { get; }

        /// <summary>
        ///     The return type of the mapped .Net method
        /// </summary>
        Type ReturnType { get; }

        /// <summary>
        ///     <para>
        ///         If set this callback method is executed before the method call is translated to a DbFunctionExpression
        ///     </para>
        ///     <para>
        ///         If this method returns true then the db function is translated into a call to the underlying data store.
        ///         If this method returns false then the method is executed client side.
        ///     </para>
        /// </summary>
        bool BeforeDbFunctionExpressionCreate([NotNull] MethodCallExpression expression);

        /// <summary>
        ///     <para>
        ///         If set this callback method is executed after the methodcall is translated to a DbFunctionExpression.
        ///     </para>
        /// </summary>
        void AfterDbFunctionExpressionCreate([NotNull] DbFunctionExpression dbFuncExpression);

        /// <summary>
        ///     If set this callback is used to translate the .Net method call to a Linq Expression.
        /// </summary>
        Expression Translate([NotNull] IReadOnlyCollection<Expression> arguments);

        /// <summary>
        ///     Tells if a translate callback has been set.
        /// </summary>
        bool HasTranslateCallback { get; }
    }
}
