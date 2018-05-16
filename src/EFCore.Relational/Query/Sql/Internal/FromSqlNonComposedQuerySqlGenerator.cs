// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Sql.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class FromSqlNonComposedQuerySqlGenerator : DefaultQuerySqlGenerator
    {
        private readonly string _sql;
        private readonly Expression _arguments;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public FromSqlNonComposedQuerySqlGenerator(
            [NotNull] QuerySqlGeneratorDependencies dependencies,
            [NotNull] SelectExpression selectExpression,
            [NotNull] string sql,
            [NotNull] Expression arguments)
            : base(dependencies, selectExpression)
        {
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(arguments, nameof(arguments));

            _sql = sql;
            _arguments = arguments;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override Expression Visit(Expression expression)
        {
            GenerateFromSql(_sql, _arguments, ParameterValues);

            return expression;
        }

        /// <summary>
        ///     Whether or not the generated SQL could have out-of-order projection columns.
        /// </summary>
        public override bool RequiresRuntimeProjectionRemapping => true;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public override IRelationalValueBufferFactory CreateValueBufferFactory(
            IRelationalValueBufferFactoryFactory relationalValueBufferFactoryFactory, DbDataReader dataReader)
        {
            Check.NotNull(relationalValueBufferFactoryFactory, nameof(relationalValueBufferFactoryFactory));
            Check.NotNull(dataReader, nameof(dataReader));

            var readerColumns
                = Enumerable
                    .Range(0, dataReader.FieldCount)
                    .Select(
                        i => new
                        {
                            Name = dataReader.GetName(i),
                            Ordinal = i
                        })
                    .ToList();

            var types = new TypeMaterializationInfo[SelectExpression.Projection.Count];

            for (var i = 0; i < SelectExpression.Projection.Count; i++)
            {
                if (SelectExpression.Projection[i] is ColumnExpression columnExpression)
                {
                    var columnName = columnExpression.Name;

                    if (columnName != null)
                    {
                        var readerColumn
                            = readerColumns.SingleOrDefault(
                                c =>
                                    string.Equals(columnName, c.Name, StringComparison.OrdinalIgnoreCase));

                        if (readerColumn == null)
                        {
                            throw new InvalidOperationException(RelationalStrings.FromSqlMissingColumn(columnName));
                        }

                        types[i] = new TypeMaterializationInfo(
                            columnExpression.Type,
                            columnExpression.Property,
                            Dependencies.TypeMappingSource,
                            fromLeftOuterJoin: false,
                            readerColumn.Ordinal);
                    }
                }
            }

            return relationalValueBufferFactoryFactory.Create(types);
        }
    }
}
