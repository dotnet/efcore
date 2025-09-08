// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure.Internal;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Infrastructure;
using Microsoft.EntityFrameworkCore.XuGu.Scaffolding.Internal;
using System.Data;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal
{
    // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
    public class XGTypeMappingSource : RelationalTypeMappingSource
    {
        // boolean
        private readonly XGBoolTypeMapping _bit1 = new XGBoolTypeMapping("bit", size: 1);
        private readonly XGBoolTypeMapping _tinyint1 = new XGBoolTypeMapping("tinyint", size: 1);

        // bit
        private readonly ULongTypeMapping _bit = new ULongTypeMapping("bit", DbType.Int64);

        // integers
        private readonly SByteTypeMapping _tinyint = new SByteTypeMapping("tinyint", DbType.SByte);
        //private readonly ByteTypeMapping _utinyint = new ByteTypeMapping("tinyint", DbType.Int16);
        private readonly ShortTypeMapping _smallint = new ShortTypeMapping("smallint", DbType.Int16);
        //private readonly UShortTypeMapping _usmallint = new UShortTypeMapping("smallint", DbType.Int32);
        private readonly IntTypeMapping _int = new IntTypeMapping("int", DbType.Int32);
        //private readonly UIntTypeMapping _uint = new UIntTypeMapping("int", DbType.Int64);
        private readonly LongTypeMapping _bigint = new LongTypeMapping("bigint", DbType.Int64);
        //private readonly ULongTypeMapping _ubigint = new ULongTypeMapping("bigint", DbType.Int64);

        private readonly BoolTypeMapping _boolean = new BoolTypeMapping("boolean", DbType.Boolean);

        // decimals
        private readonly XGDecimalTypeMapping _decimal = new XGDecimalTypeMapping("decimal", precision: 38, scale: 17);
        private readonly XGDoubleTypeMapping _double = new XGDoubleTypeMapping("double", DbType.Double);
        private readonly XGFloatTypeMapping _float = new XGFloatTypeMapping("float", DbType.Single);

        // binary
        private readonly RelationalTypeMapping _binary = new XGByteArrayTypeMapping(fixedLength: true);
        private readonly RelationalTypeMapping _varbinary = new XGByteArrayTypeMapping();

        //
        // String mappings depend on the XGOptions.NoBackslashEscapes setting:
        //

        private XGStringTypeMapping _charUnicode;
        private XGStringTypeMapping _varcharUnicode;
        private XGStringTypeMapping _tinytextUnicode;
        private XGStringTypeMapping _textUnicode;
        private XGStringTypeMapping _mediumtextUnicode;
        private XGStringTypeMapping _longtextUnicode;

        private XGStringTypeMapping _nchar;
        private XGStringTypeMapping _nvarchar;

        private XGStringTypeMapping _enum;

        private XGStringTypeMapping _varcharMax;

        // DateTime
        private readonly XGDateTypeMapping _dateDateOnly = new XGDateTypeMapping("date", typeof(DateOnly));
        private readonly XGTimeTypeMapping _timeTimeOnly = new XGTimeTypeMapping("time", typeof(TimeOnly));
        private readonly XGDateTypeMapping _dateDateTime = new XGDateTypeMapping("date", typeof(DateTime));
        private readonly XGTimeTypeMapping _timeTimeTime = new XGTimeTypeMapping("time", typeof(TimeSpan));
        private readonly XGYearTypeMapping _year = new XGYearTypeMapping("year");
        private readonly XGDateTypeMapping _date = new XGDateTypeMapping("date", typeof(DateTime));
        private readonly XGTimeSpanTypeMapping _time = new XGTimeSpanTypeMapping("time");
        private readonly XGDateTimeTypeMapping _dateTime = new XGDateTimeTypeMapping("datetime");
        private readonly XGDateTimeTypeMapping _timeStamp = new XGDateTimeTypeMapping("timestamp");
        private readonly XGDateTimeOffsetTypeMapping _dateTimeOffset = new XGDateTimeOffsetTypeMapping("DATETIME WITH TIME ZONE");
        private readonly XGDateTimeOffsetTypeMapping _timeStampOffset = new XGDateTimeOffsetTypeMapping("TIMESTAMP WITH TIME ZONE");

        private readonly RelationalTypeMapping _binaryRowVersion
            = new XGDateTimeTypeMapping(
                "timestamp",
                null,
                typeof(byte[]),
                new BytesToDateTimeConverter(),
                new ByteArrayComparer());
        private readonly RelationalTypeMapping _binaryRowVersion6
            = new XGDateTimeTypeMapping(
                "timestamp",
                6,
                typeof(byte[]),
                new BytesToDateTimeConverter(),
                new ByteArrayComparer());

        // guid
        private GuidTypeMapping _guid;

        // JSON default mapping
        private XGJsonTypeMapping<string> _jsonDefaultString;

        // Scaffolding type mappings
        private readonly XGCodeGenerationMemberAccessTypeMapping _codeGenerationMemberAccess = new XGCodeGenerationMemberAccessTypeMapping();
        private readonly XGCodeGenerationServerVersionCreationTypeMapping _codeGenerationServerVersionCreation = new XGCodeGenerationServerVersionCreationTypeMapping();

        private Dictionary<string, RelationalTypeMapping[]> _storeTypeMappings;
        private Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;
        private Dictionary<Type, RelationalTypeMapping> _scaffoldingClrTypeMappings;

        private readonly IXGOptions _options;

        private bool _initialized;
        private readonly object _initializationLock = new object();

        public XGTypeMappingSource(
            [NotNull] TypeMappingSourceDependencies dependencies,
            [NotNull] RelationalTypeMappingSourceDependencies relationalDependencies,
            [NotNull] IXGOptions options)
            : base(dependencies, relationalDependencies)
        {
            _options = options;
        }

        private void Initialize()
        {
            //
            // String mappings depend on the XGOptions.NoBackslashEscapes setting:
            //
            _varcharMax = new XGStringTypeMapping("varchar", StoreTypePostfix.Size, _options.ReplaceLineBreaksWithCharFunction);

            _charUnicode = new XGStringTypeMapping("char", StoreTypePostfix.Size, fixedLength: true, noBackslashEscapes: _options.NoBackslashEscapes, replaceLineBreaksWithCharFunction: _options.ReplaceLineBreaksWithCharFunction);
            _varcharUnicode = new XGStringTypeMapping("varchar", StoreTypePostfix.Size, noBackslashEscapes: _options.NoBackslashEscapes, replaceLineBreaksWithCharFunction: _options.ReplaceLineBreaksWithCharFunction);
            _tinytextUnicode = new XGStringTypeMapping("tinytext", StoreTypePostfix.None, noBackslashEscapes: _options.NoBackslashEscapes, replaceLineBreaksWithCharFunction: _options.ReplaceLineBreaksWithCharFunction);
            _textUnicode = new XGStringTypeMapping("text", StoreTypePostfix.None, noBackslashEscapes: _options.NoBackslashEscapes, replaceLineBreaksWithCharFunction: _options.ReplaceLineBreaksWithCharFunction);
            _mediumtextUnicode = new XGStringTypeMapping("mediumtext", StoreTypePostfix.None, noBackslashEscapes: _options.NoBackslashEscapes, replaceLineBreaksWithCharFunction: _options.ReplaceLineBreaksWithCharFunction);
            _longtextUnicode = new XGStringTypeMapping("longtext", StoreTypePostfix.None, noBackslashEscapes: _options.NoBackslashEscapes, replaceLineBreaksWithCharFunction: _options.ReplaceLineBreaksWithCharFunction);

            _nchar = new XGStringTypeMapping("nchar", StoreTypePostfix.Size, fixedLength: true, noBackslashEscapes: _options.NoBackslashEscapes, replaceLineBreaksWithCharFunction: _options.ReplaceLineBreaksWithCharFunction);
            _nvarchar = new XGStringTypeMapping("nvarchar", StoreTypePostfix.Size, noBackslashEscapes: _options.NoBackslashEscapes, replaceLineBreaksWithCharFunction: _options.ReplaceLineBreaksWithCharFunction);

            _enum = new XGStringTypeMapping("enum", StoreTypePostfix.None, noBackslashEscapes: _options.NoBackslashEscapes, replaceLineBreaksWithCharFunction: _options.ReplaceLineBreaksWithCharFunction);

            _guid = new XGGuidTypeMapping();

            _jsonDefaultString = new XGJsonTypeMapping<string>("json", null, null, _options.NoBackslashEscapes, _options.ReplaceLineBreaksWithCharFunction);

            _storeTypeMappings
                = new Dictionary<string, RelationalTypeMapping[]>(StringComparer.OrdinalIgnoreCase)
                {
                    // bit
                    { "bit",                       new[] { _bit } },

                    // integers
                    { "tinyint",                   new[] { _tinyint } },
                    //{ "tinyint unsigned",          new[] { _utinyint } },
                    { "smallint",                  new[] { _smallint } },
                    //{ "smallint unsigned",         new[] { _usmallint } },
                    { "mediumint",                 new[] { _int } },
                    //{ "mediumint unsigned",        new[] { _uint } },
                    { "int",                       new[] { _int } },
                    //{ "int unsigned",              new[] { _uint } },
                    { "integer",                   new[] { _int } },
                    //{ "integer unsigned",          new[] { _uint } },
                    { "bigint",                    new[] { _bigint } },
                    //{ "bigint unsigned",           new[] { _ubigint } },

                    // decimals
                    { "decimal",                   new[] { _decimal } },
                    { "decimal unsigned",          new[] { _decimal } },
                    { "numeric",                   new[] { _decimal } },
                    { "numeric unsigned",          new[] { _decimal } },
                    { "dec",                       new[] { _decimal } },
                    { "dec unsigned",              new[] { _decimal } },
                    { "fixed",                     new[] { _decimal } },
                    { "fixed unsigned",            new[] { _decimal } },
                    { "double",                    new[] { _double } },
                    { "double unsigned",           new[] { _double } },
                    { "double precision",          new[] { _double } },
                    { "double precision unsigned", new[] { _double } },
                    { "real",                      new[] { _double } },
                    { "real unsigned",             new[] { _double } },
                    { "float",                     new[] { _float } },
                    { "float unsigned",            new[] { _float } },

                    // binary
                    { "binary",                    new[] { _binary } },
                    { "varbinary",                 new[] { _binary } },
                    { "tinyblob",                  new[] { _binary } },
                    { "blob",                      new[] { _binary } },
                    { "mediumblob",                new[] { _binary } },
                    { "longblob",                  new[] { _binary } },

                    // string
                    { "char",                      new[] { _charUnicode } },
                    { "varchar",                   new[] { _varcharUnicode } },
                    { "tinytext",                  new[] { _tinytextUnicode } },
                    { "text",                      new[] { _varcharMax } },
                    { "mediumtext",                new[] { _varcharMax } },
                    { "longtext",                  new[] { _varcharMax } },

                    { "boolean",                   new[] { _boolean } },

                    { "enum",                      new[] { _enum } },

                    { "nchar",                     new[] { _nchar } },
                    { "nvarchar",                  new[] { _nvarchar } },

                    // DateTime
                    { "year",                      new[] { _year } },
                    { "date",                      new RelationalTypeMapping[] { _dateDateOnly, _dateDateTime } },
                    { "time",                      new RelationalTypeMapping[] { _timeTimeOnly, _timeTimeTime } },
                    { "datetime",                  new RelationalTypeMapping[] { _dateTime, _dateTimeOffset } },
                    { "timestamp",                 new RelationalTypeMapping[] { _timeStamp, _timeStampOffset } },
                };

            _clrTypeMappings
                = new Dictionary<Type, RelationalTypeMapping>
                {
	                // integers
	                { typeof(short),   _smallint },
                    //{ typeof(ushort),  _usmallint },
                    { typeof(int),     _int },
                    //{ typeof(uint),    _uint },
                    { typeof(long),    _bigint },
                    //{ typeof(ulong),   _ubigint },

	                // decimals
	                { typeof(decimal), _decimal },
                    { typeof(float),   _float },
                    { typeof(double),  _double },

	                // byte / char
	                { typeof(sbyte),   _tinyint },
                    { typeof(string),   _varcharMax },
                    //{ typeof(byte),    _utinyint },
                    { typeof(bool),    _boolean },

                    // datetimes
                    { typeof(DateOnly), _dateDateOnly },
                    { typeof(TimeOnly), _timeTimeOnly.WithPrecisionAndScale(_options.DefaultDataTypeMappings.ClrTimeOnlyPrecision, null) },
                    { typeof(TimeSpan), _options.DefaultDataTypeMappings.ClrTimeSpan switch
                        {
                            XGTimeSpanType.Time6 => _time.WithPrecisionAndScale(3, null),
                            XGTimeSpanType.Time => _time,
                            _ => _time
                        }},
                    { typeof(DateTime), _options.DefaultDataTypeMappings.ClrDateTime switch
                        {
                            XGDateTimeType.DateTime6 =>_dateTime.WithPrecisionAndScale(6, null),
                            XGDateTimeType.Timestamp6 => _timeStamp.WithPrecisionAndScale(6, null),
                            XGDateTimeType.Timestamp => _timeStamp,
                            _ => _dateTime,
                        }},
                    { typeof(DateTimeOffset), _options.DefaultDataTypeMappings.ClrDateTimeOffset switch
                        {
                            XGDateTimeType.DateTime6 =>_dateTimeOffset.WithPrecisionAndScale(null, null),
                            XGDateTimeType.Timestamp6 => _timeStampOffset.WithPrecisionAndScale(null, null),
                            XGDateTimeType.Timestamp => _timeStampOffset,
                            _ => _dateTimeOffset,
                        }},

                    // json
                    { typeof(XGJsonString), _jsonDefaultString }
                };


            // Guid
            if (_guid != null)
            {
                _storeTypeMappings[_guid.StoreType] = new RelationalTypeMapping[] { _guid };
                _clrTypeMappings[typeof(Guid)] = _guid;
            }

            // Type mappings that only exist to work around the limited code generation capabilites when scaffolding:
            _scaffoldingClrTypeMappings = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(XGCodeGenerationMemberAccess), _codeGenerationMemberAccess },
                { typeof(XGCodeGenerationServerVersionCreation), _codeGenerationServerVersionCreation },
            };
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo) =>
            // first, try any plugins, allowing them to override built-in mappings
            base.FindMapping(mappingInfo) ??
            FindRawMapping(mappingInfo)?.Clone(mappingInfo);

        private RelationalTypeMapping FindRawMapping(RelationalTypeMappingInfo mappingInfo)
        {
            // Use deferred initialization to support connection (string) based type mapping in
            // design time mode (scaffolder etc.).
            // This is a singleton class and therefore needs to be thread-safe.
            if (!_initialized)
            {
                lock (_initializationLock)
                {
                    if (!_initialized)
                    {
                        Initialize();
                        _initialized = true;
                    }
                }
            }

            var clrType = mappingInfo.ClrType;
            var storeTypeName = mappingInfo.StoreTypeName;
            var storeTypeNameBase = mappingInfo.StoreTypeNameBase;

            if (storeTypeName != null)
            {
                // First look for the fully qualified store type name.
                if (_storeTypeMappings.TryGetValue(storeTypeName, out var mappings))
                {
                    // We found the user-specified store type.
                    // If no CLR type was provided, we're probably scaffolding from an existing database. Take the first
                    // mapping as the default.
                    // If a CLR type was provided, look for a mapping between the store and CLR types. If none is found,
                    // fail immediately.
                    return clrType == null
                        ? mappings[0]
                        : mappings.FirstOrDefault(m => m.ClrType == clrType);
                }

                // Then look for the base store type name.
                if (_storeTypeMappings.TryGetValue(storeTypeNameBase, out mappings))
                {
                    return clrType == null
                        ? mappings[0]
                            .WithTypeMappingInfo(in mappingInfo)
                        : mappings.FirstOrDefault(m => m.ClrType == clrType)
                            ?.WithTypeMappingInfo(in mappingInfo);
                }

                if (storeTypeName.Equals("json", StringComparison.OrdinalIgnoreCase) &&
                    (clrType == null || clrType == typeof(string) || clrType == typeof(XGJsonString)))
                {
                    return _jsonDefaultString;
                }

                // A store type name was provided, but is unknown. This could be a domain (alias) type, in which case
                // we proceed with a CLR type lookup (if the type doesn't exist at all the failure will come later).
            }

            if (clrType != null)
            {
                if (_clrTypeMappings.TryGetValue(clrType, out var mapping))
                {
                    // If needed, clone the mapping with the configured length/precision/scale
                    if (mappingInfo.Precision.HasValue)
                    {
                        if (clrType == typeof(decimal))
                        {
                            return mapping.WithPrecisionAndScale(mappingInfo.Precision.Value, mappingInfo.Scale);
                        }

                        if (clrType == typeof(DateTime) ||
                            clrType == typeof(DateTimeOffset) ||
                            clrType == typeof(TimeSpan))
                        {
                            return mapping.WithPrecisionAndScale(mappingInfo.Precision.Value, null);
                        }
                    }

                    return mapping;
                }

                if (clrType == typeof(string))
                {
                    var isFixedLength = mappingInfo.IsFixedLength == true;

                    // Because we cannot check the annotations of the property mapping, we can't know whether `HasPrefixLength()` has been
                    // used or not. Therefore by default, the `LimitKeyedOrIndexedStringColumnLength` option will be true, and we will
                    // ensure, that the length of string properties will be set to a reasonable length, so that two columns limited this
                    // way could still fit.
                    // If users disable the `LimitKeyedOrIndexedStringColumnLength` option, they are responsible for oppropriately calling
                    // `HasPrefixLength()` for string properties, that are not mapped to a store type, where needed.
                    var size = mappingInfo.Size ??
                               (mappingInfo.IsKeyOrIndex &&
                                _options.LimitKeyedOrIndexedStringColumnLength
                                   // Allow to use at most half of the max key length, so at least 2 columns can fit
                                   ? Math.Min(_options.ServerVersion.MaxKeyLength / (_options.DefaultCharSet.MaxBytesPerChar * 2), 255)
                                   : (int?)null);

                    // If a string column size is bigger than it can/might be, we automatically adjust it to a variable one with an
                    // unlimited size.
                    if (size > 65_535 / _options.DefaultCharSet.MaxBytesPerChar)
                    {
                        size = null;
                        isFixedLength = false;
                    }
                    else if (size < 0) // specifying HasMaxLength(-1) is valid and should lead to an unbounded string/text.
                    {
                        size = null;
                    }

                    mapping = isFixedLength
                        ? _charUnicode
                        : size == null
                            ? _longtextUnicode
                            : _varcharUnicode;

                    return size == null
                        ? mapping
                        : mapping.WithStoreTypeAndSize($"{mapping.StoreTypeNameBase}({size})", size);
                }

                if (clrType == typeof(byte[]))
                {
                    if (mappingInfo.IsRowVersion == true)
                    {
                        return _options.ServerVersion.Supports.DateTime6
                            ? _binaryRowVersion6
                            : _binaryRowVersion;
                    }

                    var size = mappingInfo.Size ??
                               (mappingInfo.IsKeyOrIndex
                                   ? _options.ServerVersion.MaxKeyLength
                                   : (int?)null);

                    // Specifying HasMaxLength(-1) is valid and should lead to an unbounded byte array/blob.
                    if (size < 0)
                    {
                        size = null;
                    }

                    return new XGByteArrayTypeMapping(
                        size: size,
                        fixedLength: mappingInfo.IsFixedLength == true);
                }

                if (_scaffoldingClrTypeMappings.TryGetValue(clrType, out mapping))
                {
                    return mapping;
                }
            }

            return null;
        }

        protected override string ParseStoreTypeName(
            string storeTypeName,
            ref bool? unicode,
            ref int? size,
            ref int? precision,
            ref int? scale)
        {
            var storeTypeBaseName = base.ParseStoreTypeName(storeTypeName, ref unicode, ref size, ref precision, ref scale);

            if (storeTypeBaseName is not null)
            {
                // We are checking for a character set clause as part of the store type base name here, because it was common before 5.0
                // to specify charsets this way, because there were no character set specific annotations available yet.
                // Users might still use migrations generated with previous versions and just add newer migrations on top of those.
                var characterSetOccurrenceIndex = storeTypeBaseName.IndexOf("character set", StringComparison.OrdinalIgnoreCase);

                if (characterSetOccurrenceIndex < 0)
                {
                    characterSetOccurrenceIndex = storeTypeBaseName.IndexOf("charset", StringComparison.OrdinalIgnoreCase);
                }

                if (characterSetOccurrenceIndex >= 0)
                {
                    storeTypeBaseName = storeTypeBaseName[..characterSetOccurrenceIndex].TrimEnd();
                }

                //if (storeTypeName.Contains("unsigned", StringComparison.OrdinalIgnoreCase))
                //{
                //    storeTypeBaseName += " unsigned";
                //}
            }

            return storeTypeBaseName;
        }
    }
}
