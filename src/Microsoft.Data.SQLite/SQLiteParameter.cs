// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.Data.SQLite.Interop;
using Microsoft.Data.SQLite.Utilities;

namespace Microsoft.Data.SQLite
{
    public class SQLiteParameter : DbParameter
    {
        private bool _bound;
        private string _parameterName;
        private object _value;

        public SQLiteParameter()
        {
        }

        public SQLiteParameter([NotNull] string parameterName, [NotNull] object value)
            : this()
        {
            Check.NotEmpty(parameterName, "parameterName");
            Check.NotNull(value, "value");

            _parameterName = parameterName;
            _value = value;
        }

        // TODO: Implement this
        public override DbType DbType { get; set; }

        public override ParameterDirection Direction
        {
            get { return ParameterDirection.Input; }
            set
            {
                if (value != ParameterDirection.Input)
                {
                    throw new ArgumentException(Strings.FormatInvalidParameterDirection(value));
                }
            }
        }

        public override bool IsNullable { get; set; }

        public override string ParameterName
        {
            get { return _parameterName; }
            [param: NotNull]
            set
            {
                Check.NotEmpty(value, "value");
                if (_parameterName == value)
                {
                    return;
                }

                _parameterName = value;
                _bound = false;
            }
        }

        public override int Size { get; set; }
        public override string SourceColumn { get; set; }
        public override bool SourceColumnNullMapping { get; set; }

        public override object Value
        {
            get { return _value; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, "value");

                // NOTE: Using Equals here avoids reference comparison
                if (value.Equals(_value))
                {
                    return;
                }

                _value = value;
                _bound = false;
            }
        }

        internal bool Bound
        {
            get { return _bound; }
        }

        internal SQLiteParameterCollection Parent { get; set; }

        public override void ResetDbType()
        {
            throw new NotSupportedException();
        }

        internal void Bind(IEnumerable<StatementHandle> handles)
        {
            Debug.Assert(handles != null, "handles is null.");
            if (_parameterName == null)
            {
                throw new InvalidOperationException(Strings.FormatRequiresSet("ParameterName"));
            }
            if (_value == null)
            {
                throw new InvalidOperationException(Strings.FormatRequiresSet("Value"));
            }

            foreach (var handle in handles)
            {
                var index = NativeMethods.sqlite3_bind_parameter_index(handle, _parameterName);
                if (index != 0)
                {
                    var typeMap = SQLiteTypeMap.FromClrType(_value.GetType());
                    switch (typeMap.SQLiteType)
                    {
                        case SQLiteType.Integer:
                            var rc = NativeMethods.sqlite3_bind_int64(handle, index, (long)typeMap.ToInterop(_value));
                            MarshalEx.ThrowExceptionForRC(rc);
                            break;

                        case SQLiteType.Float:
                            rc = NativeMethods.sqlite3_bind_double(handle, index, (double)typeMap.ToInterop(_value));
                            MarshalEx.ThrowExceptionForRC(rc);
                            break;

                        case SQLiteType.Text:
                            rc = NativeMethods.sqlite3_bind_text(handle, index, (string)typeMap.ToInterop(_value));
                            MarshalEx.ThrowExceptionForRC(rc);
                            break;

                        case SQLiteType.Blob:
                            rc = NativeMethods.sqlite3_bind_blob(handle, index, (byte[])typeMap.ToInterop(_value));
                            MarshalEx.ThrowExceptionForRC(rc);
                            break;

                        case SQLiteType.Null:
                            rc = NativeMethods.sqlite3_bind_null(handle, index);
                            MarshalEx.ThrowExceptionForRC(rc);
                            break;

                        default:
                            Debug.Assert(false, "Unexpected value.");
                            break;
                    }
                }
            }

            _bound = true;
        }
    }
}
