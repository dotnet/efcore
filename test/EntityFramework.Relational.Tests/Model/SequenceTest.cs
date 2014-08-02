// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Relational.Model;
using Microsoft.Data.Entity.Relational.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Model
{
    public class SequenceTest
    {
        [Fact]
        public void Create_and_initialize_sequence()
        {
            var sequence = new Sequence("dbo.MySequence");

            Assert.Equal("dbo.MySequence", sequence.Name);
            Assert.Equal("BIGINT", sequence.DataType);
            Assert.Equal(0, sequence.StartWith);
            Assert.Equal(1, sequence.IncrementBy);

            var sequence2 = new Sequence("dbo.MySequence2", "int", 5, 2);

            Assert.Equal("dbo.MySequence2", sequence2.Name);
            Assert.Equal("int", sequence2.DataType);
            Assert.Equal(5, sequence2.StartWith);
            Assert.Equal(2, sequence2.IncrementBy);
        }

        [Fact]
        public void Clone_replicates_instance()
        {
            var sequence = new Sequence("dbo.MySequence", "int", 5, 2);
            var clone = sequence.Clone(new CloneContext());

            Assert.NotSame(sequence, clone);
            Assert.Equal("dbo.MySequence", clone.Name);
            Assert.Equal("int", clone.DataType);
            Assert.Equal(5, clone.StartWith);
            Assert.Equal(2, clone.IncrementBy);
        }
    }
}
