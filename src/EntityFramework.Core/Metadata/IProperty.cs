// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IProperty : IPropertyBase
    {
        Type ClrType { get; }
        Type UnderlyingType { get; }
        bool IsNullable { get; }
        bool IsReadOnly { get; }
        bool UseStoreDefault { get; }
        bool IsStoreComputed { get; }
        bool IsValueGeneratedOnAdd { get; }
        int Index { get; }
        int ShadowIndex { get; }
        int OriginalValueIndex { get; }
        bool IsShadowProperty { get; }
        bool IsConcurrencyToken { get; }
        int MaxLength { get; }
        object SentinelValue { get; }
    }
}
