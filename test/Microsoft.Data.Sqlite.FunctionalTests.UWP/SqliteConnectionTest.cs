// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if WINDOWS_UWP

using System.IO;
using Windows.Storage;
using Xunit;

namespace Microsoft.Data.Sqlite.Tests
{
    public class SqliteConnectionTest
    {
        [Theory]
        [InlineData("./local.db")]
        [InlineData(".\\local.db")]
        [InlineData("local.db")]
        public void Opens_relative_path_in_local_folder(string relativePath)
        {
            var expectedPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "local.db");
            var connectionString = "Filename=" + relativePath;

            AssertConnectionOpens(connectionString, expectedPath);
        }

        [Fact]
        public void Open_fails_with_file_uri()
        {
            Assert.Throws<SqliteException>(() => new SqliteConnection("Filename=file:test.db").Open());
        }

        [Fact]
        public void Opens_absolute_paths()
        {
            var expectedPath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "roaming.db");
            var connectionString = "Filename=" + expectedPath;

            AssertConnectionOpens(connectionString, expectedPath);
        }

        private static void AssertConnectionOpens(string connectionString, string expectedPath)
        {
            if (File.Exists(expectedPath))
            {
                File.Delete(expectedPath);
            }

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                Assert.Equal(expectedPath, connection.DataSource);
            }

            Assert.True(File.Exists(expectedPath));
        }
    }
}

#endif
