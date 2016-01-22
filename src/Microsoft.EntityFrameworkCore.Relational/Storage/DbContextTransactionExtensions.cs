// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public static class DbContextTransactionExtensions
    {
        public static DbTransaction GetDbTransaction([NotNull] this IDbContextTransaction dbContextTransaction)
        {
            Check.NotNull(dbContextTransaction, nameof(dbContextTransaction));

            var accessor = dbContextTransaction as IInfrastructure<DbTransaction>;

            if (accessor == null)
            {
                throw new InvalidOperationException(RelationalStrings.RelationalNotInUse);
            }

            return accessor.GetInfrastructure();
        }
    }
}
