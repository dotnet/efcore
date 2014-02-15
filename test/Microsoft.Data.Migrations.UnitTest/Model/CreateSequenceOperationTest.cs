// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Migrations.Model
{
    public class CreateSequenceOperationTest
    {
        [Fact]
        public void CreateOperationAndSetProperties()
        {
            var createSequenceOperation
                = new CreateSequenceOperation("foo.bar")
                {
                    StartWith = 42,
                    IncrementBy = 44,
                    DataType = "smallint"
                };

            Assert.Equal<string>("foo.bar", createSequenceOperation.SchemaQualifiedName);
            Assert.Equal(42, createSequenceOperation.StartWith);
            Assert.Equal(44, createSequenceOperation.IncrementBy);
            Assert.Equal("smallint", createSequenceOperation.DataType);
        }
    }
}
