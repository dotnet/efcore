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
    /// Provides a simple way to create and manage the contents of connection strings used by <see cref="SqliteConnection"/>.
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

        public SqliteConnectionStringBuilder()
        {
        }

        public SqliteConnectionStringBuilder(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public string DataSource
        {
            get { return _dataSource; }
            set { base[DataSourceKeyword] = _dataSource = value; }
        }

        public SqliteOpenMode Mode
        {
            get { return _mode; }
            set { base[ModeKeyword] = _mode = value; }
        }

        public override ICollection Keys => new ReadOnlyCollection<string>((string[])_validKeywords);

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

        public SqliteCacheMode Cache
        {
            get { return _cache; }
            set { base[CacheKeyword] = _cache = value; }
        }

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
                throw new ArgumentException(Strings.FormatConvertFailed(value.GetType(), typeof(TEnum)));
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
                    Strings.FormatInvalidEnumValue(typeof(TEnum), enumValue));
            }

            return enumValue;
        }

        public override void Clear()
        {
            base.Clear();

            for (var i = 0; i < _validKeywords.Count; i++)
            {
                Reset((Keywords)i);
            }
        }

        public override bool ContainsKey(string keyword) => _keywords.ContainsKey(keyword);

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

        public override bool ShouldSerialize(string keyword)
        {
            Keywords index;
            return _keywords.TryGetValue(keyword, out index) && base.ShouldSerialize(_validKeywords[(int)index]);
        }

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
                throw new ArgumentException(Strings.FormatKeywordNotSupported(keyword));
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
