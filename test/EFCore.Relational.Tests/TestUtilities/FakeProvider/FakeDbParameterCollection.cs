// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

public class FakeDbParameterCollection : DbParameterCollection
{
    private readonly List<object> _parameters = [];

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
        => throw new NotImplementedException();

    public override bool Contains(object value)
        => throw new NotImplementedException();

    public override void CopyTo(Array array, int index)
        => throw new NotImplementedException();

    public override int IndexOf(string parameterName)
        => throw new NotImplementedException();

    public override int IndexOf(object value)
        => throw new NotImplementedException();

    public override void Insert(int index, object value)
        => throw new NotImplementedException();

    public override void Remove(object value)
        => throw new NotImplementedException();

    public override void RemoveAt(string parameterName)
        => throw new NotImplementedException();

    public override void RemoveAt(int index)
        => throw new NotImplementedException();

    protected override DbParameter GetParameter(string parameterName)
        => throw new NotImplementedException();

    protected override void SetParameter(string parameterName, DbParameter value)
        => throw new NotImplementedException();

    protected override void SetParameter(int index, DbParameter value)
        => throw new NotImplementedException();
}
