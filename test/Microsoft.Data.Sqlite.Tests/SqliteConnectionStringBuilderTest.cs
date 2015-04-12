// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Data.Sqlite
{
    public class SqliteConnectionStringBuilderTest
    {
        [Fact]
        public void Ctor_parses_options()
        {
            var builder = new SqliteConnectionStringBuilder("Data Source=test.db");

            Assert.Equal("test.db", builder.DataSource);
        }

        [Fact]
        public void DataSource_works()
        {
            var builder = new SqliteConnectionStringBuilder();

            builder.DataSource = "test.db";

            Assert.Equal("test.db", builder.DataSource);
        }

        [Fact]
        public void DataSource_defaults_to_empty()
        {
            Assert.Empty(new SqliteConnectionStringBuilder().DataSource);
        }

        [Fact]
        public void Keys_works()
        {
            var keys = (ICollection<string>)new SqliteConnectionStringBuilder().Keys;

            Assert.True(keys.IsReadOnly);
            Assert.Equal(1, keys.Count);
            Assert.Contains("Data Source", keys);
        }

        [Fact]
        public void Values_works()
        {
            var values = (ICollection<object>)new SqliteConnectionStringBuilder().Values;

            Assert.True(values.IsReadOnly);
            Assert.Equal(1, values.Count);
        }

        [Fact]
        public void Item_validates_argument()
        {
            var ex = Assert.Throws<ArgumentException>(() => new SqliteConnectionStringBuilder()["Invalid"]);
            Assert.Equal(Strings.FormatKeywordNotSupported("Invalid"), ex.Message);

            ex = Assert.Throws<ArgumentException>(() => new SqliteConnectionStringBuilder()["Invalid"] = 0);
            Assert.Equal(Strings.FormatKeywordNotSupported("Invalid"), ex.Message);
        }

        [Fact]
        public void Item_resets_value_when_null()
        {
            var builder = new SqliteConnectionStringBuilder();
            builder.DataSource = "test.db";

            builder["Data Source"] = null;

            Assert.Empty(builder.DataSource);
        }

        [Fact]
        public void Item_gets_value()
        {
            var builder = new SqliteConnectionStringBuilder();
            builder.DataSource = "test.db";

            Assert.Equal("test.db", builder["Data Source"]);
        }

        [Fact]
        public void Item_sets_value()
        {
            var builder = new SqliteConnectionStringBuilder();

            builder["Data Source"] = "test.db";

            Assert.Equal("test.db", builder.DataSource);
        }

        [Fact]
        public void Clear_resets_everything()
        {
            var builder = new SqliteConnectionStringBuilder("Data Source=test.db");

            builder.Clear();

            Assert.Empty(builder.DataSource);
        }

        [Fact]
        public void ContainsKey_returns_true_when_exists()
        {
            Assert.True(new SqliteConnectionStringBuilder().ContainsKey("Data Source"));
        }

        [Fact]
        public void ContainsKey_returns_false_when_not_exists()
        {
            Assert.False(new SqliteConnectionStringBuilder().ContainsKey("Invalid"));
        }

        [Fact]
        public void Remove_returns_false_when_not_exists()
        {
            Assert.False(new SqliteConnectionStringBuilder().Remove("Invalid"));
        }

        [Fact]
        public void Remove_resets_option()
        {
            var builder = new SqliteConnectionStringBuilder("Data Source=test.db");

            var removed = builder.Remove("Data Source");

            Assert.True(removed);
            Assert.Empty(builder.DataSource);
        }

        [Fact]
        public void ShouldSerialize_returns_false_when_not_exists()
        {
            Assert.False(new SqliteConnectionStringBuilder().ShouldSerialize("Invalid"));
        }

        [Fact]
        public void ShouldSerialize_returns_false_when_unset()
        {
            Assert.False(new SqliteConnectionStringBuilder().ShouldSerialize("Data Source"));
        }

        [Fact]
        public void ShouldSerialize_returns_true_when_set()
        {
            var builder = new SqliteConnectionStringBuilder("Data Source=test.db");

            Assert.True(builder.ShouldSerialize("Data Source"));
        }

        [Fact]
        public void TryGetValue_returns_false_when_not_exists()
        {
            object value;
            var retrieved = new SqliteConnectionStringBuilder().TryGetValue("Invalid", out value);

            Assert.False(retrieved);
            Assert.Null(value);
        }

        [Fact]
        public void TryGetValue_returns_true_when_exists()
        {
            var builder = new SqliteConnectionStringBuilder("Data Source=test.db");

            object value;
            var retrieved = builder.TryGetValue("Data Source", out value);

            Assert.True(retrieved);
            Assert.Equal("test.db", value);
        }
    }
}
