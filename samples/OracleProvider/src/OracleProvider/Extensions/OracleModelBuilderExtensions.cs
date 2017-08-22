// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Oracle specific extension methods for <see cref="ModelBuilder" />.
    /// </summary>
    public static class OracleModelBuilderExtensions
    {
        /// <summary>
        ///     Configures the model to use a sequence-based hi-lo pattern to generate values for key properties
        ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting Oracle.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <param name="name"> The name of the sequence. </param>
        /// <param name="schema">The schema of the sequence. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder ForOracleUseSequenceHiLo(
            [NotNull] this ModelBuilder modelBuilder,
            [CanBeNull] string name = null)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));
            Check.NullButNotEmpty(name, nameof(name));

            var model = modelBuilder.Model;

            name = name ?? OracleModelAnnotations.DefaultHiLoSequenceName;

            if (model.Oracle().FindSequence(name) == null)
            {
                modelBuilder.HasSequence(name).IncrementsBy(10);
            }

            model.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.SequenceHiLo;
            model.Oracle().HiLoSequenceName = name;

            return modelBuilder;
        }

        /// <summary>
        ///     Configures the model to use the Oracle IDENTITY feature to generate values for key properties
        ///     marked as <see cref="ValueGenerated.OnAdd" />, when targeting Oracle. This is the default
        ///     behavior when targeting Oracle.
        /// </summary>
        /// <param name="modelBuilder"> The model builder. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ModelBuilder ForOracleUseIdentityColumns(
            [NotNull] this ModelBuilder modelBuilder)
        {
            Check.NotNull(modelBuilder, nameof(modelBuilder));

            var property = modelBuilder.Model;

            property.Oracle().ValueGenerationStrategy = OracleValueGenerationStrategy.IdentityColumn;
            property.Oracle().HiLoSequenceName = null;

            return modelBuilder;
        }
    }
}
