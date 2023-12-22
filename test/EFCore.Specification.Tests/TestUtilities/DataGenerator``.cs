// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class DataGenerator<T1, T2> : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator() =>
        DataGenerator.GetCombinations(typeof(T1), typeof(T2)).AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
