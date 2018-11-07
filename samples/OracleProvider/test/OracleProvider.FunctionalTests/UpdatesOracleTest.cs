// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class UpdatesOracleTest : UpdatesRelationalTestBase<UpdatesOracleFixture>
    {
        public UpdatesOracleTest(UpdatesOracleFixture fixture)
            : base(fixture)
        {
        }

#if !Test21
        [Fact(Skip = "Issue #13029")]
        public override void Update_on_bytes_concurrency_token_original_value_matches_does_not_throw()
        {
            base.Update_on_bytes_concurrency_token_original_value_matches_does_not_throw();
        }

        [Fact(Skip = "Issue #13029")]
        public override void Remove_on_bytes_concurrency_token_original_value_matches_does_not_throw()
        {
            base.Remove_on_bytes_concurrency_token_original_value_matches_does_not_throw();
        }
#endif

        [Fact(Skip = "Issue #13029")]
        public override void Update_on_bytes_concurrency_token_original_value_mismatch_throws()
        {
            base.Update_on_bytes_concurrency_token_original_value_mismatch_throws();
        }

        [Fact(Skip = "Issue #13029")]
        public override void Remove_on_bytes_concurrency_token_original_value_mismatch_throws()
        {
            base.Remove_on_bytes_concurrency_token_original_value_mismatch_throws();
        }

        public override void Identifiers_are_generated_correctly()
        {
            using (var context = CreateContext())
            {
                var entityType = context.Model.FindEntityType(typeof(
                    LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly));
                Assert.Equal("LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorking~", entityType.Relational().TableName);
                Assert.Equal("PK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~", entityType.GetKeys().Single().Relational().Name);
                Assert.Equal("FK_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~", entityType.GetForeignKeys().Single().Relational().Name);
                Assert.Equal("IX_LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWork~", entityType.GetIndexes().Single().Relational().Name);
            }
        }
    }
}
