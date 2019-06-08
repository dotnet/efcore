// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Inheritance;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class InheritanceRelationalTestBase<TFixture> : InheritanceTestBase<TFixture>
        where TFixture : InheritanceFixtureBase, new()
    {
        protected InheritanceRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact]
        public virtual void FromSql_on_root()
        {
            using (var context = CreateContext())
            {
                context.Set<Animal>().FromSqlRaw(NormalizeDelimetersInRawString("select * from [Animal]")).ToList();
            }
        }

        [ConditionalFact]
        public virtual void FromSql_on_derived()
        {
            using (var context = CreateContext())
            {
                context.Set<Eagle>().FromSqlRaw(NormalizeDelimetersInRawString("select * from [Animal]")).ToList();
            }
        }

        private string NormalizeDelimetersInRawString(string sql)
            => ((RelationalTestStore)Fixture.TestStore).NormalizeDelimetersInRawString(sql);

        private FormattableString NormalizeDelimetersInInterpolatedString(FormattableString sql)
            => ((RelationalTestStore)Fixture.TestStore).NormalizeDelimetersInInterpolatedString(sql);
    }
}
