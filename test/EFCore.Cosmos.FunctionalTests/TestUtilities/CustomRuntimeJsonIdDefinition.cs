// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class CustomRuntimeJsonIdDefinition(RuntimeEntityType entityType, JsonIdDefinition jsonIdDefinition)
    : RuntimeJsonIdDefinition(entityType, jsonIdDefinition)
{
    public override string GenerateIdString(IEnumerable<object?> values)
    {
        var id = base.GenerateIdString(values);
        return id.Replace('|', '-');
    }
}
