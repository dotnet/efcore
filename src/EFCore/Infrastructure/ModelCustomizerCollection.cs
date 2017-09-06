// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Contains an ordered collection of <see cref="IModelCustomizer" />
    /// </summary>
    public class ModelCustomizerCollection : IModelCustomizerCollection
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ModelCustomizerCollection" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public ModelCustomizerCollection(
            [NotNull] ModelCustomizerCollectionDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Customizers = new List<IModelCustomizer>(dependencies.AdditionalModelCustomizers);
            Customizers.Insert(0, dependencies.ModelCustomizer);
        }

        /// <summary>
        ///     An ordered collection of <see cref="IModelCustomizer" />
        /// </summary>
        public virtual IEnumerable<IModelCustomizer> Items => Customizers;

        /// <summary>
        ///     An ordered collection of <see cref="IModelCustomizer" />
        /// </summary>
        protected virtual List<IModelCustomizer> Customizers { get; [param: NotNull] set; }
    }
}
