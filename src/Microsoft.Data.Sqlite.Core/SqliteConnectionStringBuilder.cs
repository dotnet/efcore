// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.Data.Sqlite.Properties;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    ///     Provides a simple way to create and manage the contents of connection strings used by
    ///     <see cref="SqliteConnection" />.
    /// </summary>
    /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/connection-strings">Connection Strings</seealso>
    public class SqliteConnectionStringBuilder : DbConnectionStringBuilder
    {
        private const string DataSourceKeyword = "Data Source";
        private const string DataSourceNoSpaceKeyword = "DataSource";
        private const string ModeKeyword = "Mode";
        private const string CacheKeyword = "Cache";
        private const string FilenameKeyword = "Filename";
        private const string PasswordKeyword = "Password";
        private const string ForeignKeysKeyword = "Foreign Keys";
        private const string RecursiveTriggersKeyword = "Recursive Triggers";
        private const string DefaultTimeoutKeyword = "Default Timeout";
        private const string CommandTimeoutKeyword = "Command Timeout";
        private const string PoolingKeyword = "Pooling";

        private enum Keywords
        {
            DataSource,
            Mode,
            Cache,
            Password,
            ForeignKeys,
            RecursiveTriggers,
            DefaultTimeout,
            Pooling
        }

        private static readonly IReadOnlyList<string> _validKeywords;
        private static readonly IReadOnlyDictionary<string, Keywords> _keywords;

        private string _dataSource = string.Empty;
        private SqliteOpenMode _mode = SqliteOpenMode.ReadWriteCreate;
        private SqliteCacheMode _cache = SqliteCacheMode.Default;
        private string _password = string.Empty;
        private bool? _foreignKeys;
        private bool _recursiveTriggers;
        private int _defaultTimeout = 30;
        private bool _pooling = true;

        static SqliteConnectionStringBuilder()
        {
            var validKeywords = new string[8];
            validKeywords[(int)Keywords.DataSource] = DataSourceKeyword;
            validKeywords[(int)Keywords.Mode] = ModeKeyword;
            validKeywords[(int)Keywords.Cache] = CacheKeyword;
            validKeywords[(int)Keywords.Password] = PasswordKeyword;
            validKeywords[(int)Keywords.ForeignKeys] = ForeignKeysKeyword;
            validKeywords[(int)Keywords.RecursiveTriggers] = RecursiveTriggersKeyword;
            validKeywords[(int)Keywords.DefaultTimeout] = DefaultTimeoutKeyword;
            validKeywords[(int)Keywords.Pooling] = PoolingKeyword;
            _validKeywords = validKeywords;

            _keywords = new Dictionary<string, Keywords>(11, StringComparer.OrdinalIgnoreCase)
            {
                [DataSourceKeyword] = Keywords.DataSource,
                [ModeKeyword] = Keywords.Mode,
                [CacheKeyword] = Keywords.Cache,
                [PasswordKeyword] = Keywords.Password,
                [ForeignKeysKeyword] = Keywords.ForeignKeys,
                [RecursiveTriggersKeyword] = Keywords.RecursiveTriggers,
                [DefaultTimeoutKeyword] = Keywords.DefaultTimeout,
                [PoolingKeyword] = Keywords.Pooling,

                // aliases
                [FilenameKeyword] = Keywords.DataSource,
                [DataSourceNoSpaceKeyword] = Keywords.DataSource,
                [CommandTimeoutKeyword] = Keywords.DefaultTimeout
            };
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteConnectionStringBuilder" /> class.
        /// </summary>
        public SqliteConnectionStringBuilder()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteConnectionStringBuilder" /> class.
        /// </summary>
        /// <param name="connectionString">
        ///     The initial connection string the builder will represent. Can be null.
        /// </param>
        public SqliteConnectionStringBuilder(string? connectionString)
            => ConnectionString = connectionString;

        /// <summary>
        ///     Gets or sets the database file.
        /// </summary>
        /// <value>The database file.</value>
        [AllowNull]
        public virtual string DataSource
        {
            get => _dataSource;
            set => base[DataSourceKeyword] = _dataSource = value ?? string.Empty;
        }

        /// <summary>
        ///     Gets or sets the connection mode.
        /// </summary>
        /// <value>The connection mode.</value>
        public virtual SqliteOpenMode Mode
        {
            get => _mode;
            set => base[ModeKeyword] = _mode = value;
        }

        /// <summary>
        ///     Gets a collection containing the keys used by the connection string.
        /// </summary>
        /// <value>A collection containing the keys used by the connection string.</value>
        public override ICollection Keys
            => new ReadOnlyCollection<string>((string[])_validKeywords);

        /// <summary>
        ///     Gets a collection containing the values used by the connection string.
        /// </summary>
        /// <value>A collection containing the values used by the connection string.</value>
        public override ICollection Values
        {
            get
            {
                var values = new object?[_validKeywords.Count];
                for (var i = 0; i < _validKeywords.Count; i++)
                {
                    values[i] = GetAt((Keywords)i);
                }

                return new ReadOnlyCollection<object?>(values);
            }
        }

        /// <summary>
        ///     Gets or sets the caching mode used by the connection.
        /// </summary>
        /// <value>The caching mode used by the connection.</value>
        public virtual SqliteCacheMode Cache
        {
            get => _cache;
            set => base[CacheKeyword] = _cache = value;
        }

        /// <summary>
        ///     Gets or sets the encryption key. Warning, this has no effect when the native SQLite library doesn't
        ///     support encryption. When specified, <c>PRAGMA key</c> is sent immediately after opening the connection.
        /// </summary>
        /// <value>The encryption key.</value>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/encryption">Encryption</seealso>
        [AllowNull]
        public string Password
        {
            get => _password;
            set => base[PasswordKeyword] = _password = value ?? string.Empty;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether to enable foreign key constraints. When true,
        ///     <c>PRAGMA foreign_keys = 1</c> is sent immediately after opening the connection. When false,
        ///     <c>PRAGMA foreign_keys = 0</c> is sent. When null, no pragma is sent. There is no need to enable foreign
        ///     keys if, like in e_sqlite3, SQLITE_DEFAULT_FOREIGN_KEYS was used to compile the native library.
        /// </summary>
        /// <value>A value indicating whether to enable foreign key constraints.</value>
        public bool? ForeignKeys
        {
            get => _foreignKeys;
            set => base[ForeignKeysKeyword] = _foreignKeys = value;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether to enable recursive triggers. When true,
        ///     <c>PRAGMA recursive_triggers</c> is sent immediately after opening the connection. When false, no pragma
        ///     is sent.
        /// </summary>
        /// <value>A value indicating whether to enable recursive triggers.</value>
        public bool RecursiveTriggers
        {
            get => _recursiveTriggers;
            set => base[RecursiveTriggersKeyword] = _recursiveTriggers = value;
        }

        /// <summary>
        ///     Gets or sets the default <see cref="SqliteConnection.DefaultTimeout" /> value.
        /// </summary>
        /// <value>The default <see cref="SqliteConnection.DefaultTimeout" /> value.</value>
        public int DefaultTimeout
        {
            get => _defaultTimeout;
            set => base[DefaultTimeoutKeyword] = _defaultTimeout = value;
        }

        /// <summary>
        ///     Gets or sets a value indicating whether the connection will be pooled.
        /// </summary>
        /// <value>A value indicating whether the connection will be pooled.</value>
        public bool Pooling
        {
            get => _pooling;
            set => base[PoolingKeyword] = _pooling = value;
        }

        /// <summary>
        ///     Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="keyword">The key.</param>
        /// <returns>The value.</returns>
        public override object? this[string keyword]
        {
#pragma warning disable CS8764 // NB: this["Foreign Keys"] may return null
            get => GetAt(GetIndex(keyword));
#pragma warning restore CS8764
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
                        Mode = ConvertToEnum<SqliteOpenMode>(value);
                        return;

                    case Keywords.Cache:
                        Cache = ConvertToEnum<SqliteCacheMode>(value);
                        return;

                    case Keywords.Password:
                        Password = Convert.ToString(value, CultureInfo.InvariantCulture);
                        return;

                    case Keywords.ForeignKeys:
                        ForeignKeys = ConvertToNullableBoolean(value);
                        return;

                    case Keywords.RecursiveTriggers:
                        RecursiveTriggers = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                        return;

                    case Keywords.DefaultTimeout:
                        DefaultTimeout = Convert.ToInt32(value);
                        return;

                    case Keywords.Pooling:
                        Pooling = Convert.ToBoolean(value, CultureInfo.InvariantCulture);
                        return;

                    default:
                        Debug.Fail("Unexpected keyword: " + keyword);
                        return;
                }
            }
        }

        private static TEnum ConvertToEnum<TEnum>(object value)
            where TEnum : struct
        {
            if (value is string stringValue)
            {
                return (TEnum)Enum.Parse(typeof(TEnum), stringValue, ignoreCase: true);
            }

            TEnum enumValue;
            if (value is TEnum)
            {
                enumValue = (TEnum)value;
            }
            else if (value.GetType().IsEnum)
            {
                throw new ArgumentException(Resources.ConvertFailed(value.GetType(), typeof(TEnum)));
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
                    Resources.InvalidEnumValue(typeof(TEnum), enumValue));
            }

            return enumValue;
        }

        private static bool? ConvertToNullableBoolean(object value)
            => value is null or string { Length: 0 }
                ? null
                : Convert.ToBoolean(value, CultureInfo.InvariantCulture);

        /// <summary>
        ///     Clears the contents of the builder.
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
        ///     Determines whether the specified key is used by the connection string.
        /// </summary>
        /// <param name="keyword">The key to look for.</param>
        /// <returns><see langword="true" /> if it is used; otherwise, <see langword="false" />.</returns>
        public override bool ContainsKey(string keyword)
            => _keywords.ContainsKey(keyword);

        /// <summary>
        ///     Removes the specified key and its value from the connection string.
        /// </summary>
        /// <param name="keyword">The key to remove.</param>
        /// <returns><see langword="true" /> if the key was used; otherwise, <see langword="false" />.</returns>
        public override bool Remove(string keyword)
        {
            if (!_keywords.TryGetValue(keyword, out var index)
                || !base.Remove(_validKeywords[(int)index]))
            {
                return false;
            }

            Reset(index);

            return true;
        }

        /// <summary>
        ///     Determines whether the specified key should be serialized into the connection string.
        /// </summary>
        /// <param name="keyword">The key to check.</param>
        /// <returns><see langword="true" /> if it should be serialized; otherwise, <see langword="false" />.</returns>
        public override bool ShouldSerialize(string keyword)
            => _keywords.TryGetValue(keyword, out var index) && base.ShouldSerialize(_validKeywords[(int)index]);

        /// <summary>
        ///     Gets the value of the specified key if it is used.
        /// </summary>
        /// <param name="keyword">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><see langword="true" /> if the key was used; otherwise, <see langword="false" />.</returns>
#pragma warning disable CS8765 // NB: TryGetValue("Foreign Keys", out value) returns true, but value may be null
        public override bool TryGetValue(string keyword, out object? value)
#pragma warning restore CS8765
        {
            if (!_keywords.TryGetValue(keyword, out var index))
            {
                value = null;

                return false;
            }

            value = GetAt(index);

            return true;
        }

        private object? GetAt(Keywords index)
        {
            switch (index)
            {
                case Keywords.DataSource:
                    return DataSource;

                case Keywords.Mode:
                    return Mode;

                case Keywords.Cache:
                    return Cache;

                case Keywords.Password:
                    return Password;

                case Keywords.ForeignKeys:
                    return ForeignKeys;

                case Keywords.RecursiveTriggers:
                    return RecursiveTriggers;

                case Keywords.DefaultTimeout:
                    return DefaultTimeout;

                case Keywords.Pooling:
                    return Pooling;

                default:
                    Debug.Fail("Unexpected keyword: " + index);
                    return null;
            }
        }

        private static Keywords GetIndex(string keyword)
            => !_keywords.TryGetValue(keyword, out var index)
                ? throw new ArgumentException(Resources.KeywordNotSupported(keyword))
                : index;

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

                case Keywords.Password:
                    _password = string.Empty;
                    return;

                case Keywords.ForeignKeys:
                    _foreignKeys = null;
                    return;

                case Keywords.RecursiveTriggers:
                    _recursiveTriggers = false;
                    return;

                case Keywords.DefaultTimeout:
                    _defaultTimeout = 30;
                    return;

                case Keywords.Pooling:
                    _pooling = true;
                    return;

                default:
                    Debug.Fail("Unexpected keyword: " + index);
                    return;
            }
        }
    }
}
