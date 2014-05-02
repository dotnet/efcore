// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using JetBrains.Annotations;
using Microsoft.AspNet.Logging;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Relational
{
    public partial class RelationalDataStore
    {
        private class QueryModelVisitor : EntityQueryModelVisitor
        {
            public QueryModelVisitor()
                : base(null)
            {
            }

            private QueryModelVisitor(EntityQueryModelVisitor parentQueryModelVisitor)
                : base(parentQueryModelVisitor)
            {
            }

            protected override ExpressionTreeVisitor CreateQueryingExpressionTreeVisitor(EntityQueryModelVisitor parentQueryModelVisitor)
            {
                return new RelationalQueryingExpressionTreeVisitor(parentQueryModelVisitor);
            }

            protected override ExpressionTreeVisitor CreateProjectionExpressionTreeVisitor(EntityQueryModelVisitor parentQueryModelVisitor)
            {
                return new RelationalProjectionSubQueryExpressionTreeVisitor(parentQueryModelVisitor);
            }

            private static readonly MethodInfo _entityScanMethodInfo
                = typeof(QueryModelVisitor).GetTypeInfo().GetDeclaredMethod("EntityScan");

            [UsedImplicitly]
            private static IEnumerable<TEntity> EntityScan<TEntity>(QueryContext queryContext)
            {
                var entityType = queryContext.Model.GetEntityType(typeof(TEntity));

                var sql = new StringBuilder();

                sql.Append("SELECT ")
                    .AppendJoin(entityType.Properties.Select(p => p.StorageName))
                    .AppendLine()
                    .Append("FROM ")
                    .AppendLine(entityType.StorageName);

                return new Enumerable<TEntity>((RelationalQueryContext)queryContext, sql.ToString(), entityType);
            }

            private class RelationalQueryingExpressionTreeVisitor : QueryingExpressionTreeVisitor
            {
                public RelationalQueryingExpressionTreeVisitor(EntityQueryModelVisitor parentQueryModelVisitor)
                    : base(parentQueryModelVisitor)
                {
                }

                protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
                {
                    var queryModelVisitor = new QueryModelVisitor(_parentQueryModelVisitor);

                    queryModelVisitor.VisitQueryModel(expression.QueryModel);

                    return queryModelVisitor._expression;
                }

                protected override Expression VisitEntityQueryable(Type elementType)
                {
                    return Expression.Call(
                        _entityScanMethodInfo.MakeGenericMethod(elementType),
                        _queryContextParameter);
                }
            }

            private class RelationalProjectionSubQueryExpressionTreeVisitor : RelationalQueryingExpressionTreeVisitor
            {
                public RelationalProjectionSubQueryExpressionTreeVisitor(EntityQueryModelVisitor parentQueryModelVisitor)
                    : base(parentQueryModelVisitor)
                {
                }

                protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
                {
                    return VisitProjectionSubQuery(expression, new QueryModelVisitor(_parentQueryModelVisitor));
                }
            }

            private sealed class Enumerable<T> : IEnumerable<T>
            {
                private readonly RelationalQueryContext _queryContext;
                private readonly string _sql;
                private readonly IEntityType _entityType;

                public Enumerable(RelationalQueryContext queryContext, string sql, IEntityType entityType)
                {
                    _queryContext = queryContext;
                    _sql = sql;
                    _entityType = entityType;
                }

                public IEnumerator<T> GetEnumerator()
                {
                    return new Enumerator<T>(_queryContext, _sql, _entityType);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }

            private sealed class Enumerator<T> : IEnumerator<T>
            {
                private readonly RelationalQueryContext _queryContext;
                private readonly string _sql;
                private readonly IEntityType _entityType;

                private RelationalConnection _connection;
                private DbCommand _command;
                private DbDataReader _reader;

                public Enumerator(RelationalQueryContext queryContext, string sql, IEntityType entityType)
                {
                    _queryContext = queryContext;
                    _sql = sql;
                    _entityType = entityType;
                }

                public bool MoveNext()
                {
                    if (_reader == null)
                    {
                        _connection = _queryContext.Connection;
                        _connection.Open();

                        _command = _connection.DbConnection.CreateCommand();
                        _command.CommandText = _sql;

                        _queryContext.Logger.WriteSql(_sql);

                        _reader = _command.ExecuteReader();
                    }

                    return _reader.Read();
                }

                public T Current
                {
                    get
                    {
                        if (_reader == null)
                        {
                            return default(T);
                        }

                        return (T)_queryContext.StateManager
                            .GetOrMaterializeEntry(
                                _entityType, _queryContext.ValueReaderFactory.Create(_reader)).Entity;
                    }
                }

                object IEnumerator.Current
                {
                    get { return Current; }
                }

                public void Dispose()
                {
                    if (_reader != null)
                    {
                        _reader.Dispose();
                    }

                    if (_command != null)
                    {
                        _command.Dispose();
                    } 
                    
                    if (_connection != null)
                    {
                        _connection.Close();
                    }
                }

                public void Reset()
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
