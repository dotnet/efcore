// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal;

public class CosmosMaterializerJsonReaderData : JsonReaderData
{
    public CosmosMaterializerJsonReaderData(Stream response) : base(((MemoryStream)response).GetBuffer().AsMemory()[..(int)response.Length])
    {
    }


}
