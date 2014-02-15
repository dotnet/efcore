// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations.Utilities;
using Microsoft.Data.Relational;

namespace Microsoft.Data.Migrations.Model
{
    public class CreateSequenceOperation : MigrationOperation
    {
        private readonly SchemaQualifiedName _schemaQualifiedName;

        private int _incrementBy = 1;
        private string _dataType = "BIGINT";

        public CreateSequenceOperation(SchemaQualifiedName schemaQualifiedName)
        {
            _schemaQualifiedName = schemaQualifiedName;
        }

        public virtual SchemaQualifiedName SchemaQualifiedName
        {
            get { return _schemaQualifiedName; }
        }

        public virtual int StartWith { get; set; }

        public virtual int IncrementBy
        {
            get { return _incrementBy; }
            set { _incrementBy = value; }
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

        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
