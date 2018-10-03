// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class GearsOfWarFromSqlQueryTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : GearsOfWarQueryRelationalFixture, new()
    {
        protected GearsOfWarFromSqlQueryTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        [Fact]
        public virtual void From_sql_queryable_simple_columns_out_of_order()
        {
            using (var context = CreateContext())
            {
                var actual = context.Set<Weapon>().FromSql(NormalizeDelimeters("SELECT [Id], [Name], [IsAutomatic], [AmmunitionType], [OwnerFullName], [SynergyWithId] FROM [Weapons] ORDER BY [Name]"))
                    .ToArray();

                Assert.Equal(10, actual.Length);

                var first = actual.First();

                Assert.Equal(AmmunitionType.Shell, first.AmmunitionType);
                Assert.Equal("Baird's Gnasher", first.Name);
            }
        }

        private RawSqlString NormalizeDelimeters(RawSqlString sql)
            => Fixture.TestStore.NormalizeDelimeters(sql);

        private FormattableString NormalizeDelimeters(FormattableString sql)
            => Fixture.TestStore.NormalizeDelimeters(sql);

        protected GearsOfWarContext CreateContext() => Fixture.CreateContext();

        protected virtual void ClearLog()
        {
        }
    }
}
