// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Utilities;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    public static class XGIndexBuilderExtensions
    {
        #region FullText

        // TODO: Remove/Hide for .NET 5.
        [Obsolete("This extension method is obsolete. Use IsFullText instead.")]
        public static IndexBuilder ForXGIsFullText([NotNull] this IndexBuilder indexBuilder, bool fullText = true)
            => IsFullText(indexBuilder, fullText);

        // TODO: Remove/Hide for .NET 5.
        [Obsolete("This extension method is obsolete. Use IsFullText instead.")]
        public static IndexBuilder<TEntity> ForXGIsFullText<TEntity>([NotNull] this IndexBuilder<TEntity> indexBuilder, bool fullText = true)
            => IsFullText(indexBuilder, fullText);

        /// <summary>
        /// Sets a value indicating whether the index is full text.
        /// </summary>
        /// <param name="indexBuilder"> The index builder. </param>
        /// <param name="fullText"> The value to set. </param>
        /// <param name="parser"> An optional argument (e.g. "ngram"), that will be used in an `WITH PARSER` clause. </param>
        /// <returns> The index builder. </returns>
        public static IndexBuilder IsFullText([NotNull] this IndexBuilder indexBuilder, bool fullText = true, string parser = null)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            indexBuilder.Metadata.SetIsFullText(fullText);
            indexBuilder.Metadata.SetFullTextParser(parser);

            return indexBuilder;
        }

        /// <summary>
        /// Sets a value indicating whether the index is full text.
        /// </summary>
        /// <param name="indexBuilder"> The index builder. </param>
        /// <param name="fullText"> The value to set. </param>
        /// <param name="parser"> An optional argument (e.g. "ngram"), that will be used in an `WITH PARSER` clause. </param>
        /// <returns> The index builder. </returns>
        public static IndexBuilder<TEntity> IsFullText<TEntity>([NotNull] this IndexBuilder<TEntity> indexBuilder, bool fullText = true, string parser = null)
            => (IndexBuilder<TEntity>)IsFullText((IndexBuilder)indexBuilder, fullText, parser);

        #endregion FullText

        #region Spatial

        // TODO: Remove/Hide for .NET 5.
        [Obsolete("This extension method is obsolete. Use IsSpatial instead.")]
        public static IndexBuilder ForXGIsSpatial([NotNull] this IndexBuilder indexBuilder, bool spatial = true)
            => IsSpatial(indexBuilder, spatial);

        // TODO: Remove/Hide for .NET 5.
        [Obsolete("This extension method is obsolete. Use IsSpatial instead.")]
        public static IndexBuilder<TEntity> ForXGIsSpatial<TEntity>([NotNull] this IndexBuilder<TEntity> indexBuilder, bool spatial = true)
            => IsSpatial(indexBuilder, spatial);

        /// <summary>
        /// Sets a value indicating whether the index is spartial.
        /// </summary>
        /// <param name="indexBuilder"> The index builder. </param>
        /// <param name="spatial"> The value to set. </param>
        /// <returns> The index builder. </returns>
        public static IndexBuilder IsSpatial([NotNull] this IndexBuilder indexBuilder, bool spatial = true)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            indexBuilder.Metadata.SetIsSpatial(spatial);

            return indexBuilder;
        }

        /// <summary>
        /// Sets a value indicating whether the index is spartial.
        /// </summary>
        /// <param name="indexBuilder"> The index builder. </param>
        /// <param name="spatial"> The value to set. </param>
        /// <returns> The index builder. </returns>
        public static IndexBuilder<TEntity> IsSpatial<TEntity>([NotNull] this IndexBuilder<TEntity> indexBuilder, bool spatial = true)
            => (IndexBuilder<TEntity>)IsSpatial((IndexBuilder)indexBuilder, spatial);

        #endregion Spatial

        #region PrefixLength

        /// <summary>
        /// Sets prefix lengths for the index.
        /// </summary>
        /// <param name="indexBuilder"> The index builder. </param>
        /// <param name="prefixLengths">The prefix lengths to set, in the order the index columns where specified.
        /// A value of `0` indicates, that the full length should be used for that column. </param>
        /// <returns> The index builder. </returns>
        public static IndexBuilder HasPrefixLength([NotNull] this IndexBuilder indexBuilder, params int[] prefixLengths)
        {
            Check.NotNull(indexBuilder, nameof(indexBuilder));

            indexBuilder.Metadata.SetPrefixLength(prefixLengths);

            return indexBuilder;
        }

        /// <summary>
        /// Sets prefix lengths for the index.
        /// </summary>
        /// <param name="indexBuilder"> The index builder. </param>
        /// <param name="prefixLengths">The prefix lengths to set, in the order the index columns where specified.
        /// A value of `0` indicates, that the full length should be used for that column. </param>
        /// <returns> The index builder. </returns>
        public static IndexBuilder<TEntity> HasPrefixLength<TEntity>([NotNull] this IndexBuilder<TEntity> indexBuilder, params int[] prefixLengths)
            => (IndexBuilder<TEntity>)HasPrefixLength((IndexBuilder)indexBuilder, prefixLengths);

        #endregion PrefixLength
    }
}
