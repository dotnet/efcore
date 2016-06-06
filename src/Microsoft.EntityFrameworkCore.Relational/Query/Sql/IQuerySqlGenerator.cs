// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query.Sql
{
    /// <summary>
    ///     A relational SQL generator.
    /// </summary>
    public interface IQuerySqlGenerator
    {
        /// <summary>
        ///     Generates SQL for the given parameter values.
        /// </summary>
        /// <param name="parameterValues"> The parameter values. </param>
        /// <returns>
        ///     The SQL.
        /// </returns>
        IRelationalCommand GenerateSql([NotNull] IReadOnlyDictionary<string, object> parameterValues);

        /// <summary>
        ///     Gets a value indicating whether the generated SQL is cacheable.
        /// </summary>
        /// <value>
        ///     true if the generated SQL is cacheable, false if not.
        /// </value>
        bool IsCacheable { get; }

        /// <summary>
        ///     Creates value buffer factory corresponding to the generated query.
        /// </summary>
        /// <param name="relationalValueBufferFactoryFactory"> The relational value buffer factory. </param>
        /// <param name="dataReader"> The data reader. </param>
        /// <returns>
        ///     The new value buffer factory.
        /// </returns>
        IRelationalValueBufferFactory CreateValueBufferFactory(
            [NotNull] IRelationalValueBufferFactoryFactory relationalValueBufferFactoryFactory,
            [NotNull] DbDataReader dataReader);
    }
}
