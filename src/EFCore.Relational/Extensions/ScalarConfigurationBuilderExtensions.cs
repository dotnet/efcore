// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     Relational database specific extension methods for <see cref="ScalarConfigurationBuilder" />.
    /// </summary>
    public static class ScalarConfigurationBuilderExtensions
    {
        /// <summary>
        ///     Configures the data type of the column that the scalar maps to when targeting a relational database.
        ///     This should be the complete type name, including precision, scale, length, etc.
        /// </summary>
        /// <param name="scalarBuilder"> The builder for the scalar being configured. </param>
        /// <param name="typeName"> The name of the data type of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ScalarConfigurationBuilder HaveColumnType(
            this ScalarConfigurationBuilder scalarBuilder,
            string typeName)
        {
            Check.NotNull(scalarBuilder, nameof(scalarBuilder));
            Check.NotEmpty(typeName, nameof(typeName));

            scalarBuilder.HaveAnnotation(RelationalAnnotationNames.ColumnType, typeName);

            return scalarBuilder;
        }

        /// <summary>
        ///     Configures the data type of the column that the scalar maps to when targeting a relational database.
        ///     This should be the complete type name, including precision, scale, length, etc.
        /// </summary>
        /// <typeparam name="TScalar"> The type of the scalar being configured. </typeparam>
        /// <param name="scalarBuilder"> The builder for the scalar being configured. </param>
        /// <param name="typeName"> The name of the data type of the column. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public static ScalarConfigurationBuilder<TScalar> HaveColumnType<TScalar>(
            this ScalarConfigurationBuilder<TScalar> scalarBuilder,
            string typeName)
            => (ScalarConfigurationBuilder<TScalar>)HaveColumnType((ScalarConfigurationBuilder)scalarBuilder, typeName);

        /// <summary>
        ///     Configures the scalar as capable of storing only fixed-length data, such as strings.
        /// </summary>
        /// <param name="scalarBuilder"> The builder for the scalar being configured. </param>
        /// <param name="fixedLength"> A value indicating whether the scalar is constrained to fixed length values. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public static ScalarConfigurationBuilder AreFixedLength(
            this ScalarConfigurationBuilder scalarBuilder,
            bool fixedLength = true)
        {
            Check.NotNull(scalarBuilder, nameof(scalarBuilder));

            scalarBuilder.HaveAnnotation(RelationalAnnotationNames.IsFixedLength, fixedLength);

            return scalarBuilder;
        }

        /// <summary>
        ///     Configures the scalar as capable of storing only fixed-length data, such as strings.
        /// </summary>
        /// <typeparam name="TScalar"> The type of the scalar being configured. </typeparam>
        /// <param name="scalarBuilder"> The builder for the scalar being configured. </param>
        /// <param name="fixedLength"> A value indicating whether the scalar is constrained to fixed length values. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public static ScalarConfigurationBuilder<TScalar> AreFixedLength<TScalar>(
            this ScalarConfigurationBuilder<TScalar> scalarBuilder,
            bool fixedLength = true)
            => (ScalarConfigurationBuilder<TScalar>)AreFixedLength((ScalarConfigurationBuilder)scalarBuilder, fixedLength);
    }
}
