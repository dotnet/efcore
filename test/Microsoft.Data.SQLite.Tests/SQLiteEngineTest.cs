// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Xunit;

namespace Microsoft.Data.SQLite
{
    public class SQLiteEngineTest
    {
        [Fact]
        public void DeleteDatabase_works()
        {
            var filename = Path.GetTempFileName();
            try
            {
                SQLiteEngine.DeleteDatabase(filename);

                Assert.False(File.Exists(filename));
            }
            finally
            {
                File.Delete(filename);
            }
        }
    }
}
