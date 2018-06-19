// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.EntityFrameworkCore
{
    public class UpdatesSqlServerTest : UpdatesRelationalTestBase<UpdatesSqlServerFixture>
    {
        // ReSharper disable once UnusedParameter.Local
        public UpdatesSqlServerTest(UpdatesSqlServerFixture fixture, ITestOutputHelper testOutputHelper)
            : base(fixture)
        {
            //Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
            Fixture.TestSqlLoggerFactory.Clear();
        }

        public override void Save_replaced_principal()
        {
            base.Save_replaced_principal();

            Fixture.TestSqlLoggerFactory.AssertBaseline(
                new[]
                {
                    @"@p1='78'
@p0='New Category' (Size = 4000)

SET NOCOUNT ON;
UPDATE [Categories] SET [Name] = @p0
WHERE [Id] = @p1;
SELECT @@ROWCOUNT;"
                }, assertOrder: false);
        }

        public override void Identifiers_are_generated_correctly()
        {
            using (var context = CreateContext())
            {
                var entityType = context.Model.FindEntityType(
                    typeof(
                        LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly));
                Assert.Equal("LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorking~", entityType.Relational().TableName);
                Assert.Equal("PK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~", entityType.GetKeys().Single().Relational().Name);
                Assert.Equal("FK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~", entityType.GetForeignKeys().Single().Relational().Name);
                Assert.Equal("IX_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~", entityType.GetIndexes().Single().Relational().Name);
            }
        }
    }
}
