// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Text;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;

namespace Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerSqlGenerationHelper : RelationalSqlGenerationHelper
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerSqlGenerationHelper(RelationalSqlGenerationHelperDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string BatchTerminator
        => "GO" + Environment.NewLine + Environment.NewLine;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string StartTransactionStatement
        => "BEGIN TRANSACTION" + StatementTerminator;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string EscapeIdentifier(string identifier)
        => identifier.Replace("]", "]]");

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void EscapeIdentifier(StringBuilder builder, string identifier)
    {
        var initialLength = builder.Length;
        builder.Append(identifier);
        builder.Replace("]", "]]", initialLength, identifier.Length);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override string DelimitIdentifier(string identifier)
        => $"[{EscapeIdentifier(identifier)}]"; // Interpolation okay; strings

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override void DelimitIdentifier(StringBuilder builder, string identifier)
    {
        builder.Append('[');
        EscapeIdentifier(builder, identifier);
        builder.Append(']');
    }

    /// <summary>
    ///     Generates an SQL statement which creates a savepoint with the given name.
    /// </summary>
    /// <param name="name">The name of the savepoint to be created.</param>
    /// <returns>An SQL string to create the savepoint.</returns>
    public override string GenerateCreateSavepointStatement(string name)
        => "SAVE TRANSACTION " + DelimitIdentifier(name) + StatementTerminator;

    /// <summary>
    ///     Generates an SQL statement which rolls back to a savepoint with the given name.
    /// </summary>
    /// <param name="name">The name of the savepoint to be rolled back to.</param>
    /// <returns>An SQL string to roll back the savepoint.</returns>
    public override string GenerateRollbackToSavepointStatement(string name)
        => "ROLLBACK TRANSACTION " + DelimitIdentifier(name) + StatementTerminator;

    /// <summary>
    ///     Generates an SQL statement which releases a savepoint with the given name.
    /// </summary>
    /// <param name="name">The name of the savepoint to be released.</param>
    /// <returns>An SQL string to release the savepoint.</returns>
    public override string GenerateReleaseSavepointStatement(string name)
        => throw new NotSupportedException(SqlServerStrings.NoSavepointRelease);
}
