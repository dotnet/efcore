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
            var sequence = new Sequence("dbo.MySequence2", "int", 5, 2);

            Assert.Equal("dbo.MySequence2", sequence.Name);
            Assert.Equal("int", sequence.DataType);
            Assert.Equal(5, sequence.StartWith);
            Assert.Equal(2, sequence.IncrementBy);
        }

        [Fact]
        public void Can_set_name()
        {
            var sequence = new Sequence("dbo.Sequence", "bigint", 0, 1);

            Assert.Equal("dbo.Sequence", sequence.Name);

            sequence.Name = "dbo.RenamedSequence";

            Assert.Equal("dbo.RenamedSequence", sequence.Name);
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
