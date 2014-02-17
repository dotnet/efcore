// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational.Model
{
    public class Column
    {
        private readonly SchemaQualifiedName _name;
        private string _dataType;

        public Column(SchemaQualifiedName name, [NotNull] string dataType)
        {
            Check.NotEmpty(dataType, "dataType");

            _name = name;
            _dataType = dataType;
        }

        public virtual SchemaQualifiedName Name
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
