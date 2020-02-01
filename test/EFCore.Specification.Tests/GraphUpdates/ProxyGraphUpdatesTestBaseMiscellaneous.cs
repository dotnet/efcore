// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable AccessToModifiedClosure
// ReSharper disable PossibleMultipleEnumeration
namespace Microsoft.EntityFrameworkCore
{
    public abstract partial class ProxyGraphUpdatesTestBase<TFixture>
        where TFixture : ProxyGraphUpdatesTestBase<TFixture>.ProxyGraphUpdatesFixtureBase, new()
    {
        [ConditionalFact]
        public virtual void No_fixup_to_Deleted_entities()
        {
            using var context = CreateContext();

            var root = LoadRoot(context);
            if (!DoesLazyLoading)
            {
                context.Entry(root).Collection(e => e.OptionalChildren).Load();
            }

            var existing = root.OptionalChildren.OrderBy(e => e.Id).First();

            existing.Parent = null;
            existing.ParentId = null;
            ((ICollection<Optional1>)root.OptionalChildren).Remove(existing);

            context.Entry(existing).State = EntityState.Deleted;

            var queried = context.Set<Optional1>().ToList();

            Assert.Null(existing.Parent);
            Assert.Null(existing.ParentId);
            Assert.Single(root.OptionalChildren);
            Assert.DoesNotContain(existing, root.OptionalChildren);

            Assert.Equal(2, queried.Count);
            Assert.Contains(existing, queried);
        }

        [ConditionalFact]
        public virtual void Sometimes_not_calling_DetectChanges_when_required_does_not_throw_for_null_ref()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var dependent = context.Set<BadOrder>().Single();

                    dependent.BadCustomerId = null;

                    var principal = context.Set<BadCustomer>().Single();

                    principal.Status++;

                    Assert.Null(dependent.BadCustomerId);
                    Assert.Null(dependent.BadCustomer);
                    Assert.Empty(principal.BadOrders);

                    context.SaveChanges();

                    Assert.Null(dependent.BadCustomerId);
                    Assert.Null(dependent.BadCustomer);
                    Assert.Empty(principal.BadOrders);
                },
                context =>
                {
                    var dependent = context.Set<BadOrder>().Single();
                    var principal = context.Set<BadCustomer>().Single();

                    Assert.Null(dependent.BadCustomerId);
                    Assert.Null(dependent.BadCustomer);
                    Assert.Empty(principal.BadOrders);
                });
        }
    }
}
