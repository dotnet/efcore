// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    /// Provides a simple way to create and manage the contents of connection strings used by
    /// <see cref="SqliteConnection" />.
    /// </summary>
    public class SqliteConnectionStringBuilder : DbConnectionStringBuilder
    {
        private const string DataSourceKeyword = "Data Source";
        private const string DataSourceNoSpaceKeyword = "DataSource";
        private const string ModeKeyword = "Mode";
        private const string CacheKeyword = "Cache";
        private const string FilenameKeyword = "Filename";

        private enum Keywords
        {
            DataSource,
            Mode,
            Cache
        }

        private static readonly IReadOnlyList<string> _validKeywords;
        private static readonly IReadOnlyDictionary<string, Keywords> _keywords;

        private string _dataSource = string.Empty;
        private SqliteOpenMode _mode = SqliteOpenMode.ReadWriteCreate;
        private SqliteCacheMode _cache = SqliteCacheMode.Default;

        static SqliteConnectionStringBuilder()
        {
            var validKeywords = new string[3];
            validKeywords[(int)Keywords.DataSource] = DataSourceKeyword;
            validKeywords[(int)Keywords.Mode] = ModeKeyword;
            validKeywords[(int)Keywords.Cache] = CacheKeyword;
            _validKeywords = validKeywords;

            _keywords = new Dictionary<string, Keywords>(3, StringComparer.OrdinalIgnoreCase)
            {
                [DataSourceKeyword] = Keywords.DataSource,
                [ModeKeyword] = Keywords.Mode,
                [CacheKeyword] = Keywords.Cache,

                // aliases
                [FilenameKeyword] = Keywords.DataSource,
                [DataSourceNoSpaceKeyword] = Keywords.DataSource
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteConnectionStringBuilder" /> class.
        /// </summary>
        public SqliteConnectionStringBuilder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteConnectionStringBuilder" /> class.
        /// </summary>
        /// <param name="connectionString">
        /// The initial connection string the builder will represent. Can be null.
        /// </param>
        public SqliteConnectionStringBuilder(string connectionString)
        {
            ConnectionString = connectionString;
        }

        /// <summary>
        /// Gets or sets the database file.
        /// </summary>
        public virtual string DataSource
        {
            get { return _dataSource; }
            set { base[DataSourceKeyword] = _dataSource = value; }
        }

        /// <summary>
        /// Gets or sets the connection mode.
        /// </summary>
        public virtual SqliteOpenMode Mode
        {
            get { return _mode; }
            set { base[ModeKeyword] = _mode = value; }
        }

        /// <summary>
        /// Gets a collection containing the keys used by the connection string.
        /// </summary>
        public override ICollection Keys
            => new ReadOnlyCollection<string>((string[])_validKeywords);

        /// <summary>
        /// Gets a collection containing the values used by the connection string.
        /// </summary>
        public override ICollection Values
        {
            get
            {
                var values = new object[_validKeywords.Count];
                for (var i = 0; i < _validKeywords.Count; i++)
                {
                    values[i] = GetAt((Keywords)i);
                }

                return new ReadOnlyCollection<object>(values);
            }
        }

        /// <summary>
        /// Gets or sets the caching mode used by the connection.
        /// </summary>
        /// <seealso href="http://sqlite.org/sharedcache.html">SQLite Shared-Cache Mode</seealso>
        public virtual SqliteCacheMode Cache
        {
            get { return _cache; }
            set { base[CacheKeyword] = _cache = value; }
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="keyword">The key.</param>
        /// <returns>The value.</returns>
        public override object this[string keyword]
        {
            get { return GetAt(GetIndex(keyword)); }
            set
            {
                if (value == null)
                {
                    Remove(keyword);

                    return;
                }

                switch (GetIndex(keyword))
                {
                    case Keywords.DataSource:
                        DataSource = Convert.ToString(value, CultureInfo.InvariantCulture);
                        return;

                    case Keywords.Mode:
                        Mode = ConvertToEnum<SqliteOpenMode>(keyword, value);
                        return;

                    case Keywords.Cache:
                        Cache = ConvertToEnum<SqliteCacheMode>(keyword, value);
                        return;

                    default:
                        Debug.Assert(false, "Unexpected keyword: " + keyword);
                        return;
                }
            }
        }

        private TEnum ConvertToEnum<TEnum>(string keyword, object value)
            where TEnum : struct
        {
            var stringValue = value as string;
            if (stringValue != null)
            {
                return (TEnum)Enum.Parse(typeof(TEnum), stringValue, ignoreCase: true);
            }

            TEnum enumValue;
            if (value is TEnum)
            {
                enumValue = (TEnum)value;
            }
            else if (value.GetType().GetTypeInfo().IsEnum)
            {
                throw new ArgumentException(Strings.ConvertFailed(value.GetType(), typeof(TEnum)));
            }
            else
            {
                enumValue = (TEnum)Enum.ToObject(typeof(TEnum), value);
            }

            if (!Enum.IsDefined(typeof(TEnum), enumValue))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    Strings.InvalidEnumValue(typeof(TEnum), enumValue));
            }

            return enumValue;
        }

        /// <summary>
        /// Clears the contents of the builder.
        /// </summary>
        public override void Clear()
        {
            base.Clear();

            for (var i = 0; i < _validKeywords.Count; i++)
            {
                Reset((Keywords)i);
            }
        }

        /// <summary>
        /// Determines whether the specified key is used by the connection string.
        /// </summary>
        /// <param name="keyword">The key to look for.</param>
        /// <returns>true if it is use; otherwise, false.</returns>
        public override bool ContainsKey(string keyword)
            => _keywords.ContainsKey(keyword);

        /// <summary>
        /// Removes the specified key and its value from the connection string.
        /// </summary>
        /// <param name="keyword">The key to remove.</param>
        /// <returns>true if the key was used; otherwise, false.</returns>
        public override bool Remove(string keyword)
        {
            Keywords index;
            if (!_keywords.TryGetValue(keyword, out index)
                || !base.Remove(_validKeywords[(int)index]))
            {
                return false;
            }

            Reset(index);

            return true;
        }

        /// <summary>
        /// Determines whether the specified key should be serialized into the connection string.
        /// </summary>
        /// <param name="keyword">The key to check.</param>
        /// <returns>true if it should be serialized; otherwise, false.</returns>
        public override bool ShouldSerialize(string keyword)
        {
            Keywords index;
            return _keywords.TryGetValue(keyword, out index) && base.ShouldSerialize(_validKeywords[(int)index]);
        }

        /// <summary>
        /// Gets the value of the specified key if it is used.
        /// </summary>
        /// <param name="keyword">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if the key was used; otherwise, false.</returns>
        public override bool TryGetValue(string keyword, out object value)
        {
            Keywords index;
            if (!_keywords.TryGetValue(keyword, out index))
            {
                value = null;

                return false;
            }

            value = GetAt(index);

            return true;
        }

        private object GetAt(Keywords index)
        {
            switch (index)
            {
                case Keywords.DataSource:
                    return DataSource;

                case Keywords.Mode:
                    return Mode;

                case Keywords.Cache:
                    return Cache;

                default:
                    Debug.Fail("Unexpected keyword: " + index);
                    return null;
            }
        }

        private Keywords GetIndex(string keyword)
        {
            Keywords index;
            if (!_keywords.TryGetValue(keyword, out index))
            {
                throw new ArgumentException(Strings.KeywordNotSupported(keyword));
            }

            return index;
        }

        private void Reset(Keywords index)
        {
            switch (index)
            {
                case Keywords.DataSource:
                    _dataSource = string.Empty;
                    return;

                case Keywords.Mode:
                    _mode = SqliteOpenMode.ReadWriteCreate;
                    return;

                case Keywords.Cache:
                    _cache = SqliteCacheMode.Default;
                    return;

                default:
                    Debug.Fail("Unexpected keyword: " + index);
                    return;
            }
        }
    }
}
