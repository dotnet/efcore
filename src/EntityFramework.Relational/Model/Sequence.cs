// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Model
{
    // TODO: Consider adding more validation.
    public class Sequence
    {
        private SchemaQualifiedName _name;
        private string _dataType;
        private long _startWith;
        private int _incrementBy;

        public Sequence(SchemaQualifiedName name, [NotNull] string dataType, long startWith, int incrementBy)
        {
            Check.NotEmpty(dataType, "dataType");

            _name = name;
            _dataType = dataType;
            _startWith = startWith;
            _incrementBy = incrementBy;
        }

        public virtual SchemaQualifiedName Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual long StartWith
        {
            get { return _startWith; }
            set { _startWith = value; }
        }

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

        public virtual Sequence Clone([NotNull] CloneContext cloneContext)
        {
            Check.NotNull(cloneContext, "cloneContext");

            return new Sequence(Name, DataType, StartWith, IncrementBy);
        }
    }
}
