// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Indicates how changes to the value of a property will be handled by Entity Framework change tracking
    ///     which in turn will determine whether the value set is sent to the database or not.
    ///     Used with <see cref="IMutableProperty.BeforeSaveBehavior" /> and
    ///     <see cref="IMutableProperty.AfterSaveBehavior" />
    /// </summary>
    public enum PropertySaveBehavior
    {
        /// <summary>
        ///     The value set or changed will be sent to the database in the normal way.
        /// </summary>
        Save,

        /// <summary>
        ///     Any value set or changed will be ignored.
        /// </summary>
        Ignore,

        /// <summary>
        ///     If an explicit value is set or the value is changed, then an exception will be thrown.
        /// </summary>
        Throw
    }
}
