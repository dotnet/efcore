using System;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using MongoDB.Driver;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class MongoDbDatabaseProviderServices : DatabaseProviderServices, IMongoDbDatabaseProviderServices
    {
        private readonly IDbContextOptions _dbContextOptions;

        public MongoDbDatabaseProviderServices(
            [NotNull] IDbContextOptions dbContextOptions,
            [NotNull] IServiceProvider serviceProvider)
            : base(Check.NotNull(serviceProvider, nameof(serviceProvider)))
        {
            _dbContextOptions = Check.NotNull(dbContextOptions, nameof(dbContextOptions));
        }

        public override string InvariantName
            => GetType().GetTypeInfo().Assembly.GetName().Name;

        public override IDatabase Database
            => GetService<MongoDbDatabase>();

        public override IDbContextTransactionManager TransactionManager
            => null;

        public override IDatabaseCreator Creator
            => GetService<MongoDbDatabaseCreator>();

        public override IValueGeneratorSelector ValueGeneratorSelector
            => GetService<MongoDbValueGeneratorSelector>();

        public override IConventionSetBuilder ConventionSetBuilder
            => GetService<MongoDbConventionSetBuilder>();

        public override IModelSource ModelSource
            => GetService<MongoDbModelSource>();

        public override IModelValidator ModelValidator
            => GetService<MongoDbModelValidator>();

        public override IValueGeneratorCache ValueGeneratorCache
            => GetService<MongoDbValueGeneratorCache>();

        public override IQueryContextFactory QueryContextFactory
            => GetService<MongoDbQueryContextFactory>();

        //public override IQueryCompilationContextFactory QueryCompilationContextFactory
            //=> GetService<IQueryCompilationContextFactory>();

        public override IEntityQueryModelVisitorFactory EntityQueryModelVisitorFactory
            => GetService<MongoDbEntityQueryModelVisitorFactory>();

        //public override ICompiledQueryCacheKeyGenerator CompiledQueryCacheKeyGenerator
            //=> GetService<ICompiledQueryCacheKeyGenerator>();

        //public override IExpressionPrinter ExpressionPrinter
            //=> GetService<IExpressionPrinter>();

        //public override IResultOperatorHandler ResultOperatorHandler
            //=> GetService<IResultOperatorHandler>();

        public override IEntityQueryableExpressionVisitorFactory EntityQueryableExpressionVisitorFactory
            => GetService<MongoDbEntityQueryableExpressionVisitorFactory>();

        //public override IProjectionExpressionVisitorFactory ProjectionExpressionVisitorFactory
            //=> GetService<IProjectionExpressionVisitorFactory>();

        public virtual IMongoClient MongoClient
            => _dbContextOptions.FindExtension<MongoDbOptionsExtension>()?.MongoClient
               ?? new MongoClient();
    }
}