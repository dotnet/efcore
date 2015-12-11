// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Sql.Internal
{
    public class FromSqlNonComposedQuerySqlGenerator : DefaultQuerySqlGenerator
    {
        private readonly string _sql;
        private readonly Expression _arguments;

        public FromSqlNonComposedQuerySqlGenerator(
            [NotNull] IRelationalCommandBuilderFactory relationalCommandBuilderFactory,
            [NotNull] ISqlGenerationHelper sqlGenerationHelper,
            [NotNull] IParameterNameGeneratorFactory parameterNameGeneratorFactory,
            [NotNull] IRelationalTypeMapper relationalTypeMapper,
            [NotNull] SelectExpression selectExpression,
            [NotNull] string sql,
            [NotNull] Expression arguments)
            : base(
                Check.NotNull(relationalCommandBuilderFactory, nameof(relationalCommandBuilderFactory)),
                Check.NotNull(sqlGenerationHelper, nameof(sqlGenerationHelper)),
                Check.NotNull(parameterNameGeneratorFactory, nameof(parameterNameGeneratorFactory)),
                Check.NotNull(relationalTypeMapper, nameof(relationalTypeMapper)),
                Check.NotNull(selectExpression, nameof(selectExpression)))
        {
            Check.NotEmpty(sql, nameof(sql));
            Check.NotNull(arguments, nameof(arguments));

            _sql = sql;
            _arguments = arguments;
        }

        public override Expression Visit(Expression expression)
        {
            GenerateFromSql(_sql, _arguments, ParameterValues);

            return expression;
        }

        public override IRelationalValueBufferFactory CreateValueBufferFactory(
            IRelationalValueBufferFactoryFactory relationalValueBufferFactoryFactory, DbDataReader dataReader)
        {
            Check.NotNull(relationalValueBufferFactoryFactory, nameof(relationalValueBufferFactoryFactory));
            Check.NotNull(dataReader, nameof(dataReader));

            var readerColumns
                = Enumerable
                    .Range(0, dataReader.FieldCount)
                    .Select(i => new
                    {
                        Name = dataReader.GetName(i),
                        Ordinal = i
                    })
                    .ToList();

            var types = new Type[SelectExpression.Projection.Count];
            var indexMap = new int[SelectExpression.Projection.Count];

            for (var i = 0; i < SelectExpression.Projection.Count; i++)
            {
                var aliasExpression = SelectExpression.Projection[i] as AliasExpression;

                if (aliasExpression != null)
                {
                    var columnName
                        = aliasExpression.Alias
                          ?? aliasExpression.TryGetColumnExpression()?.Name;

                    if (columnName != null)
                    {
                        var readerColumn
                            = readerColumns.SingleOrDefault(c =>
                                string.Equals(columnName, c.Name, StringComparison.OrdinalIgnoreCase));

                        if (readerColumn == null)
                        {
                            throw new InvalidOperationException(RelationalStrings.FromSqlMissingColumn(columnName));
                        }

                        types[i] = SelectExpression.Projection[i].Type;
                        indexMap[i] = readerColumn.Ordinal;
                    }
                }
            }

            return relationalValueBufferFactoryFactory.Create(types, indexMap);
        }
    }
}
