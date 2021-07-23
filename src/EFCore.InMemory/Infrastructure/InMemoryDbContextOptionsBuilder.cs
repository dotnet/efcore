// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Allows in-memory specific configuration to be performed on <see cref="DbContextOptions" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from a call to
    ///         <see
    ///             cref="InMemoryDbContextOptionsExtensions.UseInMemoryDatabase(DbContextOptionsBuilder, string, System.Action{InMemoryDbContextOptionsBuilder})" />
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class InMemoryDbContextOptionsBuilder : IInMemoryDbContextOptionsBuilderInfrastructure
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="InMemoryDbContextOptionsBuilder" /> class.
        /// </summary>
        /// <param name="optionsBuilder"> The options builder. </param>
        public InMemoryDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
        {
            Check.NotNull(optionsBuilder, nameof(optionsBuilder));

            OptionsBuilder = optionsBuilder;
        }

        /// <summary>
        ///     Clones the configuration in this builder.
        /// </summary>
        /// <returns> The cloned configuration. </returns>
        protected virtual DbContextOptionsBuilder OptionsBuilder { get; }

        /// <inheritdoc />
        DbContextOptionsBuilder IInMemoryDbContextOptionsBuilderInfrastructure.OptionsBuilder
            => OptionsBuilder;

        /// <summary>
        ///     <para>
        ///         Enables nullability check for all properties across all entities within the in-memory database.
        ///     </para>
        /// </summary>
        /// <param name="nullabilityCheckEnabled"> If <see langword="true" />, then nullability check is enforced. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual InMemoryDbContextOptionsBuilder EnableNullabilityCheck(bool nullabilityCheckEnabled = true)
        {
            var extension = OptionsBuilder.Options.FindExtension<InMemoryOptionsExtension>()
                ?? new InMemoryOptionsExtension();

            extension = extension.WithNullabilityCheckEnabled(nullabilityCheckEnabled);

            ((IDbContextOptionsBuilderInfrastructure)OptionsBuilder).AddOrUpdateExtension(extension);

            return this;
        }

        #region Hidden System.Object members

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns> A string that represents the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string? ToString()
            => base.ToString();

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj"> The object to compare with the current object. </param>
        /// <returns> <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object? obj)
            => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
            => base.GetHashCode();

        #endregion
    }
}
