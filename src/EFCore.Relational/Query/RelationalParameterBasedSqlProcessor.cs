// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A class that processes the <see cref="SelectExpression" />  after parementer values are known.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class RelationalParameterBasedSqlProcessor
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="QueryTranslationPostprocessor" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this class. </param>
        /// <param name="useRelationalNulls"> A bool value indicating if relational nulls should be used. </param>
        public RelationalParameterBasedSqlProcessor(
            [NotNull] RelationalParameterBasedSqlProcessorDependencies dependencies,
            bool useRelationalNulls)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
            UseRelationalNulls = useRelationalNulls;
        }

        /// <summary>
        ///     Parameter object containing service dependencies.
        /// </summary>
        protected virtual RelationalParameterBasedSqlProcessorDependencies Dependencies { get; }

        /// <summary>
        ///     A bool value indicating if relational nulls should be used.
        /// </summary>
        protected virtual bool UseRelationalNulls { get; }

        /// <summary>
        ///     Optimizes the <see cref="SelectExpression" /> for given parameter values.
        /// </summary>
        /// <param name="selectExpression"> A select expression to optimize. </param>
        /// <param name="parametersValues"> A dictionary of parameter values to use. </param>
        /// <param name="canCache"> A bool value indicating if the select expression can be cached. </param>
        /// <returns> An optimized select expression. </returns>
        public virtual SelectExpression Optimize(
            [NotNull] SelectExpression selectExpression,
            [NotNull] IReadOnlyDictionary<string, object> parametersValues,
            out bool canCache)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));
            Check.NotNull(parametersValues, nameof(parametersValues));

            canCache = true;
            selectExpression = ProcessSqlNullability(selectExpression, parametersValues, out var sqlNullablityCanCache);
            canCache &= sqlNullablityCanCache;

            selectExpression = ExpandFromSqlParameter(selectExpression, parametersValues, out var fromSqlParameterCanCache);
            canCache &= fromSqlParameterCanCache;

            return selectExpression;
        }

        /// <summary>
        ///     Processes the <see cref="SelectExpression" /> based on nullability of nodes to apply null semantics in use and
        ///     optimize it for given parameter values.
        /// </summary>
        /// <param name="selectExpression"> A select expression to optimize. </param>
        /// <param name="parametersValues"> A dictionary of parameter values to use. </param>
        /// <param name="canCache"> A bool value indicating if the select expression can be cached. </param>
        /// <returns> A processed select expression. </returns>
        protected virtual SelectExpression ProcessSqlNullability(
            [NotNull] SelectExpression selectExpression,
            [NotNull] IReadOnlyDictionary<string, object> parametersValues,
            out bool canCache)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));
            Check.NotNull(parametersValues, nameof(parametersValues));

            return new SqlNullabilityProcessor(Dependencies, UseRelationalNulls).Process(selectExpression, parametersValues, out canCache);
        }

        /// <summary>
        ///     Expands the parameters to <see cref="FromSqlExpression" /> inside the <see cref="SelectExpression" /> for given parameter values.
        /// </summary>
        /// <param name="selectExpression"> A select expression to optimize. </param>
        /// <param name="parametersValues"> A dictionary of parameter values to use. </param>
        /// <param name="canCache"> A bool value indicating if the select expression can be cached. </param>
        /// <returns> A processed select expression. </returns>
        protected virtual SelectExpression ExpandFromSqlParameter(
            [NotNull] SelectExpression selectExpression,
            [NotNull] IReadOnlyDictionary<string, object> parametersValues,
            out bool canCache)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));
            Check.NotNull(parametersValues, nameof(parametersValues));

            return new FromSqlParameterExpandingExpressionVisitor(Dependencies).Expand(selectExpression, parametersValues, out canCache);
        }
    }
}
