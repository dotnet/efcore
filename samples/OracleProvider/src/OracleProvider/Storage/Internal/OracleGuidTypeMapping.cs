// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    public class OracleGuidTypeMapping : GuidTypeMapping
    {
        public OracleGuidTypeMapping([NotNull] string storeType, [CanBeNull] DbType? dbType = null)
            : base(storeType, dbType)
        {
        }

        public override string GenerateSqlLiteral(object value)
        {
            if (value is Guid guid)
            {
                return $"'{BitConverter.ToString(guid.ToByteArray()).Replace("-", string.Empty)}'";
            }

            return base.GenerateSqlLiteral(value);
        }
    }
}
