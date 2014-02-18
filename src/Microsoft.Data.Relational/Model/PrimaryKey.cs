// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.Data.Relational.Model
{
    public class PrimaryKey
    {
        private readonly SchemaQualifiedName _name;
        private readonly List<Column> _columns = new List<Column>();

        public PrimaryKey(SchemaQualifiedName name)
        {
            _name = name;
        }

        public virtual SchemaQualifiedName Name
        {
            get { return _name; }
        }

        public virtual bool IsClustered { get; set; }

        public virtual List<Column> Columns
        {
            get { return _columns; }
        }
    }
}
