// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API surface for configuring an <see cref="IConventionServiceProperty" /> from conventions.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IConventionServicePropertyBuilder : IConventionAnnotatableBuilder
    {
        /// <summary>
        ///     The service property being configured.
        /// </summary>
        new IConventionServiceProperty Metadata { get; }

        /// <summary>
        ///     Sets the backing field to use for this property.
        /// </summary>
        /// <param name="fieldName"> The field name. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionServicePropertyBuilder HasField([CanBeNull] string fieldName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the backing field to use for this property.
        /// </summary>
        /// <param name="fieldInfo"> The field. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionServicePropertyBuilder HasField([CanBeNull] FieldInfo fieldInfo, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the backing field can be set for this property
        ///     from the current configuration source.
        /// </summary>
        /// <param name="fieldName"> The field name. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the backing field can be set for this property. </returns>
        bool CanSetField([CanBeNull] string fieldName, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the backing field can be set for this property
        ///     from the current configuration source.
        /// </summary>
        /// <param name="fieldInfo"> The field. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the backing field can be set for this property. </returns>
        bool CanSetField([CanBeNull] FieldInfo fieldInfo, bool fromDataAnnotation = false);

        /// <summary>
        ///     Sets the <see cref="ServiceParameterBinding" /> for this property.
        /// </summary>
        /// <param name="parameterBinding"> The parameter binding. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns>
        ///     The same builder instance if the configuration was applied,
        ///     <c>null</c> otherwise.
        /// </returns>
        IConventionServicePropertyBuilder HasParameterBinding(
            [CanBeNull] ServiceParameterBinding parameterBinding, bool fromDataAnnotation = false);

        /// <summary>
        ///     Returns a value indicating whether the <see cref="ServiceParameterBinding" /> can be set for this property.
        ///     from the current configuration source.
        /// </summary>
        /// <param name="parameterBinding"> The parameter binding. </param>
        /// <param name="fromDataAnnotation"> Indicates whether the configuration was specified using a data annotation. </param>
        /// <returns> <c>true</c> if the <see cref="ServiceParameterBinding" /> can be set for this property. </returns>
        bool CanSetParameterBinding([CanBeNull] ServiceParameterBinding parameterBinding, bool fromDataAnnotation = false);
    }
}
