// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System.Diagnostics.Contracts;
using JetBrains.Annotations;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational.Model
{
    public class Sequence
    {
        private Database _database;
        private readonly SchemaQualifiedName _name;
        private int _incrementBy = 1;
        private string _dataType = "BIGINT";

        public Sequence(SchemaQualifiedName name)
        {
            _name = name;
        }

        public Sequence(SchemaQualifiedName name, [NotNull] string dataType, int startWith, int incrementBy)
        {
            Check.NotEmpty(dataType, "dataType");

            _name = name;
            _dataType = dataType;
            StartWith = startWith;
            _incrementBy = incrementBy;
        }

        public virtual Database Database
        {
            get { return _database; }

            [param: CanBeNull]
            internal set
            {
                Contract.Assert((value == null) != (_database == null));
                _database = value;
            }
        }

        public virtual SchemaQualifiedName Name
        {
            get { return _name; }
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
    }
}
