// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity
{
    /// <summary>
    ///     Indicates how the navigation properties of an entity are traversed so that a given operation can be recursively
    ///     performed on the entities that it is related to.
    /// </summary>
    public enum GraphBehavior
    {
        /// <summary>
        ///     Navigation properties where the entity being acted on is the principal are traversed.
        /// </summary>
        IncludeDependents,

        /// <summary>
        ///     Navigation properties are not traversed. The operation is only performed on the root entity.
        /// </summary>
        SingleObject
    };
}
