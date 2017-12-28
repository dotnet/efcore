// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     A service that can be injected into entities to give them the capability
    ///     of loading navigation properties automatically the first time they are accessed.
    /// </summary>
    public interface ILazyLoader
    {
        /// <summary>
        ///     Loads a navigation property if it has not already been loaded.
        /// </summary>
        /// <param name="entity"> The entity on which the navigation property is located. </param>
        /// <param name="navigationName"> The navigation property name. </param>
        // ReSharper disable once AssignNullToNotNullAttribute
        void Load([NotNull] object entity, [NotNull] [CallerMemberName] string navigationName = null);
    }
}
