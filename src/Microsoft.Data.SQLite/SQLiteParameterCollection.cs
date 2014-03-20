// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Microsoft.Data.SQLite.Interop;

namespace Microsoft.Data.SQLite
{
    public class SQLiteParameterCollection : DbParameterCollection
    {
        private readonly List<SQLiteParameter> _parameters = new List<SQLiteParameter>();
        private bool _bound;

        internal SQLiteParameterCollection()
        {
        }

        public override int Count
        {
            get { return _parameters.Count; }
        }

        public override object SyncRoot
        {
            get { return ((ICollection)_parameters).SyncRoot; }
        }

        internal bool Bound
        {
            get { return _bound && _parameters.All(p => p.Bound); }
        }

        public new SQLiteParameter this[int index]
        {
            get { return _parameters[index]; }
            set
            {
                var current = _parameters[index];

                if (current == value)
                    return;

                Validate(index, value);
                _bound = false;
                _parameters[index] = value;
                current.Parent = null;
            }
        }

        public new SQLiteParameter this[string parameterName]
        {
            get { return this[IndexOfChecked(parameterName)]; }
            set { this[IndexOfChecked(parameterName)] = value; }
        }

        public override int Add(object value)
        {
            var parameter = (SQLiteParameter)value;
            Validate(-1, parameter);
            _bound = false;
            _parameters.Add(parameter);

            return Count - 1;
        }

        public override void AddRange(Array values)
        {
            foreach (var value in values)
                Add(value);
        }

        public SQLiteParameter AddWithValue(string parameterName, object value)
        {
            var parameter = new SQLiteParameter(parameterName, value);
            Add(parameter);

            return parameter;
        }

        public override void Clear()
        {
            _bound = false;

            foreach (var parameter in _parameters)
                parameter.Parent = null;

            _parameters.Clear();
        }

        public override bool Contains(object value)
        {
            return IndexOf(value) != -1;
        }

        public override bool Contains(string parameterName)
        {
            return IndexOf(parameterName) != -1;
        }

        public override void CopyTo(Array array, int index)
        {
            throw new NotSupportedException();
        }

        public override IEnumerator GetEnumerator()
        {
            return _parameters.GetEnumerator();
        }

        protected override DbParameter GetParameter(int index)
        {
            return this[index];
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            return GetParameter(IndexOfChecked(parameterName));
        }

        public override int IndexOf(object value)
        {
            return _parameters.IndexOf((SQLiteParameter)value);
        }

        public override int IndexOf(string parameterName)
        {
            for (var index = 0; index < _parameters.Count; index++)
                if (_parameters[index].ParameterName == parameterName)
                    return index;

            return -1;
        }

        public override void Insert(int index, object value)
        {
            var parameter = (SQLiteParameter)value;
            Validate(-1, parameter);
            _bound = false;
            _parameters.Insert(index, parameter);
        }

        public override void Remove(object value)
        {
            var parameter = (SQLiteParameter)value;
            if (_parameters.Remove(parameter))
            {
                _bound = false;
                parameter.Parent = null;
            }
        }

        public override void RemoveAt(int index)
        {
            var current = _parameters[index];
            _parameters.RemoveAt(index);
            _bound = false;
            current.Parent = null;
        }

        public override void RemoveAt(string parameterName)
        {
            RemoveAt(IndexOfChecked(parameterName));
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            this[index] = (SQLiteParameter)value;
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            SetParameter(IndexOfChecked(parameterName), value);
        }

        internal void Bind(StatementHandle handle)
        {
            foreach (var parameter in _parameters)
                parameter.Bind(handle);

            _bound = true;
        }

        private int IndexOfChecked(string parameterName)
        {
            var index = IndexOf(parameterName);
            if (index == -1)
                throw new IndexOutOfRangeException(Strings.FormatParameterNotFound(parameterName));

            return index;
        }

        private void Validate(int index, SQLiteParameter parameter)
        {
            if (parameter.Parent != null)
            {
                if (parameter.Parent != this)
                    throw new ArgumentException(Strings.CollectionIsNotParent);
                if (IndexOf(parameter) != index)
                    throw new ArgumentException(Strings.CollectionIsParent);
            }

            parameter.Parent = this;
        }
    }
}
