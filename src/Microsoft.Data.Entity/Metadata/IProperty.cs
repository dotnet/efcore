// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IProperty : IMetadata
    {
        string Name { get; }
        string StorageName { get; }
        Type PropertyType { get; }
        Type DeclaringType { get; }
        bool IsNullable { get; }
        void SetValue([NotNull] object instance, [CanBeNull] object value);
        object GetValue([NotNull] object instance);
        ValueGenerationStrategy ValueGenerationStrategy { get; }
    }
}
