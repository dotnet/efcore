// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IClrCollectionAccessor
    {
        void Add([NotNull] object instance, [NotNull] object value);
        bool Contains([NotNull] object instance, [NotNull] object value);
        void Remove([NotNull] object instance, [NotNull] object value);
    }
}
