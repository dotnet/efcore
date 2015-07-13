// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Metadata.Builders;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.Tests
{
    public class ModelBuilderNonGenericStringTest : ModelBuilderNonGenericTest
    {
        public class NonGenericOneToManyType : OneToManyTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
            {
                return new NonGenericStringTestModelBuilder(modelBuilder);
            }
        }

        public class NonGenericManyToOneType : ManyToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
            {
                return new NonGenericStringTestModelBuilder(modelBuilder);
            }
        }

        public class NonGenericOneToOneType : OneToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
            {
                return new NonGenericStringTestModelBuilder(modelBuilder);
            }
        }

        private class NonGenericStringTestModelBuilder : TestModelBuilder
        {
            public NonGenericStringTestModelBuilder(ModelBuilder modelBuilder)
                : base(modelBuilder)
            {
            }

            public override TestEntityTypeBuilder<TEntity> Entity<TEntity>()
                => new NonGenericStringTestEntityTypeBuilder<TEntity>(ModelBuilder.Entity(typeof(TEntity)));

            public override TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
                => new NonGenericStringTestModelBuilder(ModelBuilder.Entity(typeof(TEntity), entityTypeBuilder =>
                    buildAction(new NonGenericStringTestEntityTypeBuilder<TEntity>(entityTypeBuilder))));

            public override TestModelBuilder Ignore<TEntity>()
                => new NonGenericStringTestModelBuilder(ModelBuilder.Ignore(typeof(TEntity)));
        }

        private class NonGenericStringTestEntityTypeBuilder<TEntity> : NonGenericTestEntityTypeBuilder<TEntity>
            where TEntity : class
        {
            public NonGenericStringTestEntityTypeBuilder(EntityTypeBuilder entityTypeBuilder)
                : base(entityTypeBuilder)
            {
            }

            protected override NonGenericTestEntityTypeBuilder<TEntity> Wrap(EntityTypeBuilder entityTypeBuilder)
                => new NonGenericStringTestEntityTypeBuilder<TEntity>(entityTypeBuilder);

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> Reference<TRelatedEntity>(Expression<Func<TEntity, TRelatedEntity>> reference = null)
                => new NonGenericStringTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.Reference(typeof(TRelatedEntity).FullName, reference?.GetPropertyAccess().Name));

            public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> Collection<TRelatedEntity>(Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> collection = null)
                => new NonGenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.Collection(typeof(TRelatedEntity).FullName, collection?.GetPropertyAccess().Name));
        }

        private class NonGenericStringTestReferenceNavigationBuilder<TEntity, TRelatedEntity> : NonGenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public NonGenericStringTestReferenceNavigationBuilder(ReferenceNavigationBuilder referenceNavigationBuilder)
                : base(referenceNavigationBuilder)
            {
            }

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> InverseReference(Expression<Func<TRelatedEntity, TEntity>> reference = null)
            {
                var referenceName = reference?.GetPropertyAccess().Name;
                return new NonGenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(ReferenceNavigationBuilder.InverseReference(referenceName));
            }
        }

        private class NonGenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity> : NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public NonGenericStringTestReferenceReferenceBuilder(ReferenceReferenceBuilder referenceReferenceBuilder)
                : base(referenceReferenceBuilder)
            {
            }

            protected override NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> Wrap(ReferenceReferenceBuilder referenceReferenceBuilder)
                => new NonGenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(referenceReferenceBuilder);

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> ForeignKey<TDependentEntity>(Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                => Wrap(ReferenceReferenceBuilder.ForeignKey(typeof(TDependentEntity).FullName, foreignKeyExpression.GetPropertyAccessList().Select(p => p.Name).ToArray()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> PrincipalKey<TPrincipalEntity>(Expression<Func<TPrincipalEntity, object>> keyExpression)
                => Wrap(ReferenceReferenceBuilder.PrincipalKey(typeof(TPrincipalEntity).FullName, keyExpression.GetPropertyAccessList().Select(p => p.Name).ToArray()));
        }
    }
}
