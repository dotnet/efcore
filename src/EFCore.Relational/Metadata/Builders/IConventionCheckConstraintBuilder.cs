// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API for configuring a check constraint.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> and
    ///     <see href="https://aka.ms/efcore-docs-check-constraints">Database check constraints</see> for more information.
    /// </remarks>
    public interface IConventionCheckConstraintBuilder : IConventionAnnotatableBuilder
    {
        /// <summary>
        ///     The check constraint being configured.
        /// </summary>
        new IConventionCheckConstraint Metadata { get; }

        /// <summary>
        ///     Sets the database name of the check constraint.
        /// </summary>
        /// <param name="name">The database name of the check constraint.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        IConventionCheckConstraintBuilder? HasName(string? name, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the given name can be set for the check constraint.
        /// </summary>
        /// <param name="name">The database name of the check constraint.</param>
        /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
        /// <returns><see langword="true" /> if the database name can be set for the check constraint.</returns>
        bool CanSetName(string? name, bool fromDataAnnotation = false);
    }
}
