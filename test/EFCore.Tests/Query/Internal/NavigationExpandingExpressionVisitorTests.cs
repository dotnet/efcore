// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

public class NavigationExpandingExpressionVisitorTests
{
    private class TestInterceptors : IInterceptors
    {
        public TInterceptor Aggregate<TInterceptor>()
            where TInterceptor : class, IInterceptor
            => null;
    }

    private class TestNavigationExpandingExpressionVisitor() : NavigationExpandingExpressionVisitor(
        null,
        new QueryCompilationContext(
            new QueryCompilationContextDependencies(
                model: null,
                queryTranslationPreprocessorFactory: null,
                queryableMethodTranslatingExpressionVisitorFactory: null,
                queryTranslationPostprocessorFactory: null,
                shapedQueryCompilingExpressionVisitorFactory: null,
                liftableConstantFactory: null,
                liftableConstantProcessor: null,
                new ExecutionStrategyTest.TestExecutionStrategy(new MyDemoContext()),
                new CurrentDbContext(new MyDemoContext()),
                contextOptions: null,
                logger: null,
                new TestInterceptors()
            ), async: false),
        null,
        null)
    {
        public Expression TestVisitExtension(Expression extensionExpression)
            => base.VisitExtension(extensionExpression);
    }

    private class MyDemoContext : DbContext
    {
        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseInMemoryDatabase(databaseName: "test");
    }

    private class TestEntityQueryRootExpression : EntityQueryRootExpression
    {
        public int VisitCounter;

        public TestEntityQueryRootExpression(IAsyncQueryProvider asyncQueryProvider, IEntityType entityType)
            : base(asyncQueryProvider, entityType)
        {
        }

        public TestEntityQueryRootExpression(IEntityType entityType)
            : base(entityType)
        {
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            VisitCounter++;
            return this;
        }
    }

    private class A
    {
        public int B { get; set; }
    }

    [ConditionalFact]
    public void Visits_extension_children()
    {
        var model = new Model();
        var e = model.AddEntityType(typeof(A), false, ConfigurationSource.Explicit);
        var visitor = new TestNavigationExpandingExpressionVisitor();
        var testExpression = new TestEntityQueryRootExpression(e);

        visitor.TestVisitExtension(testExpression);

        Assert.Equal(1, testExpression.VisitCounter);
    }
}
