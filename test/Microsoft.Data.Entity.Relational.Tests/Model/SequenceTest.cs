// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using Microsoft.Data.Entity.Relational.Model;
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
    }
}
