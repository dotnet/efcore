// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <inheritdoc />
    public sealed class ModelCreationDependencies : IModelCreationDependencies
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ModelCreationDependencies(
            [NotNull] IModelSource modelSource,
            [NotNull] IConventionSetBuilder conventionSetBuilder,
            [NotNull] ModelDependencies modelDependencies)
        {
            Check.NotNull(modelSource, nameof(modelSource));
            Check.NotNull(conventionSetBuilder, nameof(conventionSetBuilder));
            Check.NotNull(modelDependencies, nameof(modelDependencies));

            ModelSource = modelSource;
            ConventionSetBuilder = conventionSetBuilder;
            ModelDependencies = modelDependencies;
        }

        /// <inheritdoc />
        public IModelSource ModelSource { get; }

        /// <inheritdoc />
        public IConventionSetBuilder ConventionSetBuilder { get; }

        /// <inheritdoc />
        public ModelDependencies ModelDependencies { get; }

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="modelSource"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ModelCreationDependencies With([NotNull] IModelSource modelSource)
            => new ModelCreationDependencies(modelSource, ConventionSetBuilder, ModelDependencies);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="conventionSetBuilder"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ModelCreationDependencies With([NotNull] IConventionSetBuilder conventionSetBuilder)
            => new ModelCreationDependencies(ModelSource, conventionSetBuilder, ModelDependencies);

        /// <summary>
        ///     Clones this dependency parameter object with one service replaced.
        /// </summary>
        /// <param name="modelDependencies"> A replacement for the current dependency of this type. </param>
        /// <returns> A new parameter object with the given service replaced. </returns>
        public ModelCreationDependencies With([NotNull] ModelDependencies modelDependencies)
            => new ModelCreationDependencies(ModelSource, ConventionSetBuilder, modelDependencies);
    }
}
