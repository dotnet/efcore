// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
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
        public abstract void Identifiers_are_generated_correctly();

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        protected override string UpdateConcurrencyMessage
            => RelationalStrings.UpdateConcurrencyException(1, 0);

        protected override string UpdateConcurrencyTokenMessage
            => RelationalStrings.UpdateConcurrencyException(1, 0);
    }
}
