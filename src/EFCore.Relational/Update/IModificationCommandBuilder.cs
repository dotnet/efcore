// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         A service for creating <see cref="IModificationCommand" /> instance.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers; it is generally not used in application code.
    ///     </para>
    /// </summary>
    public interface IModificationCommandBuilder
    {
        /// <summary>
        ///     List of added entries.
        /// </summary>
        public IReadOnlyList<IUpdateEntry> Entries { get; }

        /// <summary>
        ///     State of entity.
        /// </summary>
        public EntityState EntityState { get; }

        /// <summary>
        ///     Adds an entry.
        /// </summary>
        /// <param name="entry">Entry object.</param>
        /// <param name="mainEntry">Indicator of main entry. Only one main entry can be added.</param>
        ///
        /// You are can't call this method after call of GetModificationCommand method.
        /// 
        public void AddEntry(IUpdateEntry entry, bool mainEntry);

        /// <summary>
        ///     Creates an instance of <see cref="IModificationCommand" /> instance.
        /// </summary>
        /// <returns>
        ///     Instance of <see cref="IModificationCommand" />.
        /// </returns>
        ///
        /// Expected that multiple calls of this method will returns a same object.
        ///
        public IModificationCommand GetModificationCommand();
    }
}
