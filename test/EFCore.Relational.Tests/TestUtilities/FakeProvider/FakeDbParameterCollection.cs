// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider
{
    public class FakeDbParameterCollection : DbParameterCollection
    {
        private readonly List<object> _parameters = new List<object>();

        public override int Count
            => _parameters.Count;

        public override int Add(object value)
        {
            _parameters.Add(value);

            return _parameters.Count - 1;
        }

        protected override DbParameter GetParameter(int index)
            => (DbParameter)_parameters[index];

        public override IEnumerator GetEnumerator()
            => _parameters.GetEnumerator();

        public override object SyncRoot
            => throw new NotImplementedException();

        public override void AddRange(Array values)
            => throw new NotImplementedException();

        public override void Clear()
        {
            // no-op to test that parameters are passed correctly to db command.
        }

        public override bool Contains(string value)
        {
            throw new NotImplementedException();
        }

        public override bool Contains(object value)
        {
            throw new NotImplementedException();
        }

        public override void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public override int IndexOf(string parameterName)
        {
            throw new NotImplementedException();
        }

        public override int IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        public override void Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        public override void Remove(object value)
        {
            throw new NotImplementedException();
        }

        public override void RemoveAt(string parameterName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        protected override DbParameter GetParameter(string parameterName)
        {
            throw new NotImplementedException();
        }

        protected override void SetParameter(string parameterName, DbParameter value)
        {
            throw new NotImplementedException();
        }

        protected override void SetParameter(int index, DbParameter value)
        {
            throw new NotImplementedException();
        }
    }
}
