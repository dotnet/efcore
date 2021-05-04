// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
