// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IMutableProperty : IProperty, IMutableAnnotatable
    {
        new IMutableEntityType DeclaringEntityType { get; }
        new Type ClrType { get; [param: NotNull] set; }
        new bool? IsNullable { get; set; }
        new ValueGenerated? ValueGenerated { get; set; }
        new bool? IsReadOnlyBeforeSave { get; set; }
        new bool? IsReadOnlyAfterSave { get; set; }
        new bool? RequiresValueGenerator { get; set; }
        new bool? IsShadowProperty { get; set; }
        new bool? IsConcurrencyToken { get; set; }
        new bool? IsStoreGeneratedAlways { get; set; }
    }
}
