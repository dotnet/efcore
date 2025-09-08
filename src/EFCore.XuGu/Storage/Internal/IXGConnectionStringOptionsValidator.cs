// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Data.Common;

namespace Microsoft.EntityFrameworkCore.XuGu.Storage.Internal;

public interface IXGConnectionStringOptionsValidator
{
    bool EnsureMandatoryOptions(ref string connectionString);
    bool EnsureMandatoryOptions(DbConnection connection);
    bool EnsureMandatoryOptions(DbDataSource dataSource);

    void ThrowException(Exception innerException = null);
}
