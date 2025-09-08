// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;
using XuguClient;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

public class XGConnectionStringOptionsValidator : IXGConnectionStringOptionsValidator
{
    public virtual bool EnsureMandatoryOptions(ref string connectionString)
    {
        if (connectionString is not null)
        {
            var csb = new XGConnectionStringBuilder(connectionString);

            connectionString = csb.ConnectionString;

            return true;
        }

        return false;
    }

    public virtual bool EnsureMandatoryOptions(DbConnection connection)
    {
        if (connection is not null)
        {
            var csb = new XGConnectionStringBuilder(connection.ConnectionString);

            try
            {
                //csb.AllowUserVariables = true;
                //csb.UseAffectedRows = false;

                connection.ConnectionString = csb.ConnectionString;

                return true;
            }
            catch (Exception e)
            {
                ThrowException(e);
            }
        }

        return false;
    }

    public virtual bool EnsureMandatoryOptions(DbDataSource dataSource)
    {
        if (dataSource is null)
        {
            return false;
        }

        var csb = new XGConnectionStringBuilder(dataSource.ConnectionString);

        // We can't alter the connection string of a DbDataSource/XGDataSource as we do for DbConnection/XGConnection in cases
        // where the necessary connection string options have not been set.
        // We can only throw.
        ThrowException();

        return true;
    }

    public virtual void ThrowException(Exception innerException = null)
        => throw new InvalidOperationException(
            @"The connection string of a connection used by Microsoft.EntityFrameworkCore.XuGu must contain ""AllowUserVariables=True;UseAffectedRows=False"".",
            innerException);

}
