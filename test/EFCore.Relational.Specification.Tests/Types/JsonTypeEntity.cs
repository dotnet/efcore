// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Types;

public class JsonTypeEntity<T>
{
    public int Id { get; set; }

    public required T Value { get; set; }
    public required T OtherValue { get; set; }

    public required JsonContainer<T> JsonContainer { get; set; }
}

public class JsonContainer<T>
{
    public required T Value { get; set; }
    public required T OtherValue { get; set; }
}
