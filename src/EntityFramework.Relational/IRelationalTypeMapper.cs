// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;

namespace Microsoft.Data.Entity.Relational
{
    public interface IRelationalTypeMapper
    {
        RelationalTypeMapping GetTypeMapping([NotNull] IProperty property);
        RelationalTypeMapping GetTypeMapping([NotNull] ISequence sequence);

        RelationalTypeMapping GetTypeMapping(
            [CanBeNull] string specifiedType,
            [NotNull] string storageName,
            [NotNull] Type propertyType,
            bool isKey,
            bool isConcurrencyToken);
    }
}
