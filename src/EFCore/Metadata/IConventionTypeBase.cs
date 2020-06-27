// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Represents a type in an <see cref="IConventionModel" />.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="ITypeBase" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IConventionTypeBase : ITypeBase, IConventionAnnotatable
    {
        /// <summary>
        ///     Gets the model that this type belongs to.
        /// </summary>
        new IConventionModel Model { get; }

        /// <summary>
        ///     Marks the given member name as ignored, preventing conventions from adding a matching property
        ///     or navigation to the type.
        /// </summary>
        /// <param name="memberName"> The name of the member to be ignored. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> The ignored member name. </returns>
        string AddIgnored([NotNull] string memberName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Removes the ignored member name.
        /// </summary>
        /// <param name="memberName"> The name of the member to be removed. </param>
        /// <returns> The removed ignored member name. </returns>
        string RemoveIgnored([NotNull] string memberName);

        /// <summary>
        ///     Indicates whether the given member name is ignored.
        /// </summary>
        /// <param name="memberName"> The name of the member to be ignored. </param>
        /// <returns>
        ///     The configuration source if the given member name is ignored,
        ///     <see langword="null" /> otherwise.
        /// </returns>
        ConfigurationSource? FindIgnoredConfigurationSource([NotNull] string memberName);

        /// <summary>
        ///     Gets all the ignored members.
        /// </summary>
        /// <returns> The list of ignored member names. </returns>
        IEnumerable<string> GetIgnoredMembers();
    }
}
