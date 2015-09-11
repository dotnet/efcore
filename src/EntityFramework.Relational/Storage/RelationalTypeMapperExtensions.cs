// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Storage
{
    public static class RelationalTypeMapperExtensions
    {
        private static readonly RelationalTypeMapping _nullTypeMapping = new RelationalTypeMapping("NULL");

        public static RelationalTypeMapping GetMapping(
            [CanBeNull] this IRelationalTypeMapper typeMapper,
            [CanBeNull] object value)
            => value == null
               || value == DBNull.Value
               || typeMapper == null
                ? _nullTypeMapping
                : typeMapper.GetMapping(value.GetType());
    }
}
