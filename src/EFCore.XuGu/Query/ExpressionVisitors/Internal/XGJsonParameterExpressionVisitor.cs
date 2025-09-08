// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Query.Internal;
using Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal
{
    public class XGJsonParameterExpressionVisitor : ExpressionVisitor
    {
        private readonly XGSqlExpressionFactory _sqlExpressionFactory;
        private readonly IXGOptions _options;

        public XGJsonParameterExpressionVisitor(XGSqlExpressionFactory sqlExpressionFactory, IXGOptions options)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
            _options = options;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
            => extensionExpression switch
            {
                SqlParameterExpression sqlParameterExpression => VisitParameter(sqlParameterExpression),
                ShapedQueryExpression shapedQueryExpression => shapedQueryExpression.Update(Visit(shapedQueryExpression.QueryExpression), Visit(shapedQueryExpression.ShaperExpression)),
                _ => base.VisitExtension(extensionExpression)
            };

        protected virtual SqlExpression VisitParameter(SqlParameterExpression sqlParameterExpression)
        {
            if (sqlParameterExpression.TypeMapping is XGJsonTypeMapping)
            {
                var typeMapping = _sqlExpressionFactory.FindMapping(sqlParameterExpression.Type, "json");

                // MySQL has a real JSON datatype, and string parameters need to be converted to it.
                // MariaDB defines the JSON datatype just as a synonym for LONGTEXT.
                if (!_options.ServerVersion.Supports.JsonDataTypeEmulation)
                {
                    return _sqlExpressionFactory.Convert(
                        sqlParameterExpression,
                        typeMapping.ClrType, // will be typeof(string) when `sqlParameterExpression.Type`
                        typeMapping);        // is typeof(XGJsonString)
                }
            }

            return sqlParameterExpression;
        }
    }
}
