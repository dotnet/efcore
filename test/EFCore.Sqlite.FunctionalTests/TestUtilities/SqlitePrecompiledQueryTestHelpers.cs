// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Sqlite.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class SqlitePrecompiledQueryTestHelpers : PrecompiledQueryTestHelpers
{
    public static SqlitePrecompiledQueryTestHelpers Instance = new();

    protected override IEnumerable<MetadataReference> BuildProviderMetadataReferences()
    {
        yield return MetadataReference.CreateFromFile(typeof(SqliteOptionsExtension).Assembly.Location);
        yield return MetadataReference.CreateFromFile(Assembly.GetExecutingAssembly().Location);
    }
}
