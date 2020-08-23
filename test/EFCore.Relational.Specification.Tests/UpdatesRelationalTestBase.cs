// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class UpdatesRelationalTestBase<TFixture> : UpdatesTestBase<TFixture>
        where TFixture : UpdatesRelationalFixture
    {
        protected UpdatesRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public virtual void SaveChanges_works_for_entities_also_mapped_to_view()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var category = context.Categories.Single();

                    context.Add(
                        new ProductTableWithView
                        {
                            Id = Guid.NewGuid(),
                            Name = "Pear Cider",
                            Price = 1.39M,
                            DependentId = category.Id
                        });
                    context.Add(
                        new ProductViewTable
                        {
                            Id = Guid.NewGuid(),
                            Name = "Pear Cobler",
                            Price = 2.39M,
                            DependentId = category.Id
                        });

                    context.SaveChanges();
                },
                context =>
                {
                    var viewProduct = context.Set<ProductTableWithView>().Single();
                    var tableProduct = context.Set<ProductTableView>().Single();

                    Assert.Equal("Pear Cider", tableProduct.Name);
                    Assert.Equal("Pear Cobler", viewProduct.Name);
                });
        }

        [ConditionalFact]
        public virtual void SaveChanges_throws_for_entities_only_mapped_to_view()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var category = context.Categories.Single();
                    context.Add(
                        new ProductTableView
                        {
                            Id = Guid.NewGuid(),
                            Name = "Pear Cider",
                            Price = 1.39M,
                            DependentId = category.Id
                        });

                    Assert.Equal(
                        RelationalStrings.ReadonlyEntitySaved(nameof(ProductTableView)),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalFact]
        public abstract void Identifiers_are_generated_correctly();

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        protected override string UpdateConcurrencyMessage
            => RelationalStrings.UpdateConcurrencyException(1, 0);

        protected override string UpdateConcurrencyTokenMessage
            => RelationalStrings.UpdateConcurrencyException(1, 0);
    }
}
