// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Data.Relational.Update
{
    public class ModificationCommandTest
    {
        [Fact]
        public void ModificationCommand_initialized_correctly()
        {
            var columnValues = new[] { new KeyValuePair<string, object>("Col1", new object()) };
            var whereClauses = new[] { new KeyValuePair<string, object>("Col2", new object()) };

            var modificationCommand = new ModificationCommand("T1", columnValues, whereClauses);
            Assert.Equal("T1", modificationCommand.TableName);
            Assert.Equal(columnValues, modificationCommand.ColumnValues);
            Assert.Equal(whereClauses, modificationCommand.WhereClauses);
        }

        [Fact]
        public void ModificationOperation_set_correctly_based_on_parameters()
        {
            var valuePairs = new KeyValuePair<string, object>[0];

            Assert.Equal(
                ModificationOperation.Insert, 
                new ModificationCommand("T1", columnValues: valuePairs, whereClauses: null).Operation);

            Assert.Equal(
                ModificationOperation.Update,
                    new ModificationCommand("T1", columnValues: valuePairs, whereClauses: valuePairs).Operation);

            Assert.Equal(
                ModificationOperation.Delete,
                    new ModificationCommand("T1", columnValues: null, whereClauses: valuePairs).Operation);
        }
    }
}
