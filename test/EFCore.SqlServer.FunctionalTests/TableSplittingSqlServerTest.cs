// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore
{
    public class TableSplittingSqlServerTest : TableSplittingTestBase
    {
        public TableSplittingSqlServerTest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }

        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

        public override void Can_change_dependent_instance_non_derived()
        {
            base.Can_change_dependent_instance_non_derived();

            TestSqlLoggerFactory.AssertBaseline(new []{
                @"@p3='Trek Pro Fit Madone 6 Series' (Nullable = false) (Size = 450)
@p0='LicensedOperator' (Nullable = false) (Size = 4000)
@p1='repairman' (Size = 4000)
@p2='Repair' (Size = 4000)

SET NOCOUNT ON;
UPDATE [Vehicles] SET [Operator_Discriminator] = @p0, [Operator_Name] = @p1, [LicenseType] = @p2
WHERE [Name] = @p3;
SELECT @@ROWCOUNT;"
            }, assertOrder: false);
        }

        public override void Can_change_principal_instance_non_derived()
        {
            base.Can_change_principal_instance_non_derived();

            TestSqlLoggerFactory.AssertBaseline(new[]{
                @"@p1='Trek Pro Fit Madone 6 Series' (Nullable = false) (Size = 450)
@p0='2'

SET NOCOUNT ON;
UPDATE [Vehicles] SET [SeatingCapacity] = @p0
WHERE [Name] = @p1;
SELECT @@ROWCOUNT;"
            }, assertOrder: false);
        }
    }
}
