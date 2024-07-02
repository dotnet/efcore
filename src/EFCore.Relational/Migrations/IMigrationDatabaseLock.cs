// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
///     Represents an exclusive lock on the database that is used to ensure that only one migration application can be run at a time.
/// </summary>
public interface IMigrationDatabaseLock : IDisposable, IAsyncDisposable
{
}
