// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities.FakeProvider;

public static class FakeRelationalDbContextOptionsExtension
{
    public static DbContextOptionsBuilder UseFakeRelational(
        this DbContextOptionsBuilder optionsBuilder,
        Action<FakeRelationalDbContextOptionsBuilder> fakeRelationalOptionsAction = null)
        => optionsBuilder.UseFakeRelational("Database=Fake", fakeRelationalOptionsAction);

    public static DbContextOptionsBuilder UseFakeRelational(
        this DbContextOptionsBuilder optionsBuilder,
        string connectionString,
        Action<FakeRelationalDbContextOptionsBuilder> fakeRelationalOptionsAction = null)
        => optionsBuilder.UseFakeRelational(new FakeDbConnection(connectionString), fakeRelationalOptionsAction);

    public static DbContextOptionsBuilder UseFakeRelational(
        this DbContextOptionsBuilder optionsBuilder,
        DbConnection connection,
        Action<FakeRelationalDbContextOptionsBuilder> fakeRelationalOptionsAction = null)
    {
        var extension = (FakeRelationalOptionsExtension)GetOrCreateExtension(optionsBuilder).WithConnection(connection);
        ((IDbContextOptionsBuilderInfrastructure)optionsBuilder).AddOrUpdateExtension(extension);

        fakeRelationalOptionsAction?.Invoke(new FakeRelationalDbContextOptionsBuilder(optionsBuilder));

        return optionsBuilder;
    }

    private static FakeRelationalOptionsExtension GetOrCreateExtension(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.Options.FindExtension<FakeRelationalOptionsExtension>()
            ?? new FakeRelationalOptionsExtension();
}
