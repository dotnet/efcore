// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore
{
    public class OptimisticConcurrencySqliteTest : OptimisticConcurrencyRelationalTestBase<F1SqliteFixture>
    {
        public OptimisticConcurrencySqliteTest(F1SqliteFixture fixture)
            : base(fixture)
        {
        }

        public override void Property_entry_original_value_is_set()
        {
            base.Property_entry_original_value_is_set();

            AssertSql(
                @"SELECT ""e"".""Id"", ""e"".""EngineSupplierId"", ""e"".""Name"", ""e"".""StorageLocation_Latitude"", ""e"".""StorageLocation_Longitude""
FROM ""Engines"" AS ""e""
ORDER BY ""e"".""Id""
LIMIT 1",
                //
                @"@p1='1' (DbType = String)
@p2='Mercedes' (Size = 8)
@p0='FO 108X' (Size = 7)
@p3='ChangedEngine' (Size = 13)
@p4='47.64491' (Nullable = true) (DbType = String)
@p5='-122.128101' (Nullable = true) (DbType = String)

UPDATE ""Engines"" SET ""Name"" = @p0
WHERE ""Id"" = @p1 AND ""EngineSupplierId"" = @p2 AND ""Name"" = @p3 AND ""StorageLocation_Latitude"" = @p4 AND ""StorageLocation_Longitude"" = @p5;
SELECT changes();");
        }

        // Override failing tests because SQLite does not allow store-generated row versions.
        // Row version behavior could be imitated on SQLite. See Issue #2195
        public override Task Simple_concurrency_exception_can_be_resolved_with_store_values()
            => Task.FromResult(true);

        public override Task Simple_concurrency_exception_can_be_resolved_with_client_values()
            => Task.FromResult(true);

        public override Task Simple_concurrency_exception_can_be_resolved_with_new_values()
            => Task.FromResult(true);

        public override Task Simple_concurrency_exception_can_be_resolved_with_store_values_using_equivalent_of_accept_changes()
            => Task.FromResult(true);

        public override Task Simple_concurrency_exception_can_be_resolved_with_store_values_using_Reload()
            => Task.FromResult(true);

        public override Task Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException()
            => Task.FromResult(true);

        public override Task
            Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values()
            => Task.FromResult(true);

        public override Task
            Change_in_independent_association_after_change_in_different_concurrency_token_results_in_independent_association_exception()
            => Task.FromResult(true);

        public override Task Change_in_independent_association_results_in_independent_association_exception()
            => Task.FromResult(true);

        public override Task Two_concurrency_issues_in_one_to_many_related_entities_can_be_handled_by_dealing_with_dependent_first()
            => Task.FromResult(true);

        public override Task Two_concurrency_issues_in_one_to_one_related_entities_can_be_handled_by_dealing_with_dependent_first()
            => Task.FromResult(true);

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());
    }
}
