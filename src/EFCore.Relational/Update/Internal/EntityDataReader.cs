// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#pragma warning disable CS1591
#pragma warning disable EF1001

using System.Collections;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public sealed class EntityDataReader<TEntity> : DbDataReader
    where TEntity : class
{
    private readonly IEnumerator<TEntity>? _syncEnumerator;
    private readonly IAsyncEnumerator<TEntity>? _asyncEnumerator;
    private readonly PropertyColumnMapping[] _mappings;
    private readonly Dictionary<string, int> _ordinals;
    private readonly object?[] _rowValues;
    private readonly bool[] _rowValuesComputed;
    private TEntity? _current;
    private bool _isClosed;

    public readonly record struct PropertyColumnMapping(
        IProperty Property,
        string ColumnName,
        Type ClrType,
        RelationalTypeMapping TypeMapping,
        Func<object, object?> ValueAccessor,
        ValueConverter? Converter);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EntityDataReader(
        IEntityType entityType,
        IEnumerable<TEntity> source,
        StoreObjectIdentifier storeObject,
        bool keepIdentity,
        Func<IProperty, bool>? isStoreGeneratedIdentityColumn = null)
    {
        _syncEnumerator = source.GetEnumerator();
        _mappings = CreateMappings(entityType, storeObject, keepIdentity, isStoreGeneratedIdentityColumn);
        _rowValues = new object?[_mappings.Length];
        _rowValuesComputed = new bool[_mappings.Length];
        _ordinals = _mappings.Select((m, i) => (m.ColumnName, Index: i))
            .ToDictionary(x => x.ColumnName, x => x.Index, StringComparer.Ordinal);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public EntityDataReader(
        IEntityType entityType,
        IAsyncEnumerable<TEntity> source,
        StoreObjectIdentifier storeObject,
        bool keepIdentity,
        Func<IProperty, bool>? isStoreGeneratedIdentityColumn = null,
        CancellationToken cancellationToken = default)
    {
        _asyncEnumerator = source.GetAsyncEnumerator(cancellationToken);
        _mappings = CreateMappings(entityType, storeObject, keepIdentity, isStoreGeneratedIdentityColumn);
        _rowValues = new object?[_mappings.Length];
        _rowValuesComputed = new bool[_mappings.Length];
        _ordinals = _mappings.Select((m, i) => (m.ColumnName, Index: i))
            .ToDictionary(x => x.ColumnName, x => x.Index, StringComparer.Ordinal);
    }

    private static PropertyColumnMapping[] CreateMappings(
        IEntityType entityType,
        StoreObjectIdentifier storeObject,
        bool keepIdentity,
        Func<IProperty, bool>? isStoreGeneratedIdentityColumn)
    {
        var list = new List<PropertyColumnMapping>();
        var discriminatorProperty = entityType.FindDiscriminatorProperty();
        var discriminatorValue = entityType.GetDiscriminatorValue();

        foreach (var property in entityType.GetFlattenedProperties())
        {
            var columnName = property.GetColumnName(storeObject);
            if (columnName is null)
            {
                continue;
            }

            // Exclude computed columns or columns marked to ignore before save
            if (property.GetComputedColumnSql() is not null
                || property.GetBeforeSaveBehavior() == PropertySaveBehavior.Ignore
                || property.ValueGenerated == ValueGenerated.OnAddOrUpdate)
            {
                continue;
            }

            // Exclude store-generated identity primary keys unless keepIdentity is requested
            var isIdentity = isStoreGeneratedIdentityColumn?.Invoke(property)
                ?? (property.IsPrimaryKey()
                    && property.ValueGenerated == ValueGenerated.OnAdd
                    && property.ClrType != typeof(Guid)
                    && property.GetValueGeneratorFactory() == null);

            if (isIdentity && !keepIdentity)
            {
                continue;
            }

            Func<object, object?> valueAccessor;
            if (property == discriminatorProperty)
            {
                valueAccessor = _ => discriminatorValue;
            }
            else if (property.GetGetter() is { } clrGetter)
            {
                valueAccessor = entity =>
                {
                    try
                    {
                        return clrGetter.GetClrValueUsingContainingEntity(entity);
                    }
                    catch (NullReferenceException)
                    {
                        return null;
                    }
                };
            }
            else
            {
                valueAccessor = _ => null;
            }

            var typeMapping = (RelationalTypeMapping)property.GetTypeMapping();
            var converter = typeMapping.Converter ?? property.GetValueConverter();
            var targetType = converter?.ProviderClrType ?? property.ClrType;
            var unwrapped = Nullable.GetUnderlyingType(targetType) ?? targetType;

            list.Add(new PropertyColumnMapping(property, columnName, unwrapped, typeMapping, valueAccessor, converter));
        }

        return list.ToArray();
    }

    /// <summary>
    ///     The list of mapped database column names.
    /// </summary>
    public IReadOnlyList<string> ColumnNames
        => _mappings.Select(m => m.ColumnName).ToArray();

    /// <summary>
    ///     The mapped column definitions.
    /// </summary>
    public IReadOnlyList<PropertyColumnMapping> ColumnMappings
        => _mappings;

    public override int FieldCount
        => _mappings.Length;

    public override bool HasRows
        => true;

    public override bool IsClosed
        => _isClosed;

    public override int RecordsAffected
        => -1;

    public override int Depth
        => 0;

    public override object this[int ordinal]
        => GetValue(ordinal);

    public override object this[string name]
        => GetValue(GetOrdinal(name));

    public override string GetName(int ordinal)
        => _mappings[ordinal].ColumnName;

    public override int GetOrdinal(string name)
    {
        if (_ordinals.TryGetValue(name, out var index))
        {
            return index;
        }

        // Fallback for case-insensitive match if exact ordinal not found
        for (var i = 0; i < _mappings.Length; i++)
        {
            if (string.Equals(_mappings[i].ColumnName, name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        throw new IndexOutOfRangeException($"Column '{name}' not found.");
    }

    public override Type GetFieldType(int ordinal)
        => _mappings[ordinal].ClrType;

    public override string GetDataTypeName(int ordinal)
        => _mappings[ordinal].TypeMapping.StoreType;

    public override object GetValue(int ordinal)
    {
        if (_current is null)
        {
            throw new InvalidOperationException("No current row available. Call Read or ReadAsync first.");
        }

        if (!_rowValuesComputed[ordinal])
        {
            var mapping = _mappings[ordinal];
            var val = mapping.ValueAccessor(_current);
            if (val is null)
            {
                _rowValues[ordinal] = DBNull.Value;
            }
            else
            {
                if (mapping.Converter != null)
                {
                    val = mapping.Converter.ConvertToProvider(val);
                }

                _rowValues[ordinal] = val ?? DBNull.Value;
            }

            _rowValuesComputed[ordinal] = true;
        }

        return _rowValues[ordinal]!;
    }

    public override int GetValues(object[] values)
    {
        var count = Math.Min(values.Length, _mappings.Length);
        for (var i = 0; i < count; i++)
        {
            values[i] = GetValue(i);
        }

        return count;
    }

    public override bool IsDBNull(int ordinal)
        => GetValue(ordinal) is DBNull;

    public override bool GetBoolean(int ordinal)
        => (bool)GetValue(ordinal);

    public override byte GetByte(int ordinal)
        => (byte)GetValue(ordinal);

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
        var bytes = (byte[])GetValue(ordinal);
        if (buffer is null)
        {
            return bytes.Length;
        }

        var available = Math.Max(0, bytes.Length - (int)dataOffset);
        var toCopy = Math.Min(available, length);
        Array.Copy(bytes, dataOffset, buffer, bufferOffset, toCopy);
        return toCopy;
    }

    public override char GetChar(int ordinal)
        => (char)GetValue(ordinal);

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
        var chars = ((string)GetValue(ordinal)).ToCharArray();
        if (buffer is null)
        {
            return chars.Length;
        }

        var available = Math.Max(0, chars.Length - (int)dataOffset);
        var toCopy = Math.Min(available, length);
        Array.Copy(chars, dataOffset, buffer, bufferOffset, toCopy);
        return toCopy;
    }

    public override DateTime GetDateTime(int ordinal)
        => (DateTime)GetValue(ordinal);

    public override decimal GetDecimal(int ordinal)
        => (decimal)GetValue(ordinal);

    public override double GetDouble(int ordinal)
        => (double)GetValue(ordinal);

    public override float GetFloat(int ordinal)
        => (float)GetValue(ordinal);

    public override Guid GetGuid(int ordinal)
        => (Guid)GetValue(ordinal);

    public override short GetInt16(int ordinal)
        => (short)GetValue(ordinal);

    public override int GetInt32(int ordinal)
        => (int)GetValue(ordinal);

    public override long GetInt64(int ordinal)
        => (long)GetValue(ordinal);

    public override string GetString(int ordinal)
        => (string)GetValue(ordinal);

    public override IEnumerator GetEnumerator()
        => new DbEnumerator(this, closeReader: false);

    public override bool NextResult()
        => false;

    public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
        => Task.FromResult(false);

    public override bool Read()
    {
        Array.Clear(_rowValuesComputed, 0, _rowValuesComputed.Length);
        if (_syncEnumerator is not null)
        {
            if (_syncEnumerator.MoveNext())
            {
                _current = _syncEnumerator.Current;
                return true;
            }
        }
        else if (_asyncEnumerator is not null)
        {
            if (_asyncEnumerator.MoveNextAsync().AsTask().GetAwaiter().GetResult())
            {
                _current = _asyncEnumerator.Current;
                return true;
            }
        }

        _current = null;
        return false;
    }

    public override async Task<bool> ReadAsync(CancellationToken cancellationToken)
    {
        Array.Clear(_rowValuesComputed, 0, _rowValuesComputed.Length);
        if (_asyncEnumerator is not null)
        {
            if (await _asyncEnumerator.MoveNextAsync().ConfigureAwait(false))
            {
                _current = _asyncEnumerator.Current;
                return true;
            }
        }
        else if (_syncEnumerator is not null)
        {
            if (_syncEnumerator.MoveNext())
            {
                _current = _syncEnumerator.Current;
                return true;
            }
        }

        _current = null;
        return false;
    }

    public override void Close()
    {
        _isClosed = true;
        _syncEnumerator?.Dispose();
        _current = null;
    }

    public override async ValueTask DisposeAsync()
    {
        _isClosed = true;
        _syncEnumerator?.Dispose();
        if (_asyncEnumerator is not null)
        {
            await _asyncEnumerator.DisposeAsync().ConfigureAwait(false);
        }

        _current = null;
        await base.DisposeAsync().ConfigureAwait(false);
    }
}
