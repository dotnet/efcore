// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
#if Test20
using Microsoft.EntityFrameworkCore.Internal;
#else
using Microsoft.EntityFrameworkCore.InMemory.Internal;
#endif
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class UpdatesInMemoryTestBase<TFixture> : UpdatesTestBase<TFixture>
        where TFixture : UpdatesInMemoryFixtureBase
    {
        protected UpdatesInMemoryTestBase(TFixture fixture)
            : base(fixture)
        {
        }

#if !Test20
        [Fact]
        public virtual void Update_on_bytes_concurrency_token_original_value_matches_throws_with_quirk()
        {
            var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

            try
            {
                AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue12214", true);

                ExecuteWithStrategyInTransaction(
                    context =>
                    {
                        var entry = context.ProductWithBytes.Attach(
                            new ProductWithBytes
                            {
                                Id = productId,
                                Name = "MegaChips",
                                Bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
                            });

                        entry.Entity.Name = "GigaChips";

                        Assert.Throws<DbUpdateConcurrencyException>(
                            () => context.SaveChanges());
                    },
                    context =>
                    {
                        Assert.Equal("MegaChips", context.ProductWithBytes.Find(productId).Name);
                    });

            }
            finally
            {
                AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue12214", false);
            }
        }

        [Fact]
        public virtual void Remove_on_bytes_concurrency_token_original_value_matches_throws_with_quirk()
        {
            var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

            try
            {
                AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue12214", true);

                ExecuteWithStrategyInTransaction(
                    context =>
                    {
                        var entry = context.ProductWithBytes.Attach(
                            new ProductWithBytes
                            {
                                Id = productId,
                                Name = "MegaChips",
                                Bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
                            });

                        entry.State = EntityState.Deleted;

                        Assert.Throws<DbUpdateConcurrencyException>(
                            () => context.SaveChanges());
                    },
                    context =>
                    {
                        Assert.Equal("MegaChips", context.ProductWithBytes.Find(productId).Name);
                    });

            }
            finally
            {
                AppContext.SetSwitch("Microsoft.EntityFrameworkCore.Issue12214", false);
            }
        }
#endif

        protected override string UpdateConcurrencyMessage
            => InMemoryStrings.UpdateConcurrencyException;

        protected override void ExecuteWithStrategyInTransaction(
            Action<UpdatesContext> testOperation, Action<UpdatesContext> nestedTestOperation1 = null)
        {
            base.ExecuteWithStrategyInTransaction(testOperation, nestedTestOperation1);
            Fixture.Reseed();
        }

        protected override async Task ExecuteWithStrategyInTransactionAsync(
            Func<UpdatesContext, Task> testOperation, Func<UpdatesContext, Task> nestedTestOperation1 = null)
        {
            await base.ExecuteWithStrategyInTransactionAsync(testOperation, nestedTestOperation1);
            Fixture.Reseed();
        }
    }
}
