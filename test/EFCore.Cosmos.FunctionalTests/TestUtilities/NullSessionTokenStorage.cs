// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public sealed class NullSessionTokenStorage : SessionTokenStorage
{
    public NullSessionTokenStorage(DbContext dbContext) : base(dbContext)
    {
    }

    public override void AppendSessionToken(string sessionToken) { }

    public override void AppendSessionToken(string containerName, string sessionToken) { }

    public override void Clear() { }

    public override string? GetSessionToken() => null;

    public override string? GetSessionToken(string containerName) => null;

    public override void SetSessionToken(string containerName, string? sessionToken) { }

    public override void AppendSessionTokens(IReadOnlyDictionary<string, string> sessionTokens) { }
}
