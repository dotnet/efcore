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

using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Relational.Utilities;

namespace Microsoft.Data.Relational
{
    public class RelationalTypedValueReader : IValueReader
    {
        private readonly DbDataReader _dataReader;

        public RelationalTypedValueReader([NotNull] DbDataReader dataReader)
        {
            Check.NotNull(dataReader, "dataReader");

            _dataReader = dataReader;
        }

        public virtual bool IsNull(int index)
        {
            return _dataReader.IsDBNull(index);
        }

        public virtual T ReadValue<T>(int index)
        {
            return _dataReader.GetFieldValue<T>(index);
        }

        public virtual int Count
        {
            get { return _dataReader.FieldCount; }
        }
    }
}
