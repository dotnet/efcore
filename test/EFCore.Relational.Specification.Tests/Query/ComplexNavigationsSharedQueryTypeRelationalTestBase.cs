﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsSharedQueryTypeRelationalTestBase<TFixture> : ComplexNavigationsSharedTypeQueryTestBase<TFixture>
        where TFixture : ComplexNavigationsSharedTypeQueryRelationalFixtureBase, new()
    {
        protected ComplexNavigationsSharedQueryTypeRelationalTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        public override Task Complex_query_with_optional_navigations_and_client_side_evaluation(bool async)
        {
            return AssertTranslationFailed(() => base.Complex_query_with_optional_navigations_and_client_side_evaluation(async));
        }

        protected virtual bool CanExecuteQueryString
            => false;

        protected override QueryAsserter CreateQueryAsserter(TFixture fixture)
            => new RelationalQueryAsserter(
                fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression, canExecuteQueryString: CanExecuteQueryString);
    }
}
