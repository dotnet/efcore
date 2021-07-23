// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class UpdatesSqliteTest : UpdatesRelationalTestBase<UpdatesSqliteFixture>
    {
        public UpdatesSqliteTest(UpdatesSqliteFixture fixture)
            : base(fixture)
        {
        }

        public override void Identifiers_are_generated_correctly()
        {
            using var context = CreateContext();
            var entityType = context.Model.FindEntityType(
                typeof(
                    LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly
                ));
            Assert.Equal(
                "LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly",
                entityType.GetTableName());
            Assert.Equal(
                "PK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly",
                entityType.GetKeys().Single().GetName());
            Assert.Equal(
                "FK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly_Profile_ProfileId_ProfileId1_ProfileId3_ProfileId4_ProfileId5_ProfileId6_ProfileId7_ProfileId8_ProfileId9_ProfileId10_ProfileId11_ProfileId12_ProfileId13_ProfileId14",
                entityType.GetForeignKeys().Single().GetConstraintName());
            Assert.Equal(
                "IX_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly_ProfileId_ProfileId1_ProfileId3_ProfileId4_ProfileId5_ProfileId6_ProfileId7_ProfileId8_ProfileId9_ProfileId10_ProfileId11_ProfileId12_ProfileId13_ProfileId14_ExtraProperty",
                entityType.GetIndexes().Single().GetDatabaseName());
        }
    }
}
