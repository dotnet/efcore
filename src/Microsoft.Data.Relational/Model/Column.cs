// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational.Model
{
    public class Column
    {
        private Table _table;
        private readonly string _name;
        private string _dataType;

        public Column([NotNull] string name, [NotNull] string dataType)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(dataType, "dataType");

            _name = name;
            _dataType = dataType;
        }

        public virtual Table Table
        {
            get { return _table; }

            [param: CanBeNull]
            internal set
            {
                Contract.Assert((value == null) != (_table == null));
                _table = value;
            }
        }

        public virtual string Name
        {
            get { return _name; }

            [param: NotNull]
            set
            {
                Check.NotEmpty(value, "value");

                _dataType = value;
            }
        }

        public virtual string DataType
        {
            get { return _dataType; }

            [param: NotNull]
            set
            {
                Check.NotEmpty(value, "value");

                _dataType = value;
            }
        }

        public virtual bool IsNullable { get; set; }

        public virtual object DefaultValue { get; [param: CanBeNull] set; }
    }
}
