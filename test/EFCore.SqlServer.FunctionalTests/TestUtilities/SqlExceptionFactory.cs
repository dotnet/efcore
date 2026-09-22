// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class SqlExceptionFactory
{
    public static SqlException CreateSqlException(int number, Guid? connectionId = null)
    {
        var errorCtors = typeof(SqlError)
            .GetTypeInfo()
            .DeclaredConstructors;

        var error = (SqlError)errorCtors.First(c => c.GetParameters().Length == 8)
            .Invoke([number, (byte)0, (byte)0, "Server", "ErrorMessage", "Procedure", 0, null]);
        var errors = (SqlErrorCollection)typeof(SqlErrorCollection)
            .GetTypeInfo()
            .DeclaredConstructors
            .Single()
            .Invoke(null);

        typeof(SqlErrorCollection).GetRuntimeMethods().Single(m => m.Name == "Add").Invoke(errors, [error]);

        var exceptionCtors = typeof(SqlException)
            .GetTypeInfo()
            .DeclaredConstructors;

        return (SqlException)exceptionCtors.First(c => c.GetParameters().Length == 4)
            .Invoke(["Bang!", errors, null, connectionId ?? Guid.NewGuid()]);
    }
}
