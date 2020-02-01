// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Allows Cosmos specific configuration to be performed on <see cref="DbContextOptions" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from a call to
    ///         <see cref="CosmosDbContextOptionsExtensions.UseCosmos{TContext}" />
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class CosmosDbContextOptionsBuilder
    {
        private readonly DbContextOptionsBuilder _optionsBuilder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CosmosDbContextOptionsBuilder" /> class.
        /// </summary>
        /// <param name="optionsBuilder"> The options builder. </param>
        public CosmosDbContextOptionsBuilder([NotNull] DbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            _optionsBuilder = optionsBuilder;
        }

        /// <summary>
        ///     Configures the context to use the provided <see cref="IExecutionStrategy" />.
        /// </summary>
        /// <param name="getExecutionStrategy"> A function that returns a new instance of an execution strategy. </param>
        public virtual CosmosDbContextOptionsBuilder ExecutionStrategy(
            [NotNull] Func<ExecutionStrategyDependencies, IExecutionStrategy> getExecutionStrategy)
            => WithOption(e => e.WithExecutionStrategyFactory(Check.NotNull(getExecutionStrategy, nameof(getExecutionStrategy))));

        /// <summary>
        ///     Configures the context to use the provided Region.
        /// </summary>
        /// <param name="region">CosmosDB region name</param>
        public virtual CosmosDbContextOptionsBuilder Region([NotNull] string region)
            => WithOption(e => e.WithRegion(Check.NotNull(region, nameof(region))));

        /// <summary>
        ///     Configures the context to use the provided connection mode.
        /// </summary>
        /// <param name="connectionMode">CosmosDB connection mode</param>
        public virtual CosmosDbContextOptionsBuilder ConnectionMode(ConnectionMode connectionMode)
            => WithOption(e => e.WithConnectionMode(Check.NotNull(connectionMode, nameof(connectionMode))));

        /// <summary>
        ///     Sets an option by cloning the extension used to store the settings. This ensures the builder
        ///     does not modify options that are already in use elsewhere.
        /// </summary>
        /// <param name="setAction"> An action to set the option. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        protected virtual CosmosDbContextOptionsBuilder WithOption([NotNull] Func<CosmosOptionsExtension, CosmosOptionsExtension> setAction)
        {
            ((IDbContextOptionsBuilderInfrastructure)_optionsBuilder).AddOrUpdateExtension(
                setAction(_optionsBuilder.Options.FindExtension<CosmosOptionsExtension>() ?? new CosmosOptionsExtension()));

            return this;
        }

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> true if the specified object is equal to the current object; otherwise, false. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
