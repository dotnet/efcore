// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Metadata
{
    public interface IClrPropertySetter
    {
        void SetClrValue([NotNull] object instance, [CanBeNull] object value);
    }
}
