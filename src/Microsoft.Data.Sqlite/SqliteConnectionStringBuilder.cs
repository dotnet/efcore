// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Data.Sqlite.Interop;
using Microsoft.Data.Sqlite.Utilities;

namespace Microsoft.Data.Sqlite
{
    public class SqliteConnectionStringBuilder : DbConnectionStringBuilder
    {
        // NOTE: Order must match the Keywords enum
        private static readonly string[] _validKeywords = new[]
            {
                "Cache",
                "Filename",
                "Mode",
                "Mutex",
                "Uri",
                "VFS"
            };

        private static readonly IDictionary<string, Keywords> _keywords = new Dictionary<string, Keywords>(
            7,
            StringComparer.OrdinalIgnoreCase)
            {
                { "Cache", Keywords.Cache },
                { "Data Source", Keywords.Filename },
                { "Filename", Keywords.Filename },
                { "Mode", Keywords.Mode },
                { "Mutex", Keywords.Mutex },
                { "Uri", Keywords.Uri },
                { "VFS", Keywords.VirtualFileSystem }
            };

        private string _cache;
        private string _filename;
        private string _mode = "RWC";
        private string _mutex;
        private bool _uri;
        private string _virtualFileSystem;

        public SqliteConnectionStringBuilder()
        {
        }

        public SqliteConnectionStringBuilder(string connectionString)
        {
            Check.NotEmpty(connectionString, "connectionString");

            ConnectionString = connectionString;
        }

        public string Cache
        {
            get { return _cache; }
            set
            {
                if (string.Equals(value, "Private", StringComparison.OrdinalIgnoreCase))
                {
                    value = "Private";
                }
                else if (string.Equals(value, "Shared", StringComparison.OrdinalIgnoreCase))
                {
                    value = "Shared";
                }
                else if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                else
                {
                    throw new ArgumentException(Strings.FormatInvalidConnectionOptionValue("Cache", value));
                }

                base["Cache"] = value;
                _cache = value;
            }
        }

        public string Filename
        {
            get { return _filename; }
            set
            {
                base["Filename"] = value;
                _filename = value;
            }
        }

        public string Mode
        {
            get { return _mode; }
            set
            {
                if (string.Equals(value, "RO", StringComparison.OrdinalIgnoreCase))
                {
                    value = "RO";
                }
                else if (string.Equals(value, "RW", StringComparison.OrdinalIgnoreCase))
                {
                    value = "RW";
                }
                else if (string.IsNullOrEmpty(value)
                         || value.Equals("RWC", StringComparison.OrdinalIgnoreCase))
                {
                    value = "RWC";
                }
                else
                {
                    throw new ArgumentException(Strings.FormatInvalidConnectionOptionValue("Mode", value));
                }

                base["Mode"] = value;
                _mode = value;
            }
        }

        public string Mutex
        {
            get { return _mutex; }
            set
            {
                if (string.Equals(value, "None", StringComparison.OrdinalIgnoreCase))
                {
                    value = "None";
                }
                else if (string.Equals(value, "Full", StringComparison.OrdinalIgnoreCase))
                {
                    value = "Full";
                }
                else if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                else
                {
                    throw new ArgumentException(Strings.FormatInvalidConnectionOptionValue("Mutex", value));
                }

                base["Mutex"] = value;
                _mutex = value;
            }
        }

        public bool Uri
        {
            get { return _uri; }
            set
            {
                base["Uri"] = value.ToString();
                _uri = value;
            }
        }

        public string VirtualFileSystem
        {
            get { return _virtualFileSystem; }
            set
            {
                base["VFS"] = value;
                _virtualFileSystem = value;
            }
        }

        public override ICollection Keys
        {
            get { return new ReadOnlyCollection<string>(_validKeywords); }
        }

        public override ICollection Values
        {
            get
            {
                var values = new object[_validKeywords.Length];
                for (var i = 0; i < _validKeywords.Length; i++)
                {
                    values[i] = GetAt((Keywords)i);
                }

                return new ReadOnlyCollection<object>(values);
            }
        }

        public override object this[string keyword]
        {
            get
            {
                Check.NotEmpty(keyword, "keyword");

                return GetAt(GetIndex(keyword));
            }
            set
            {
                Check.NotEmpty(keyword, "keyword");

                if (value == null)
                {
                    Remove(keyword);

                    return;
                }

                switch (GetIndex(keyword))
                {
                    case Keywords.Cache:
                        Cache = Convert.ToString(value, CultureInfo.InvariantCulture);
                        return;

                    case Keywords.Filename:
                        Filename = Convert.ToString(value, CultureInfo.InvariantCulture);
                        return;

                    case Keywords.Mode:
                        Mode = Convert.ToString(value, CultureInfo.InvariantCulture);
                        return;

                    case Keywords.Mutex:
                        Mutex = Convert.ToString(value, CultureInfo.InvariantCulture);
                        return;

                    case Keywords.Uri:
                        Uri = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                        return;

                    case Keywords.VirtualFileSystem:
                        VirtualFileSystem = Convert.ToString(value, CultureInfo.InvariantCulture);
                        return;

                    default:
                        Debug.Assert(false, "Unexpected keyword");
                        return;
                }
            }
        }

        internal int GetFlags()
        {
            var flags = 0;

            switch (Mode)
            {
                case "RO":
                    flags |= Constants.SQLITE_OPEN_READONLY;
                    break;

                case "RW":
                    flags |= Constants.SQLITE_OPEN_READWRITE;
                    break;

                default:
                    Debug.Assert(Mode == "RWC", "Mode is not RWC.");
                    flags |= Constants.SQLITE_OPEN_READWRITE | Constants.SQLITE_OPEN_CREATE;
                    break;
            }

            switch (Mutex)
            {
                case "None":
                    flags |= Constants.SQLITE_OPEN_NOMUTEX;
                    break;

                case "Full":
                    flags |= Constants.SQLITE_OPEN_FULLMUTEX;
                    break;

                default:
                    Debug.Assert(Mutex == null, "Mutex is not null.");
                    break;
            }

            if (Uri)
            {
                flags |= Constants.SQLITE_OPEN_URI;
            }

            return flags;
        }

        public override void Clear()
        {
            base.Clear();

            for (var i = 0; i < _validKeywords.Length; i++)
            {
                Reset((Keywords)i);
            }
        }

        public override bool ContainsKey(string keyword)
        {
            Check.NotEmpty(keyword, "keyword");

            return _keywords.ContainsKey(keyword);
        }

        public override bool Remove(string keyword)
        {
            Check.NotEmpty(keyword, "keyword");

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
            Check.NotEmpty(keyword, "keyword");

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
                case Keywords.Cache:
                    return Cache;

                case Keywords.Filename:
                    return Filename;

                case Keywords.Mode:
                    return Mode;

                case Keywords.Mutex:
                    return Mutex;

                case Keywords.Uri:
                    return Uri;

                case Keywords.VirtualFileSystem:
                    return VirtualFileSystem;

                default:
                    Debug.Assert(false, "Unexpected keyword.");
                    return null;
            }
        }

        private Keywords GetIndex(string keyword)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(keyword), "keyword is null or empty.");

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
                case Keywords.Cache:
                    _cache = null;
                    return;

                case Keywords.Filename:
                    _filename = null;
                    return;

                case Keywords.Mode:
                    _mode = "RWC";
                    return;

                case Keywords.Mutex:
                    _mutex = null;
                    return;

                case Keywords.Uri:
                    _uri = false;
                    return;

                case Keywords.VirtualFileSystem:
                    _virtualFileSystem = null;
                    return;

                default:
                    Debug.Assert(false, "Unexpected keyword.");
                    return;
            }
        }

        // NOTE: Values must match _validKeywords field order
        private enum Keywords
        {
            Cache = 0,
            Filename = 1,
            Mode = 2,
            Mutex = 3,
            Uri = 4,
            VirtualFileSystem = 5
        }
    }
}
