// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Data;
using System.Runtime.CompilerServices;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class BufferedDataReader : DbDataReader
{
    private readonly bool _detailedErrorsEnabled;

    private DbDataReader? _underlyingReader;
    private List<BufferedDataRecord> _bufferedDataRecords = [];
    private BufferedDataRecord _currentResultSet;
    private int _currentResultSetNumber;
    private int _recordsAffected;
    private bool _disposed;
    private bool _isClosed;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public BufferedDataReader(DbDataReader reader, bool detailedErrorsEnabled)
    {
        _underlyingReader = reader;
        _detailedErrorsEnabled = detailedErrorsEnabled;
        _currentResultSet = null!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int RecordsAffected
        => _recordsAffected;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override object this[string name]
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override object this[int ordinal]
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int Depth
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int FieldCount
    {
        get
        {
            AssertReaderIsOpen();
            return _currentResultSet.FieldCount;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool HasRows
    {
        get
        {
            AssertReaderIsOpen();
            return _currentResultSet.HasRows;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool IsClosed
        => _isClosed;

    [Conditional("DEBUG")]
    private void AssertReaderIsOpen()
    {
        if (_underlyingReader != null)
        {
            throw new InvalidOperationException("The reader wasn't initialized");
        }

        if (_isClosed)
        {
            throw new InvalidOperationException("The reader is closed.");
        }
    }

    [Conditional("DEBUG")]
    private void AssertReaderIsOpenWithData()
    {
        AssertReaderIsOpen();

        if (!_currentResultSet.IsDataReady)
        {
            throw new InvalidOperationException("The reader doesn't have any data.");
        }
    }

    [Conditional("DEBUG")]
    private void AssertFieldIsReady(int ordinal)
    {
        AssertReaderIsOpenWithData();

        if (0 > ordinal
            || ordinal > _currentResultSet.FieldCount)
        {
            throw new IndexOutOfRangeException();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual BufferedDataReader Initialize(IReadOnlyList<ReaderColumn?> columns)
    {
        if (_underlyingReader == null)
        {
            return this;
        }

        try
        {
            do
            {
                _bufferedDataRecords.Add(new BufferedDataRecord(_detailedErrorsEnabled).Initialize(_underlyingReader, columns));
            }
            while (_underlyingReader.NextResult());

            _recordsAffected = _underlyingReader.RecordsAffected;
            _currentResultSet = _bufferedDataRecords[_currentResultSetNumber];

            return this;
        }
        finally
        {
            _underlyingReader.Dispose();
            _underlyingReader = null;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual async Task<BufferedDataReader> InitializeAsync(
        IReadOnlyList<ReaderColumn?> columns,
        CancellationToken cancellationToken)
    {
        if (_underlyingReader == null)
        {
            return this;
        }

        try
        {
            do
            {
                _bufferedDataRecords.Add(
                    await new BufferedDataRecord(_detailedErrorsEnabled).InitializeAsync(_underlyingReader, columns, cancellationToken)
                        .ConfigureAwait(false));
            }
            while (await _underlyingReader.NextResultAsync(cancellationToken).ConfigureAwait(false));

            _recordsAffected = _underlyingReader.RecordsAffected;
            _currentResultSet = _bufferedDataRecords[_currentResultSetNumber];

            return this;
        }
        finally
        {
            await _underlyingReader.DisposeAsync().ConfigureAwait(false);
            _underlyingReader = null;
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsSupportedValueType(Type type)
        => type == typeof(int)
            || type == typeof(bool)
            || type == typeof(Guid)
            || type == typeof(byte)
            || type == typeof(char)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(decimal)
            || type == typeof(double)
            || type == typeof(float)
            || type == typeof(short)
            || type == typeof(long)
            || type == typeof(uint)
            || type == typeof(ushort)
            || type == typeof(ulong)
            || type == typeof(sbyte);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void Close()
    {
        _bufferedDataRecords = null!;
        _isClosed = true;

        var reader = _underlyingReader;
        if (reader != null)
        {
            _underlyingReader = null;
            reader.Dispose();
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (!_disposed
            && disposing
            && !IsClosed)
        {
            Close();
        }

        _disposed = true;

        base.Dispose(disposing);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool GetBoolean(int ordinal)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.GetBoolean(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override byte GetByte(int ordinal)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.GetByte(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override char GetChar(int ordinal)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.GetChar(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override DateTime GetDateTime(int ordinal)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.GetDateTime(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override decimal GetDecimal(int ordinal)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.GetDecimal(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override double GetDouble(int ordinal)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.GetDouble(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override float GetFloat(int ordinal)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.GetFloat(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Guid GetGuid(int ordinal)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.GetGuid(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override short GetInt16(int ordinal)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.GetInt16(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int GetInt32(int ordinal)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.GetInt32(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override long GetInt64(int ordinal)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.GetInt64(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string GetString(int ordinal)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.GetFieldValue<string>(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override T GetFieldValue<T>(int ordinal)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.GetFieldValue<T>(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.GetFieldValueAsync<T>(ordinal, cancellationToken);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override object GetValue(int ordinal)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.GetValue(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int GetValues(object[] values)
    {
        AssertReaderIsOpenWithData();
        return BufferedDataRecord.GetValues(values);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string GetDataTypeName(int ordinal)
    {
        AssertReaderIsOpen();
        return _currentResultSet.GetDataTypeName(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Type GetFieldType(int ordinal)
    {
        AssertReaderIsOpen();
        return _currentResultSet.GetFieldType(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string GetName(int ordinal)
    {
        AssertReaderIsOpen();
        return _currentResultSet.GetName(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override int GetOrdinal(string name)
    {
        AssertReaderIsOpen();
        return _currentResultSet.GetOrdinal(name);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool IsDBNull(int ordinal)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.IsDBNull(ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
    {
        AssertFieldIsReady(ordinal);
        return _currentResultSet.IsDBNullAsync(ordinal, cancellationToken);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEnumerator GetEnumerator()
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override DataTable GetSchemaTable()
        => throw new NotSupportedException();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool NextResult()
    {
        AssertReaderIsOpen();
        if (++_currentResultSetNumber < _bufferedDataRecords.Count)
        {
            _currentResultSet = _bufferedDataRecords[_currentResultSetNumber];
            return true;
        }

        _currentResultSet = null!;
        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
        => Task.FromResult(NextResult());

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override bool Read()
    {
        AssertReaderIsOpen();
        return _currentResultSet.Read();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
        AssertReaderIsOpen();
        return _currentResultSet.ReadAsync(cancellationToken);
    }

    private sealed class BufferedDataRecord
    {
        private int _currentRowNumber = -1;
        private int _rowCount;
        private string[] _dataTypeNames;
        private Type[] _fieldTypes;
        private string[] _columnNames;
        private Lazy<Dictionary<string, int>> _fieldNameLookup;

        private int _rowCapacity = 1;

        // Resizing bool[] is faster than BitArray, but the latter is more efficient for long-term storage.
        private BitArray _bools;
        private bool[] _tempBools;
        private int _boolCount;
        private byte[] _bytes;
        private int _byteCount;
        private char[] _chars;
        private int _charCount;
        private DateTime[] _dateTimes;
        private int _dateTimeCount;
        private DateTimeOffset[] _dateTimeOffsets;
        private int _dateTimeOffsetCount;
        private decimal[] _decimals;
        private int _decimalCount;
        private double[] _doubles;
        private int _doubleCount;
        private float[] _floats;
        private int _floatCount;
        private Guid[] _guids;
        private int _guidCount;
        private short[] _shorts;
        private int _shortCount;
        private int[] _ints;
        private int _intCount;
        private long[] _longs;
        private int _longCount;
        private sbyte[] _sbytes;
        private int _sbyteCount;
        private uint[] _uints;
        private int _uintCount;
        private ushort[] _ushorts;
        private int _ushortCount;
        private ulong[] _ulongs;
        private int _ulongCount;
        private object[] _objects;
        private int _objectCount;
        private int[] _ordinalToIndexMap;

        private BitArray _nulls;
        private bool[] _tempNulls;
        private int _nullCount;
        private int[] _nullOrdinalToIndexMap;

        private TypeCase[] _columnTypeCases;

        private DbDataReader _underlyingReader;
        private IReadOnlyList<ReaderColumn?> _columns;
        private int[] _indexMap;
        private readonly bool _detailedErrorsEnabled;

        public BufferedDataRecord(bool detailedErrorsEnabled)
        {
            _detailedErrorsEnabled = detailedErrorsEnabled;
            _dataTypeNames = null!;
            _columnNames = null!;
            _columns = null!;
            _columnTypeCases = null!;
            _fieldNameLookup = null!;
            _fieldTypes = null!;
            _indexMap = null!;
            _nullOrdinalToIndexMap = null!;
            _ordinalToIndexMap = null!;
            _underlyingReader = null!;

            _bools = null!;
            _bytes = null!;
            _chars = null!;
            _dateTimeOffsets = null!;
            _dateTimes = null!;
            _decimals = null!;
            _doubles = null!;
            _floats = null!;
            _guids = null!;
            _ints = null!;
            _longs = null!;
            _longs = null!;
            _nulls = null!;
            _objects = null!;
            _sbytes = null!;
            _shorts = null!;
            _tempBools = null!;
            _tempNulls = null!;
            _uints = null!;
            _ulongs = null!;
            _ushorts = null!;
        }

        public bool IsDataReady { get; private set; }

        public bool HasRows
            => _rowCount > 0;

        public int FieldCount
            => _fieldTypes.Length;

        public string GetDataTypeName(int ordinal)
            => _dataTypeNames[ordinal];

        public Type GetFieldType(int ordinal)
            => _fieldTypes[ordinal];

        public string GetName(int ordinal)
            => _columnNames[ordinal];

        public int GetOrdinal(string name)
            => _fieldNameLookup.Value[name];

        public bool GetBoolean(int ordinal)
            => _columnTypeCases[ordinal] == TypeCase.Bool
                ? _bools[_currentRowNumber * _boolCount + _ordinalToIndexMap[ordinal]]
                : GetFieldValue<bool>(ordinal);

        public byte GetByte(int ordinal)
            => _columnTypeCases[ordinal] == TypeCase.Byte
                ? _bytes[_currentRowNumber * _byteCount + _ordinalToIndexMap[ordinal]]
                : GetFieldValue<byte>(ordinal);

        public char GetChar(int ordinal)
            => _columnTypeCases[ordinal] == TypeCase.Char
                ? _chars[_currentRowNumber * _charCount + _ordinalToIndexMap[ordinal]]
                : GetFieldValue<char>(ordinal);

        public DateTime GetDateTime(int ordinal)
            => _columnTypeCases[ordinal] == TypeCase.DateTime
                ? _dateTimes[_currentRowNumber * _dateTimeCount + _ordinalToIndexMap[ordinal]]
                : GetFieldValue<DateTime>(ordinal);

        public DateTimeOffset GetDateTimeOffset(int ordinal)
            => _columnTypeCases[ordinal] == TypeCase.DateTimeOffset
                ? _dateTimeOffsets[_currentRowNumber * _dateTimeOffsetCount + _ordinalToIndexMap[ordinal]]
                : GetFieldValue<DateTimeOffset>(ordinal);

        public decimal GetDecimal(int ordinal)
            => _columnTypeCases[ordinal] == TypeCase.Decimal
                ? _decimals[_currentRowNumber * _decimalCount + _ordinalToIndexMap[ordinal]]
                : GetFieldValue<decimal>(ordinal);

        public double GetDouble(int ordinal)
            => _columnTypeCases[ordinal] == TypeCase.Double
                ? _doubles[_currentRowNumber * _doubleCount + _ordinalToIndexMap[ordinal]]
                : GetFieldValue<double>(ordinal);

        public float GetFloat(int ordinal)
            => _columnTypeCases[ordinal] == TypeCase.Float
                ? _floats[_currentRowNumber * _floatCount + _ordinalToIndexMap[ordinal]]
                : GetFieldValue<float>(ordinal);

        public Guid GetGuid(int ordinal)
            => _columnTypeCases[ordinal] == TypeCase.Guid
                ? _guids[_currentRowNumber * _guidCount + _ordinalToIndexMap[ordinal]]
                : GetFieldValue<Guid>(ordinal);

        public short GetInt16(int ordinal)
            => _columnTypeCases[ordinal] == TypeCase.Short
                ? _shorts[_currentRowNumber * _shortCount + _ordinalToIndexMap[ordinal]]
                : GetFieldValue<short>(ordinal);

        public int GetInt32(int ordinal)
            => _columnTypeCases[ordinal] == TypeCase.Int
                ? _ints[_currentRowNumber * _intCount + _ordinalToIndexMap[ordinal]]
                : GetFieldValue<int>(ordinal);

        public long GetInt64(int ordinal)
            => _columnTypeCases[ordinal] == TypeCase.Long
                ? _longs[_currentRowNumber * _longCount + _ordinalToIndexMap[ordinal]]
                : GetFieldValue<long>(ordinal);

        public sbyte GetSByte(int ordinal)
            => _columnTypeCases[ordinal] == TypeCase.SByte
                ? _sbytes[_currentRowNumber * _sbyteCount + _ordinalToIndexMap[ordinal]]
                : GetFieldValue<sbyte>(ordinal);

        public ushort GetUInt16(int ordinal)
            => _columnTypeCases[ordinal] == TypeCase.UShort
                ? _ushorts[_currentRowNumber * _ushortCount + _ordinalToIndexMap[ordinal]]
                : GetFieldValue<ushort>(ordinal);

        public uint GetUInt32(int ordinal)
            => _columnTypeCases[ordinal] == TypeCase.UInt
                ? _uints[_currentRowNumber * _uintCount + _ordinalToIndexMap[ordinal]]
                : GetFieldValue<uint>(ordinal);

        public ulong GetUInt64(int ordinal)
            => _columnTypeCases[ordinal] == TypeCase.ULong
                ? _ulongs[_currentRowNumber * _ulongCount + _ordinalToIndexMap[ordinal]]
                : GetFieldValue<ulong>(ordinal);

        public object GetValue(int ordinal)
            => GetFieldValue<object>(ordinal);

#pragma warning disable IDE0060 // Remove unused parameter
        public static int GetValues(object[] values)
#pragma warning restore IDE0060 // Remove unused parameter
            => throw new NotSupportedException();

        public T GetFieldValue<T>(int ordinal)
            => (_columnTypeCases[ordinal]) switch
            {
                TypeCase.Bool => (T)(object)GetBoolean(ordinal),
                TypeCase.Byte => (T)(object)GetByte(ordinal),
                TypeCase.Char => (T)(object)GetChar(ordinal),
                TypeCase.DateTime => (T)(object)GetDateTime(ordinal),
                TypeCase.DateTimeOffset => (T)(object)GetDateTimeOffset(ordinal),
                TypeCase.Decimal => (T)(object)GetDecimal(ordinal),
                TypeCase.Double => (T)(object)GetDouble(ordinal),
                TypeCase.Float => (T)(object)GetFloat(ordinal),
                TypeCase.Guid => (T)(object)GetGuid(ordinal),
                TypeCase.Short => (T)(object)GetInt16(ordinal),
                TypeCase.Int => (T)(object)GetInt32(ordinal),
                TypeCase.Long => (T)(object)GetInt64(ordinal),
                TypeCase.SByte => (T)(object)GetSByte(ordinal),
                TypeCase.UShort => (T)(object)GetUInt16(ordinal),
                TypeCase.UInt => (T)(object)GetUInt32(ordinal),
                TypeCase.ULong => (T)(object)GetUInt64(ordinal),
                _ => (T)_objects[_currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal]]
            };

        // ReSharper disable once InconsistentNaming
        public bool IsDBNull(int ordinal)
            => _nulls[_currentRowNumber * _nullCount + _nullOrdinalToIndexMap[ordinal]];

        public bool Read()
            => IsDataReady = ++_currentRowNumber < _rowCount;

#pragma warning disable IDE0060 // Remove unused parameter
        public Task<T> GetFieldValueAsync<T>(int ordinal, CancellationToken cancellationToken)
            => Task.FromResult(GetFieldValue<T>(ordinal));

        // ReSharper disable once InconsistentNaming
        public Task<bool> IsDBNullAsync(int ordinal, CancellationToken cancellationToken)
            => Task.FromResult(IsDBNull(ordinal));

        public Task<bool> ReadAsync(CancellationToken cancellationToken)
            => Task.FromResult(Read());
#pragma warning restore IDE0060 // Remove unused parameter

        public BufferedDataRecord Initialize(DbDataReader reader, IReadOnlyList<ReaderColumn?> columns)
        {
            _underlyingReader = reader;
            _columns = columns;

            ReadMetadata();
            InitializeFields();

            while (reader.Read())
            {
                ReadRow();
            }

            _bools = new BitArray(_tempBools);
            _tempBools = null!;
            _nulls = new BitArray(_tempNulls);
            _tempNulls = null!;
            _rowCount = _currentRowNumber + 1;
            _currentRowNumber = -1;
            _underlyingReader = null!;
            _columns = null!;

            return this;
        }

        public async Task<BufferedDataRecord> InitializeAsync(
            DbDataReader reader,
            IReadOnlyList<ReaderColumn?> columns,
            CancellationToken cancellationToken)
        {
            _underlyingReader = reader;
            _columns = columns;

            ReadMetadata();
            InitializeFields();

            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                ReadRow();
            }

            _bools = new BitArray(_tempBools);
            _tempBools = null!;
            _nulls = new BitArray(_tempNulls);
            _tempNulls = null!;
            _rowCount = _currentRowNumber + 1;
            _currentRowNumber = -1;
            _underlyingReader = null!;
            _columns = null!;

            return this;
        }

        private void ReadRow()
        {
            _currentRowNumber++;

            if (_rowCapacity == _currentRowNumber)
            {
                DoubleBufferCapacity();
            }

            for (var i = 0; i < FieldCount; i++)
            {
                var column = _columns[i];
                if (column == null)
                {
                    continue;
                }

                var nullIndex = _nullOrdinalToIndexMap[i];
                switch (_columnTypeCases[i])
                {
                    case TypeCase.Bool:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadBool(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadBool(_underlyingReader, i, column);
                        }

                        break;
                    case TypeCase.Byte:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadByte(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadByte(_underlyingReader, i, column);
                        }

                        break;
                    case TypeCase.Char:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadChar(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadChar(_underlyingReader, i, column);
                        }

                        break;
                    case TypeCase.DateTime:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadDateTime(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadDateTime(_underlyingReader, i, column);
                        }

                        break;
                    case TypeCase.DateTimeOffset:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadDateTimeOffset(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadDateTimeOffset(_underlyingReader, i, column);
                        }

                        break;
                    case TypeCase.Decimal:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadDecimal(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadDecimal(_underlyingReader, i, column);
                        }

                        break;
                    case TypeCase.Double:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadDouble(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadDouble(_underlyingReader, i, column);
                        }

                        break;
                    case TypeCase.Float:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadFloat(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadFloat(_underlyingReader, i, column);
                        }

                        break;
                    case TypeCase.Guid:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadGuid(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadGuid(_underlyingReader, i, column);
                        }

                        break;
                    case TypeCase.Short:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadShort(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadShort(_underlyingReader, i, column);
                        }

                        break;
                    case TypeCase.Int:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadInt(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadInt(_underlyingReader, i, column);
                        }

                        break;
                    case TypeCase.Long:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadLong(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadLong(_underlyingReader, i, column);
                        }

                        break;
                    case TypeCase.SByte:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadSByte(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadSByte(_underlyingReader, i, column);
                        }

                        break;
                    case TypeCase.UShort:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadUShort(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadUShort(_underlyingReader, i, column);
                        }

                        break;
                    case TypeCase.UInt:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadUInt(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadUInt(_underlyingReader, i, column);
                        }

                        break;
                    case TypeCase.ULong:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadULong(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadULong(_underlyingReader, i, column);
                        }

                        break;
                    case TypeCase.Empty:
                        break;
                    default:
                        if (nullIndex != -1)
                        {
                            if (!(_tempNulls[_currentRowNumber * _nullCount + nullIndex] = _underlyingReader.IsDBNull(i)))
                            {
                                ReadObject(_underlyingReader, i, column);
                            }
                        }
                        else
                        {
                            ReadObject(_underlyingReader, i, column);
                        }

                        break;
                }
            }
        }

        private void ReadMetadata()
        {
            var fieldCount = _underlyingReader.FieldCount;
            var dataTypeNames = new string[fieldCount];
            var columnTypes = new Type[fieldCount];
            var columnNames = new string[fieldCount];
            for (var i = 0; i < fieldCount; i++)
            {
                dataTypeNames[i] = _underlyingReader.GetDataTypeName(i);
                columnTypes[i] = _underlyingReader.GetFieldType(i);
                columnNames[i] = _underlyingReader.GetName(i);
            }

            _dataTypeNames = dataTypeNames;
            _fieldTypes = columnTypes;
            _columnNames = columnNames;
            _fieldNameLookup = new Lazy<Dictionary<string, int>>(CreateNameLookup, isThreadSafe: false);

            Dictionary<string, int> CreateNameLookup()
            {
                var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                for (var i = 0; i < _columnNames.Length; i++)
                {
                    index[_columnNames[i]] = i;
                }

                return index;
            }
        }

        private void InitializeFields()
        {
            var fieldCount = FieldCount;
            if (FieldCount < _columns.Count)
            {
                // Non-composed FromSql
                var firstMissingColumn = _columns.Select(c => c?.Name).Where(c => c != null).Except(_columnNames).FirstOrDefault();
                if (firstMissingColumn != null)
                {
                    throw new InvalidOperationException(RelationalStrings.FromSqlMissingColumn(firstMissingColumn));
                }

                throw new InvalidOperationException(RelationalStrings.TooFewReaderFields(_columns.Count, FieldCount));
            }

            _columnTypeCases = Enumerable.Repeat(TypeCase.Empty, fieldCount).ToArray();
            _ordinalToIndexMap = Enumerable.Repeat(-1, fieldCount).ToArray();
            if (_columns.Count > 0
                && _columns.Any(e => e?.Name != null))
            {
                // Non-Composed FromSql
                var readerColumns = _fieldNameLookup.Value;

                _indexMap = new int[_columns.Count];
                var newColumnMap = new ReaderColumn?[fieldCount];
                for (var i = 0; i < _columns.Count; i++)
                {
                    var column = _columns[i];
                    if (column == null)
                    {
                        continue;
                    }

                    if (!readerColumns.TryGetValue(column.Name!, out var ordinal))
                    {
                        if (_columns.Count != 1)
                        {
                            throw new InvalidOperationException(RelationalStrings.FromSqlMissingColumn(column.Name));
                        }

                        ordinal = 0;
                    }

                    newColumnMap[ordinal] = column;
                    _indexMap[i] = ordinal;
                }

                _columns = newColumnMap;
            }

            if (FieldCount != _columns.Count)
            {
                var newColumnMap = new ReaderColumn?[fieldCount];
                for (var i = 0; i < _columns.Count; i++)
                {
                    newColumnMap[i] = _columns[i];
                }

                _columns = newColumnMap;
            }

            for (var i = 0; i < fieldCount; i++)
            {
                var column = _columns[i];
                if (column == null)
                {
                    continue;
                }

                var type = column.Type;
                if (type == typeof(bool))
                {
                    _columnTypeCases[i] = TypeCase.Bool;
                    _ordinalToIndexMap[i] = _boolCount;
                    _boolCount++;
                }
                else if (type == typeof(byte))
                {
                    _columnTypeCases[i] = TypeCase.Byte;
                    _ordinalToIndexMap[i] = _byteCount;
                    _byteCount++;
                }
                else if (type == typeof(char))
                {
                    _columnTypeCases[i] = TypeCase.Char;
                    _ordinalToIndexMap[i] = _charCount;
                    _charCount++;
                }
                else if (type == typeof(DateTime))
                {
                    _columnTypeCases[i] = TypeCase.DateTime;
                    _ordinalToIndexMap[i] = _dateTimeCount;
                    _dateTimeCount++;
                }
                else if (type == typeof(DateTimeOffset))
                {
                    _columnTypeCases[i] = TypeCase.DateTimeOffset;
                    _ordinalToIndexMap[i] = _dateTimeOffsetCount;
                    _dateTimeOffsetCount++;
                }
                else if (type == typeof(decimal))
                {
                    _columnTypeCases[i] = TypeCase.Decimal;
                    _ordinalToIndexMap[i] = _decimalCount;
                    _decimalCount++;
                }
                else if (type == typeof(double))
                {
                    _columnTypeCases[i] = TypeCase.Double;
                    _ordinalToIndexMap[i] = _doubleCount;
                    _doubleCount++;
                }
                else if (type == typeof(float))
                {
                    _columnTypeCases[i] = TypeCase.Float;
                    _ordinalToIndexMap[i] = _floatCount;
                    _floatCount++;
                }
                else if (type == typeof(Guid))
                {
                    _columnTypeCases[i] = TypeCase.Guid;
                    _ordinalToIndexMap[i] = _guidCount;
                    _guidCount++;
                }
                else if (type == typeof(short))
                {
                    _columnTypeCases[i] = TypeCase.Short;
                    _ordinalToIndexMap[i] = _shortCount;
                    _shortCount++;
                }
                else if (type == typeof(int))
                {
                    _columnTypeCases[i] = TypeCase.Int;
                    _ordinalToIndexMap[i] = _intCount;
                    _intCount++;
                }
                else if (type == typeof(long))
                {
                    _columnTypeCases[i] = TypeCase.Long;
                    _ordinalToIndexMap[i] = _longCount;
                    _longCount++;
                }
                else if (type == typeof(sbyte))
                {
                    _columnTypeCases[i] = TypeCase.SByte;
                    _ordinalToIndexMap[i] = _sbyteCount;
                    _sbyteCount++;
                }
                else if (type == typeof(ushort))
                {
                    _columnTypeCases[i] = TypeCase.UShort;
                    _ordinalToIndexMap[i] = _ushortCount;
                    _ushortCount++;
                }
                else if (type == typeof(uint))
                {
                    _columnTypeCases[i] = TypeCase.UInt;
                    _ordinalToIndexMap[i] = _uintCount;
                    _uintCount++;
                }
                else if (type == typeof(ulong))
                {
                    _columnTypeCases[i] = TypeCase.ULong;
                    _ordinalToIndexMap[i] = _ulongCount;
                    _ulongCount++;
                }
                else
                {
                    _columnTypeCases[i] = TypeCase.Object;
                    _ordinalToIndexMap[i] = _objectCount;
                    _objectCount++;
                }
            }

            _tempBools = new bool[_rowCapacity * _boolCount];
            _bytes = new byte[_rowCapacity * _byteCount];
            _chars = new char[_rowCapacity * _charCount];
            _dateTimes = new DateTime[_rowCapacity * _dateTimeCount];
            _dateTimeOffsets = new DateTimeOffset[_rowCapacity * _dateTimeOffsetCount];
            _decimals = new decimal[_rowCapacity * _decimalCount];
            _doubles = new double[_rowCapacity * _doubleCount];
            _floats = new float[_rowCapacity * _floatCount];
            _guids = new Guid[_rowCapacity * _guidCount];
            _shorts = new short[_rowCapacity * _shortCount];
            _ints = new int[_rowCapacity * _intCount];
            _longs = new long[_rowCapacity * _longCount];
            _sbytes = new sbyte[_rowCapacity * _sbyteCount];
            _ushorts = new ushort[_rowCapacity * _ushortCount];
            _uints = new uint[_rowCapacity * _uintCount];
            _ulongs = new ulong[_rowCapacity * _ulongCount];
            _objects = new object[_rowCapacity * _objectCount];

            _nullOrdinalToIndexMap = Enumerable.Repeat(-1, fieldCount).ToArray();
            for (var i = 0; i < fieldCount; i++)
            {
                if (_columns[i]?.IsNullable == true)
                {
                    _nullOrdinalToIndexMap[i] = _nullCount;
                    _nullCount++;
                }
            }

            _tempNulls = new bool[_rowCapacity * _nullCount];
        }

        private void DoubleBufferCapacity()
        {
            _rowCapacity <<= 1;

            var newBools = new bool[_tempBools.Length << 1];
            Array.Copy(_tempBools, newBools, _tempBools.Length);
            _tempBools = newBools;

            var newBytes = new byte[_bytes.Length << 1];
            Array.Copy(_bytes, newBytes, _bytes.Length);
            _bytes = newBytes;

            var newChars = new char[_chars.Length << 1];
            Array.Copy(_chars, newChars, _chars.Length);
            _chars = newChars;

            var newDateTimes = new DateTime[_dateTimes.Length << 1];
            Array.Copy(_dateTimes, newDateTimes, _dateTimes.Length);
            _dateTimes = newDateTimes;

            var newDateTimeOffsets = new DateTimeOffset[_dateTimeOffsets.Length << 1];
            Array.Copy(_dateTimeOffsets, newDateTimeOffsets, _dateTimeOffsets.Length);
            _dateTimeOffsets = newDateTimeOffsets;

            var newDecimals = new decimal[_decimals.Length << 1];
            Array.Copy(_decimals, newDecimals, _decimals.Length);
            _decimals = newDecimals;

            var newDoubles = new double[_doubles.Length << 1];
            Array.Copy(_doubles, newDoubles, _doubles.Length);
            _doubles = newDoubles;

            var newFloats = new float[_floats.Length << 1];
            Array.Copy(_floats, newFloats, _floats.Length);
            _floats = newFloats;

            var newGuids = new Guid[_guids.Length << 1];
            Array.Copy(_guids, newGuids, _guids.Length);
            _guids = newGuids;

            var newShorts = new short[_shorts.Length << 1];
            Array.Copy(_shorts, newShorts, _shorts.Length);
            _shorts = newShorts;

            var newInts = new int[_ints.Length << 1];
            Array.Copy(_ints, newInts, _ints.Length);
            _ints = newInts;

            var newLongs = new long[_longs.Length << 1];
            Array.Copy(_longs, newLongs, _longs.Length);
            _longs = newLongs;

            var newSBytes = new sbyte[_sbytes.Length << 1];
            Array.Copy(_sbytes, newSBytes, _sbytes.Length);
            _sbytes = newSBytes;

            var newUShorts = new ushort[_ushorts.Length << 1];
            Array.Copy(_ushorts, newUShorts, _ushorts.Length);
            _ushorts = newUShorts;

            var newUInts = new uint[_uints.Length << 1];
            Array.Copy(_uints, newUInts, _uints.Length);
            _uints = newUInts;

            var newULongs = new ulong[_ulongs.Length << 1];
            Array.Copy(_ulongs, newULongs, _ulongs.Length);
            _ulongs = newULongs;

            var newObjects = new object[_objects.Length << 1];
            Array.Copy(_objects, newObjects, _objects.Length);
            _objects = newObjects;

            var newNulls = new bool[_tempNulls.Length << 1];
            Array.Copy(_tempNulls, newNulls, _tempNulls.Length);
            _tempNulls = newNulls;
        }

        private void ReadBool(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _tempBools[_currentRowNumber * _boolCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<bool>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _tempBools[_currentRowNumber * _boolCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<bool>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private void ReadByte(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _bytes[_currentRowNumber * _byteCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<byte>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _bytes[_currentRowNumber * _byteCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<byte>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private void ReadChar(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _chars[_currentRowNumber * _charCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<char>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _chars[_currentRowNumber * _charCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<char>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private void ReadDateTime(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _dateTimes[_currentRowNumber * _dateTimeCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<DateTime>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _dateTimes[_currentRowNumber * _dateTimeCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<DateTime>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private void ReadDateTimeOffset(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _dateTimeOffsets[_currentRowNumber * _dateTimeOffsetCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<DateTimeOffset>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _dateTimeOffsets[_currentRowNumber * _dateTimeOffsetCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<DateTimeOffset>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private void ReadDecimal(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _decimals[_currentRowNumber * _decimalCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<decimal>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _decimals[_currentRowNumber * _decimalCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<decimal>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private void ReadDouble(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _doubles[_currentRowNumber * _doubleCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<double>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _doubles[_currentRowNumber * _doubleCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<double>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private void ReadFloat(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _floats[_currentRowNumber * _floatCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<float>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _floats[_currentRowNumber * _floatCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<float>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private void ReadGuid(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _guids[_currentRowNumber * _guidCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<Guid>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _guids[_currentRowNumber * _guidCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<Guid>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private void ReadShort(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _shorts[_currentRowNumber * _shortCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<short>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _shorts[_currentRowNumber * _shortCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<short>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private void ReadInt(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _ints[_currentRowNumber * _intCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<int>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _ints[_currentRowNumber * _intCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<int>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private void ReadLong(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _longs[_currentRowNumber * _longCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<long>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _longs[_currentRowNumber * _longCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<long>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private void ReadSByte(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _sbytes[_currentRowNumber * _sbyteCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<sbyte>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _sbytes[_currentRowNumber * _sbyteCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<sbyte>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private void ReadUShort(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _ushorts[_currentRowNumber * _ushortCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<ushort>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _ushorts[_currentRowNumber * _ushortCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<ushort>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private void ReadUInt(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _uints[_currentRowNumber * _uintCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<uint>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _uints[_currentRowNumber * _uintCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<uint>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private void ReadULong(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _ulongs[_currentRowNumber * _ulongCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<ulong>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _ulongs[_currentRowNumber * _ulongCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<ulong>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private void ReadObject(DbDataReader reader, int ordinal, ReaderColumn column)
        {
            if (_detailedErrorsEnabled)
            {
                try
                {
                    _objects[_currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal]] =
                        ((ReaderColumn<object>)column).GetFieldValue(reader, _indexMap);
                }
                catch (Exception e)
                {
                    ThrowReadValueException(e, reader, ordinal, column);
                }
            }
            else
            {
                _objects[_currentRowNumber * _objectCount + _ordinalToIndexMap[ordinal]] =
                    ((ReaderColumn<object>)column).GetFieldValue(reader, _indexMap);
            }
        }

        private enum TypeCase
        {
            Empty = 0,
            Object,
            Bool,
            Byte,
            Char,
            DateTime,
            DateTimeOffset,
            Decimal,
            Double,
            Float,
            Guid,
            SByte,
            Short,
            Int,
            Long,
            UInt,
            ULong,
            UShort
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowReadValueException(
            Exception exception,
            DbDataReader reader,
            int ordinal,
            ReaderColumn column)
        {
            var value = reader.GetFieldValue<object?>(ordinal);
            var property = column.Property;
            var expectedType = column.Type.MakeNullable(column.IsNullable);

            var actualType = value?.GetType();

            string message;

            if (property != null)
            {
                var entityType = property.DeclaringType.DisplayName();
                var propertyName = property.Name;
                if (expectedType == typeof(object))
                {
                    expectedType = property.ClrType;
                }

                message = exception is NullReferenceException
                    || Equals(value, DBNull.Value)
                        ? RelationalStrings.ErrorMaterializingPropertyNullReference(entityType, propertyName, expectedType)
                        : exception is InvalidCastException
                            ? CoreStrings.ErrorMaterializingPropertyInvalidCast(entityType, propertyName, expectedType, actualType)
                            : RelationalStrings.ErrorMaterializingProperty(entityType, propertyName);
            }
            else
            {
                message = exception is NullReferenceException
                    || Equals(value, DBNull.Value)
                        ? RelationalStrings.ErrorMaterializingValueNullReference(expectedType)
                        : exception is InvalidCastException
                            ? RelationalStrings.ErrorMaterializingValueInvalidCast(expectedType, actualType)
                            : RelationalStrings.ErrorMaterializingValue;
            }

            throw new InvalidOperationException(message, exception);
        }
    }
}
