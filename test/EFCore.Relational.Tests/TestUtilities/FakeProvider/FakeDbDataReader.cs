// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

public class FakeDbDataReader : DbDataReader
{
    private readonly string[] _columnNames;
    private IList<object[]> _results;
    private readonly IList<IList<object[]>> _resultSets;

    private int _currentResultSet;
    private object[] _currentRow;
    private int _rowIndex;
    private bool _closed;

    public FakeDbDataReader(string[] columnNames = null, IList<object[]> results = null)
    {
        _columnNames = columnNames ?? [];
        _results = results ?? new List<object[]>();
        _resultSets = new List<IList<object[]>> { _results };
    }

    public FakeDbDataReader(string[] columnNames, IList<IList<object[]>> resultSets)
    {
        _columnNames = columnNames ?? [];
        _resultSets = resultSets ?? new List<IList<object[]>> { new List<object[]>() };
        _results = _resultSets[0];
    }

    public override bool Read()
    {
        _currentRow = _rowIndex < _results.Count
            ? _results[_rowIndex++]
            : null;

        return _currentRow != null;
    }

    public int ReadAsyncCount { get; private set; }

    public override Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
        ReadAsyncCount++;

        _currentRow = _rowIndex < _results.Count
            ? _results[_rowIndex++]
            : null;

        return Task.FromResult(_currentRow != null);
    }

    public int CloseCount { get; private set; }

    public override void Close()
    {
        CloseCount++;
        _closed = true;
    }

    public int DisposeCount { get; private set; }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeCount++;

            base.Dispose(true);
        }

        _closed = true;
    }

    public override int FieldCount
        => _columnNames.Length;

    public override string GetName(int ordinal)
        => _columnNames[ordinal];

    public override bool IsDBNull(int ordinal)
        => _currentRow[ordinal] == DBNull.Value;

    public override object GetValue(int ordinal)
        => _currentRow[ordinal];

    public int GetInt32Count { get; private set; }

    public override int GetInt32(int ordinal)
    {
        GetInt32Count++;

        return (int)_currentRow[ordinal];
    }

    public override object this[string name]
        => throw new NotImplementedException();

    public override object this[int ordinal]
        => throw new NotImplementedException();

    public override int Depth
        => throw new NotImplementedException();

    public override bool HasRows
        => _results.Count != 0;

    public override bool IsClosed
        => _closed;

    public override int RecordsAffected
        => _resultSets.Aggregate(0, (a, r) => a + r.Count);

    public override bool GetBoolean(int ordinal)
        => (bool)_currentRow[ordinal];

    public override byte GetByte(int ordinal)
        => (byte)_currentRow[ordinal];

    public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        => throw new NotImplementedException();

    public override char GetChar(int ordinal)
        => (char)_currentRow[ordinal];

    public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        => throw new NotImplementedException();

    public override string GetDataTypeName(int ordinal)
        => GetFieldType(ordinal).Name;

    public override DateTime GetDateTime(int ordinal)
        => (DateTime)_currentRow[ordinal];

    public override decimal GetDecimal(int ordinal)
        => (decimal)_currentRow[ordinal];

    public override double GetDouble(int ordinal)
        => (double)_currentRow[ordinal];

    public override IEnumerator GetEnumerator()
        => throw new NotImplementedException();

    public override Type GetFieldType(int ordinal)
        => _results.Count > 0
            ? _results[0][ordinal]?.GetType() ?? typeof(object)
            : typeof(object);

    public override float GetFloat(int ordinal)
        => (float)_currentRow[ordinal];

    public override Guid GetGuid(int ordinal)
        => (Guid)_currentRow[ordinal];

    public override short GetInt16(int ordinal)
        => (short)_currentRow[ordinal];

    public override long GetInt64(int ordinal)
        => (long)_currentRow[ordinal];

    public override int GetOrdinal(string name)
        => throw new NotImplementedException();

    public override string GetString(int ordinal)
        => (string)_currentRow[ordinal];

    public override int GetValues(object[] values)
        => throw new NotImplementedException();

    public override bool NextResult()
    {
        var hasResult = _resultSets.Count > ++_currentResultSet;
        if (hasResult)
        {
            _results = _resultSets[_currentResultSet];
        }

        return hasResult;
    }
}
