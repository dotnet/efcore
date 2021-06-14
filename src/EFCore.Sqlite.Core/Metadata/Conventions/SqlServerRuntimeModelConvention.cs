// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite.Metadata.Internal;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions
{
    /// <summary>
    ///     A convention that creates an optimized copy of the mutable model.
    /// </summary>
    public class SqliteRuntimeModelConvention : RelationalRuntimeModelConvention
    {
        /// <summary>
        ///     Creates a new instance of <see cref="RelationalModelConvention" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this convention. </param>
        /// <param name="relationalDependencies">  Parameter object containing relational dependencies for this convention. </param>
        public SqliteRuntimeModelConvention(
            ProviderConventionSetBuilderDependencies dependencies,
            RelationalConventionSetBuilderDependencies relationalDependencies)
            : base(dependencies, relationalDependencies)
        {
        }

        /// <summary>
        ///     Updates the property annotations that will be set on the read-only object.
        /// </summary>
        /// <param name="annotations"> The annotations to be processed. </param>
        /// <param name="property"> The source property. </param>
        /// <param name="runtimeProperty"> The target property that will contain the annotations. </param>
        /// <param name="runtime"> Indicates whether the given annotations are runtime annotations. </param>
        protected override void ProcessPropertyAnnotations(
            Dictionary<string, object?> annotations, IProperty property, RuntimeProperty runtimeProperty, bool runtime)
        {
            base.ProcessPropertyAnnotations(annotations, property, runtimeProperty, runtime);

            if (!runtime)
            {
                annotations.Remove(SqliteAnnotationNames.Srid);
            }
        }
    }
}
