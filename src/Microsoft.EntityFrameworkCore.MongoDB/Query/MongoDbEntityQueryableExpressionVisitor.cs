using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using MongoDB.Driver;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class MongoDbEntityQueryableExpressionVisitor : EntityQueryableExpressionVisitor
    {
        private readonly IMongoDbConnection _mongoDbConnection;

        private static readonly MethodInfo _getCollectionMethod = typeof(IMongoDbConnection).GetTypeInfo()
            .GetMethod(nameof(IMongoDbConnection.GetCollection))
            .GetGenericMethodDefinition();

        private static readonly MethodInfo _asQueryableMethod = typeof(IMongoCollectionExtensions).GetTypeInfo()
            .GetMethod(nameof(IMongoCollectionExtensions.AsQueryable))
            .GetGenericMethodDefinition();

        public MongoDbEntityQueryableExpressionVisitor([NotNull] EntityQueryModelVisitor entityQueryModelVisitor,
            [NotNull] IMongoDbConnection mongoDbConnection)
            : base(entityQueryModelVisitor)
        {
            _mongoDbConnection = Check.NotNull(mongoDbConnection, nameof(mongoDbConnection));
        }

        protected override Expression VisitEntityQueryable(Type elementType)
            => Expression.Call(
                _asQueryableMethod.MakeGenericMethod(elementType),
                Expression.Call(
                    Expression.Constant(_mongoDbConnection),
                    _getCollectionMethod.MakeGenericMethod(elementType)),
                Expression.Constant(value: null, type: typeof(AggregateOptions)));
    }
}