// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.Sqlite.Properties;
using SQLitePCL;

namespace Microsoft.Data.Sqlite
{
    /// <summary>
    ///     Represents a collection of SQLite parameters.
    /// </summary>
    /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
    public class SqliteParameterCollection : DbParameterCollection
    {
        private readonly List<SqliteParameter> _parameters = [];

        /// <summary>
        ///     Initializes a new instance of the <see cref="SqliteParameterCollection" /> class.
        /// </summary>
        protected internal SqliteParameterCollection()
        {
        }

        /// <summary>
        ///     Gets the number of items in the collection.
        /// </summary>
        /// <value>The number of items in the collection.</value>
        public override int Count
            => _parameters.Count;

        /// <summary>
        ///     Gets the object used to synchronize access to the collection.
        /// </summary>
        /// <value>The object used to synchronize access to the collection.</value>
        public override object SyncRoot
            => ((ICollection)_parameters).SyncRoot;

        /// <summary>
        ///     Gets or sets the parameter at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the parameter.</param>
        /// <returns>The parameter.</returns>
        public new virtual SqliteParameter this[int index]
        {
            get => _parameters[index];
            set
            {
                if (_parameters[index] == value)
                {
                    return;
                }

                _parameters[index] = value;
            }
        }

        /// <summary>
        ///     Gets or sets the parameter with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>The parameter.</returns>
        public new virtual SqliteParameter this[string parameterName]
        {
            get => this[IndexOfChecked(parameterName)];
            set => this[IndexOfChecked(parameterName)] = value;
        }

        /// <summary>
        ///     Adds a parameter to the collection.
        /// </summary>
        /// <param name="value">The parameter to add. Must be a <see cref="SqliteParameter" />.</param>
        /// <returns>The zero-based index of the parameter that was added.</returns>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
        public override int Add(object value)
        {
            _parameters.Add((SqliteParameter)value);

            return Count - 1;
        }

        /// <summary>
        ///     Adds a parameter to the collection.
        /// </summary>
        /// <param name="value">The parameter to add.</param>
        /// <returns>The parameter that was added.</returns>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
        public virtual SqliteParameter Add(SqliteParameter value)
        {
            _parameters.Add(value);

            return value;
        }

        /// <summary>
        ///     Adds a parameter to the collection.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="type">The SQLite type of the parameter.</param>
        /// <returns>The parameter that was added.</returns>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
        public virtual SqliteParameter Add(string? parameterName, SqliteType type)
            => Add(new SqliteParameter(parameterName, type));

        /// <summary>
        ///     Adds a parameter to the collection.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="type">The SQLite type of the parameter.</param>
        /// <param name="size">The maximum size, in bytes, of the parameter.</param>
        /// <returns>The parameter that was added.</returns>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
        public virtual SqliteParameter Add(string? parameterName, SqliteType type, int size)
            => Add(new SqliteParameter(parameterName, type, size));

        /// <summary>
        ///     Adds a parameter to the collection.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="type">The SQLite type of the parameter.</param>
        /// <param name="size">The maximum size, in bytes, of the parameter.</param>
        /// <param name="sourceColumn">
        ///     The source column used for loading the value of the parameter. Can be null.
        /// </param>
        /// <returns>The parameter that was added.</returns>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
        public virtual SqliteParameter Add(string? parameterName, SqliteType type, int size, string? sourceColumn)
            => Add(new SqliteParameter(parameterName, type, size, sourceColumn));

        /// <summary>
        ///     Adds multiple parameters to the collection.
        /// </summary>
        /// <param name="values">
        ///     An array of parameters to add. They must be <see cref="SqliteParameter" /> objects.
        /// </param>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
        public override void AddRange(Array values)
            => AddRange(values.Cast<SqliteParameter>());

        /// <summary>
        ///     Adds multiple parameters to the collection.
        /// </summary>
        /// <param name="values">The parameters to add.</param>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
        public virtual void AddRange(IEnumerable<SqliteParameter> values)
            => _parameters.AddRange(values);

        /// <summary>
        ///     Adds a parameter to the collection.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <param name="value">The value of the parameter. Can be null.</param>
        /// <returns>The parameter that was added.</returns>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/parameters">Parameters</seealso>
        /// <seealso href="https://docs.microsoft.com/dotnet/standard/data/sqlite/types">Data Types</seealso>
        public virtual SqliteParameter AddWithValue(string? parameterName, object? value)
            => Add(new SqliteParameter(parameterName, value));

        /// <summary>
        ///     Removes all parameters from the collection.
        /// </summary>
        public override void Clear()
            => _parameters.Clear();

        /// <summary>
        ///     Gets a value indicating whether the collection contains the specified parameter.
        /// </summary>
        /// <param name="value">The parameter to look for. Must be a <see cref="SqliteParameter" />.</param>
        /// <returns><see langword="true" /> if the collection contains the parameter; otherwise, <see langword="false" />.</returns>
        public override bool Contains(object value)
            => Contains((SqliteParameter)value);

        /// <summary>
        ///     Gets a value indicating whether the collection contains the specified parameter.
        /// </summary>
        /// <param name="value">The parameter to look for.</param>
        /// <returns><see langword="true" /> if the collection contains the parameter; otherwise, <see langword="false" />.</returns>
        public virtual bool Contains(SqliteParameter value)
            => _parameters.Contains(value);

        /// <summary>
        ///     Gets a value indicating whether the collection contains a parameter with the specified name.
        /// </summary>
        /// <param name="value">The name of the parameter.</param>
        /// <returns><see langword="true" /> if the collection contains the parameter; otherwise, <see langword="false" />.</returns>
        public override bool Contains(string value)
            => IndexOf(value) != -1;

        /// <summary>
        ///     Copies the collection to an array of parameters.
        /// </summary>
        /// <param name="array">
        ///     The array into which the parameters are copied. Must be an array of <see cref="SqliteParameter" /> objects.
        /// </param>
        /// <param name="index">The zero-based index to which the parameters are copied.</param>
        public override void CopyTo(Array array, int index)
            => CopyTo((SqliteParameter[])array, index);

        /// <summary>
        ///     Copies the collection to an array of parameters.
        /// </summary>
        /// <param name="array">The array into which the parameters are copied.</param>
        /// <param name="index">The zero-based index to which the parameters are copied.</param>
        public virtual void CopyTo(SqliteParameter[] array, int index)
            => _parameters.CopyTo(array, index);

        /// <summary>
        ///     Gets an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public override IEnumerator GetEnumerator()
            => _parameters.GetEnumerator();

        /// <summary>
        ///     Gets a parameter at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the parameter.</param>
        /// <returns>The parameter.</returns>
        protected override DbParameter GetParameter(int index)
            => this[index];

        /// <summary>
        ///     Gets a parameter with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>The parameter.</returns>
        protected override DbParameter GetParameter(string parameterName)
            => GetParameter(IndexOfChecked(parameterName));

        /// <summary>
        ///     Gets the index of the specified parameter.
        /// </summary>
        /// <param name="value">The parameter. Must be a <see cref="SqliteParameter" />.</param>
        /// <returns>The zero-based index of the parameter.</returns>
        public override int IndexOf(object value)
            => IndexOf((SqliteParameter)value);

        /// <summary>
        ///     Gets the index of the specified parameter.
        /// </summary>
        /// <param name="value">The parameter.</param>
        /// <returns>The zero-based index of the parameter.</returns>
        public virtual int IndexOf(SqliteParameter value)
            => _parameters.IndexOf(value);

        /// <summary>
        ///     Gets the index of the parameter with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>The zero-based index of the parameter or -1 if not found.</returns>
        public override int IndexOf(string parameterName)
        {
            for (var index = 0; index < _parameters.Count; index++)
            {
                if (_parameters[index].ParameterName == parameterName)
                {
                    return index;
                }
            }

            return -1;
        }

        /// <summary>
        ///     Inserts a parameter into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the parameter should be inserted.</param>
        /// <param name="value">The parameter to insert. Must be a <see cref="SqliteParameter" />.</param>
        public override void Insert(int index, object value)
            => Insert(index, (SqliteParameter)value);

        /// <summary>
        ///     Inserts a parameter into the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which the parameter should be inserted.</param>
        /// <param name="value">The parameter to insert.</param>
        public virtual void Insert(int index, SqliteParameter value)
            => _parameters.Insert(index, value);

        /// <summary>
        ///     Removes a parameter from the collection.
        /// </summary>
        /// <param name="value">The parameter to remove. Must be a <see cref="SqliteParameter" />.</param>
        public override void Remove(object value)
            => Remove((SqliteParameter)value);

        /// <summary>
        ///     Removes a parameter from the collection.
        /// </summary>
        /// <param name="value">The parameter to remove.</param>
        public virtual void Remove(SqliteParameter value)
            => _parameters.Remove(value);

        /// <summary>
        ///     Removes a parameter from the collection at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the parameter to remove.</param>
        public override void RemoveAt(int index)
            => _parameters.RemoveAt(index);

        /// <summary>
        ///     Removes a parameter with the specified name from the collection.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to remove.</param>
        public override void RemoveAt(string parameterName)
            => RemoveAt(IndexOfChecked(parameterName));

        /// <summary>
        ///     Sets the parameter at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the parameter to set.</param>
        /// <param name="value">The parameter. Must be a <see cref="SqliteParameter" />.</param>
        protected override void SetParameter(int index, DbParameter value)
            => this[index] = (SqliteParameter)value;

        /// <summary>
        ///     Sets the parameter with the specified name.
        /// </summary>
        /// <param name="parameterName">The name of the parameter to set.</param>
        /// <param name="value">The parameter. Must be a <see cref="SqliteParameter" />.</param>
        protected override void SetParameter(string parameterName, DbParameter value)
            => SetParameter(IndexOfChecked(parameterName), value);

        internal int Bind(sqlite3_stmt stmt, sqlite3 handle)
        {
            var bound = 0;
            foreach (var parameter in _parameters)
            {
                if (parameter.Bind(stmt, handle))
                {
                    bound++;
                }
            }

            return bound;
        }

        private int IndexOfChecked(string parameterName)
        {
            var index = IndexOf(parameterName);
            if (index == -1)
            {
                throw new IndexOutOfRangeException(Resources.ParameterNotFound(parameterName));
            }

            return index;
        }
    }
}
