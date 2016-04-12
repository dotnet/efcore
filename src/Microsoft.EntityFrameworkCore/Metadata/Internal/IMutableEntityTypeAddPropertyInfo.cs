// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public interface IMutableEntityTypeAddPropertyInfo
    {
        /// <summary>
        ///     Adds a property to this entity.
        /// </summary>
        /// <param name="propertyInfo"> The corresponding property in the entity class. </param>
        /// <returns> The newly created property. </returns>
        IMutableProperty AddProperty([NotNull] PropertyInfo propertyInfo);
    }
}
