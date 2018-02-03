// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Inheritance;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class InheritanceRelationalTestBase<TFixture> : InheritanceTestBase<TFixture>
        where TFixture : InheritanceFixtureBase, new()
    {
        protected InheritanceRelationalTestBase(TFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public virtual void FromSql_on_root()
        {
            using (var context = CreateContext())
            {
                context.Set<Animal>().FromSql(@"select * from ""Animal""").ToList();
            }
        }

        [Fact]
        public virtual void FromSql_on_derived()
        {
            using (var context = CreateContext())
            {
                context.Set<Eagle>().FromSql(@"select * from ""Animal""").ToList();
            }
        }
    }
}
