// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite.Interop;
using Xunit;

namespace Microsoft.Data.Sqlite
{
    public class SqliteConnectionStringBuilderTest
    {
        [Fact]
        public void Ctor_validates_argument()
        {
            var ex = Assert.Throws<ArgumentException>(() => new SqliteConnectionStringBuilder(null));

            Assert.Equal(Strings.FormatArgumentIsNullOrWhitespace("connectionString"), ex.Message);
        }

        [Fact]
        public void Cache_validates_value()
        {
            var builder = new SqliteConnectionStringBuilder();

            var ex = Assert.Throws<ArgumentException>(() => builder.Cache = "Invalid");

            Assert.Equal(Strings.FormatInvalidConnectionOptionValue("Cache", "Invalid"), ex.Message);
        }

        [Fact]
        public void Ctor_parses_options()
        {
            var builder = new SqliteConnectionStringBuilder("Data Source=test.db");

            Assert.Equal("test.db", builder.Filename);
        }

        [Fact]
        public void Cache_normalizes_values()
        {
            var builder = new SqliteConnectionStringBuilder();

            builder.Cache = "private";
            Assert.Equal("Private", builder.Cache);

            builder.Cache = "shared";
            Assert.Equal("Shared", builder.Cache);

            builder.Cache = string.Empty;
            Assert.Null(builder.Cache);
        }

        [Fact]
        public void Filename_works()
        {
            var builder = new SqliteConnectionStringBuilder();

            builder.Filename = "test.db";

            Assert.Equal("test.db", builder.Filename);
        }

        [Fact]
        public void Mode_defaults_to_RWC()
        {
            Assert.Equal("RWC", new SqliteConnectionStringBuilder().Mode);
        }

        [Fact]
        public void Mode_validates_value()
        {
            var builder = new SqliteConnectionStringBuilder();

            var ex = Assert.Throws<ArgumentException>(() => builder.Mode = "Invalid");

            Assert.Equal(Strings.FormatInvalidConnectionOptionValue("Mode", "Invalid"), ex.Message);
        }

        [Fact]
        public void Mode_normalizes_values()
        {
            var builder = new SqliteConnectionStringBuilder();

            builder.Mode = "ro";
            Assert.Equal("RO", builder.Mode);

            builder.Mode = "rw";
            Assert.Equal("RW", builder.Mode);

            builder.Mode = "rwc";
            Assert.Equal("RWC", builder.Mode);

            builder.Mode = string.Empty;
            Assert.Equal("RWC", builder.Mode);
        }

        [Fact]
        public void Mutex_validates_value()
        {
            var builder = new SqliteConnectionStringBuilder();

            var ex = Assert.Throws<ArgumentException>(() => builder.Mutex = "Invalid");

            Assert.Equal(Strings.FormatInvalidConnectionOptionValue("Mutex", "Invalid"), ex.Message);
        }

        [Fact]
        public void Mutex_normalizes_values()
        {
            var builder = new SqliteConnectionStringBuilder();

            builder.Mutex = "none";
            Assert.Equal("None", builder.Mutex);

            builder.Mutex = "full";
            Assert.Equal("Full", builder.Mutex);

            builder.Mutex = string.Empty;
            Assert.Null(builder.Mutex);
        }

        [Fact]
        public void Uri_works()
        {
            var builder = new SqliteConnectionStringBuilder();

            builder.Uri = true;

            Assert.True(builder.Uri);
        }

        [Fact]
        public void VirtualFileSystem_works()
        {
            var builder = new SqliteConnectionStringBuilder();

            builder.VirtualFileSystem = "win32";

            Assert.Equal("win32", builder.VirtualFileSystem);
        }

        [Fact]
        public void Keys_works()
        {
            var keys = (ICollection<string>)new SqliteConnectionStringBuilder().Keys;

            Assert.True(keys.IsReadOnly);
            Assert.Equal(6, keys.Count);
            Assert.Contains("Cache", keys);
            Assert.Contains("Filename", keys);
            Assert.Contains("Mode", keys);
            Assert.Contains("Mutex", keys);
            Assert.Contains("Uri", keys);
            Assert.Contains("VFS", keys);
        }

        [Fact]
        public void Values_works()
        {
            var values = (ICollection<object>)new SqliteConnectionStringBuilder().Values;

            Assert.True(values.IsReadOnly);
            Assert.Equal(6, values.Count);
        }

        [Fact]
        public void Item_validates_argument()
        {
            var ex = Assert.Throws<ArgumentException>(() => new SqliteConnectionStringBuilder()[null]);
            Assert.Equal(Strings.FormatArgumentIsNullOrWhitespace("keyword"), ex.Message);

            ex = Assert.Throws<ArgumentException>(() => new SqliteConnectionStringBuilder()[null] = 0);
            Assert.Equal(Strings.FormatArgumentIsNullOrWhitespace("keyword"), ex.Message);

            ex = Assert.Throws<ArgumentException>(() => new SqliteConnectionStringBuilder()["Invalid"]);
            Assert.Equal(Strings.FormatKeywordNotSupported("Invalid"), ex.Message);

            ex = Assert.Throws<ArgumentException>(() => new SqliteConnectionStringBuilder()["Invalid"] = 0);
            Assert.Equal(Strings.FormatKeywordNotSupported("Invalid"), ex.Message);
        }

        [Fact]
        public void Item_resets_value_when_null()
        {
            var builder = new SqliteConnectionStringBuilder();
            builder.Mode = "RO";

            builder["Mode"] = null;

            Assert.Equal("RWC", builder.Mode);
        }

        [Fact]
        public void Item_gets_value()
        {
            Assert.Equal("RWC", new SqliteConnectionStringBuilder()["Mode"]);
        }

        [Fact]
        public void Item_sets_value()
        {
            var builder = new SqliteConnectionStringBuilder();

            builder["Mode"] = "RO";

            Assert.Equal("RO", builder.Mode);
        }

        [Fact]
        public void GetFlags_calculates_mode_flags()
        {
            var builder = new SqliteConnectionStringBuilder();
            Func<int, int> filter = f => f & ~(
                Constants.SQLITE_OPEN_NOMUTEX | Constants.SQLITE_OPEN_FULLMUTEX | Constants.SQLITE_OPEN_URI);

            builder.Mode = "RO";
            Assert.Equal(Constants.SQLITE_OPEN_READONLY, filter(builder.GetFlags()));

            builder.Mode = "RW";
            Assert.Equal(Constants.SQLITE_OPEN_READWRITE, filter(builder.GetFlags()));

            builder.Mode = "RWC";
            Assert.Equal(Constants.SQLITE_OPEN_READWRITE | Constants.SQLITE_OPEN_CREATE, filter(builder.GetFlags()));
        }

        [Fact]
        public void GetFlags_calculates_mutex_flags()
        {
            var builder = new SqliteConnectionStringBuilder();
            Func<int, int> filter = f => f & ~(
                Constants.SQLITE_OPEN_READONLY | Constants.SQLITE_OPEN_READWRITE | Constants.SQLITE_OPEN_CREATE
                | Constants.SQLITE_OPEN_URI);

            builder.Mutex = "None";
            Assert.Equal(Constants.SQLITE_OPEN_NOMUTEX, filter(builder.GetFlags()));

            builder.Mutex = "Full";
            Assert.Equal(Constants.SQLITE_OPEN_FULLMUTEX, filter(builder.GetFlags()));

            builder.Mutex = null;
            Assert.Equal(0, filter(builder.GetFlags()));
        }

        [Fact]
        public void GetFlags_calculates_uri_flag()
        {
            var builder = new SqliteConnectionStringBuilder();
            Func<int, int> filter = f => f & ~(
                Constants.SQLITE_OPEN_READONLY | Constants.SQLITE_OPEN_READWRITE | Constants.SQLITE_OPEN_CREATE
                | Constants.SQLITE_OPEN_NOMUTEX | Constants.SQLITE_OPEN_FULLMUTEX);

            builder.Uri = true;
            Assert.Equal(Constants.SQLITE_OPEN_URI, filter(builder.GetFlags()));

            builder.Uri = false;
            Assert.Equal(0, filter(builder.GetFlags()));
        }

        [Fact]
        public void Clear_resets_everything()
        {
            var builder = new SqliteConnectionStringBuilder("Filename=test.db;Mode=RO");

            builder.Clear();

            Assert.Null(builder.Filename);
            Assert.Equal("RWC", builder.Mode);
        }

        [Fact]
        public void ContainsKey_validates_argument()
        {
            var ex = Assert.Throws<ArgumentException>(() => new SqliteConnectionStringBuilder().ContainsKey(null));

            Assert.Equal(Strings.FormatArgumentIsNullOrWhitespace("keyword"), ex.Message);
        }

        [Fact]
        public void ContainsKey_returns_true_when_exists()
        {
            Assert.True(new SqliteConnectionStringBuilder().ContainsKey("Filename"));
        }

        [Fact]
        public void ContainsKey_returns_false_when_not_exists()
        {
            Assert.False(new SqliteConnectionStringBuilder().ContainsKey("Invalid"));
        }

        [Fact]
        public void Remove_validates_argument()
        {
            var ex = Assert.Throws<ArgumentException>(() => new SqliteConnectionStringBuilder().Remove(null));

            Assert.Equal(Strings.FormatArgumentIsNullOrWhitespace("keyword"), ex.Message);
        }

        [Fact]
        public void Remove_returns_false_when_not_exists()
        {
            Assert.False(new SqliteConnectionStringBuilder().Remove("Invalid"));
        }

        [Fact]
        public void Remove_resets_option()
        {
            var builder = new SqliteConnectionStringBuilder("Filename=test.db");

            var removed = builder.Remove("Filename");

            Assert.True(removed);
            Assert.Null(builder.Filename);
        }

        [Fact]
        public void ShouldSerialize_validates_argument()
        {
            var ex = Assert.Throws<ArgumentException>(() => new SqliteConnectionStringBuilder().ShouldSerialize(null));

            Assert.Equal(Strings.FormatArgumentIsNullOrWhitespace("keyword"), ex.Message);
        }

        [Fact]
        public void ShouldSerialize_returns_false_when_not_exists()
        {
            Assert.False(new SqliteConnectionStringBuilder().ShouldSerialize("Invalid"));
        }

        [Fact]
        public void ShouldSerialize_returns_false_when_unset()
        {
            Assert.False(new SqliteConnectionStringBuilder().ShouldSerialize("Filename"));
        }

        [Fact]
        public void ShouldSerialize_returns_true_when_set()
        {
            var builder = new SqliteConnectionStringBuilder("Filename=test.db");

            Assert.True(builder.ShouldSerialize("Filename"));
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
            object value;

            var retrieved = new SqliteConnectionStringBuilder().TryGetValue("Mode", out value);

            Assert.True(retrieved);
            Assert.Equal("RWC", value);
        }
    }
}
