// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Data.Sqlite
{
    public class SqliteConnectionStringBuilder : DbConnectionStringBuilder
    {
        private const string DataSourceKeyword = "Data Source";
        private const string CacheKeyword = "Cache";

        private enum Keywords
        {
            DataSource,
            Cache
        }

        private static readonly IReadOnlyList<string> _validKeywords;
        private static readonly IReadOnlyDictionary<string, Keywords> _keywords;

        private string _dataSource = string.Empty;
        private CacheMode _cacheMode = CacheMode.Private;

        static SqliteConnectionStringBuilder()
        {
            var validKeywords = new string[2];
            validKeywords[(int)Keywords.DataSource] = DataSourceKeyword;
            validKeywords[(int)Keywords.Cache] = CacheKeyword;
            _validKeywords = validKeywords;

            _keywords = new Dictionary<string, Keywords>(2, StringComparer.OrdinalIgnoreCase)
                {
                    [DataSourceKeyword] = Keywords.DataSource,
                    [CacheKeyword] = Keywords.Cache
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

        public CacheMode CacheMode
        {
            get { return _cacheMode; }
            set { base[CacheKeyword] = _cacheMode = value; }
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

                    case Keywords.Cache:
                        CacheMode mode;
                        if (!Enum.TryParse(value as string, out mode))
                        {
                            throw new ArgumentException(Strings.FormatInvalidCacheMode(value));
                        }
                        CacheMode = mode;
                        return;

                    default:
                        Debug.Fail("Unexpected keyword: " + keyword);
                        return;
                }
            }
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
            if (!_keywords.TryGetValue(keyword, out index))
            {
                return false;
            }

            return base.ShouldSerialize(_validKeywords[(int)index]);
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

                case Keywords.Cache:
                    return CacheMode;

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

                case Keywords.Cache:
                    _cacheMode = CacheMode.Private;
                    return;

#if NET45 || DNX451 || DNXCORE50
                default:
                    Debug.Fail("Unexpected keyword: " + index);
                    return;
#endif
            }
        }
    }
}
