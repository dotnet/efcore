// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.Sqlite.Interop;

namespace Microsoft.Data.Sqlite
{
    public class SqliteParameterCollection : DbParameterCollection
    {
        private readonly List<SqliteParameter> _parameters = new List<SqliteParameter>();

        protected internal SqliteParameterCollection()
        {
        }

        public override int Count => _parameters.Count;
        public override object SyncRoot => ((ICollection)_parameters).SyncRoot;

        public virtual new SqliteParameter this[int index]
        {
            get { return _parameters[index]; }
            set
            {
                var current = _parameters[index];

                if (current == value)
                {
                    return;
                }

                _parameters[index] = value;
            }
        }

        public virtual new SqliteParameter this[string parameterName]
        {
            get { return this[IndexOfChecked(parameterName)]; }
            set { this[IndexOfChecked(parameterName)] = value; }
        }

        public override int Add(object value)
        {
            _parameters.Add((SqliteParameter)value);

            return Count - 1;
        }

        public SqliteParameter Add(SqliteParameter value)
        {
            _parameters.Add(value);

            return value;
        }

        public SqliteParameter Add(string parameterName, SqliteType type) =>
            Add(new SqliteParameter(parameterName, type));
        public SqliteParameter Add(string parameterName, SqliteType type, int size) =>
            Add(new SqliteParameter(parameterName, type, size));
        public SqliteParameter Add(string parameterName, SqliteType type, int size, string sourceColumn) =>
            Add(new SqliteParameter(parameterName, type, size, sourceColumn));
        public override void AddRange(Array values) => Add(values.Cast<SqliteParameter>());
        public virtual void AddRange(IEnumerable<SqliteParameter> values) => _parameters.AddRange(values);

        public virtual SqliteParameter AddWithValue(string parameterName, object value)
        {
            var parameter = new SqliteParameter(parameterName, value);
            Add(parameter);

            return parameter;
        }

        public override void Clear() => _parameters.Clear();
        public override bool Contains(object value) => Contains((SqliteParameter)value);
        public virtual bool Contains(SqliteParameter value) => _parameters.Contains(value);
        public override bool Contains(string value) => IndexOf(value) != -1;
        public override void CopyTo(Array array, int index) => CopyTo((SqliteParameter[])array, index);
        public virtual void CopyTo(SqliteParameter[] array, int index) => _parameters.CopyTo(array, index);
        public override IEnumerator GetEnumerator() => _parameters.GetEnumerator();
        protected override DbParameter GetParameter(int index) => this[index];
        protected override DbParameter GetParameter(string parameterName) => GetParameter(IndexOfChecked(parameterName));
        public override int IndexOf(object value) => IndexOf((SqliteParameter)value);
        public virtual int IndexOf(SqliteParameter value) => _parameters.IndexOf(value);

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

        public override void Insert(int index, object value) => Insert(index, (SqliteParameter)value);
        public virtual void Insert(int index, SqliteParameter value) => _parameters.Insert(index, value);
        public override void Remove(object value) => Remove((SqliteParameter)value);
        public virtual void Remove(SqliteParameter value) => _parameters.Remove(value);
        public override void RemoveAt(int index) => _parameters.RemoveAt(index);
        public override void RemoveAt(string parameterName) => RemoveAt(IndexOfChecked(parameterName));
        protected override void SetParameter(int index, DbParameter value) => this[index] = (SqliteParameter)value;
        protected override void SetParameter(string parameterName, DbParameter value) => SetParameter(IndexOfChecked(parameterName), value);

        internal void Bind(Sqlite3StmtHandle stmt)
        {
            foreach (var parameter in _parameters)
            {
                parameter.Bind(stmt);
            }
        }

        private int IndexOfChecked(string parameterName)
        {
            var index = IndexOf(parameterName);
            if (index == -1)
            {
                throw new IndexOutOfRangeException(Strings.FormatParameterNotFound(parameterName));
            }

            return index;
        }
    }
}
