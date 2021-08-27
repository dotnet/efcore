// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    ///     The options to be used by a <see cref="DbContext" />. You normally override
    ///     <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> or use a <see cref="DbContextOptionsBuilder" />
    ///     to create instances of this class and it is not designed to be directly constructed in your application code.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see> for more information.
    /// </remarks>
    public abstract class DbContextOptions : IDbContextOptions
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="DbContextOptions" /> class. You normally override
        ///     <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" /> or use a <see cref="DbContextOptionsBuilder" />
        ///     to create instances of this class and it is not designed to be directly constructed in your application code.
        /// </summary>
        /// <param name="extensions"> The extensions that store the configured options. </param>
        protected DbContextOptions(
            IReadOnlyDictionary<Type, IDbContextOptionsExtension> extensions)
        {
            Check.NotNull(extensions, nameof(extensions));

            _extensionsMap = extensions as ImmutableSortedDictionary<Type, IDbContextOptionsExtension>
                ?? ImmutableSortedDictionary.Create<Type, IDbContextOptionsExtension>(TypeFullNameComparer.Instance)
                    .AddRange(extensions);
        }

        /// <summary>
        ///     Gets the extensions that store the configured options.
        /// </summary>
        public virtual IEnumerable<IDbContextOptionsExtension> Extensions
            => ExtensionsMap.Values;

        /// <summary>
        ///     Gets the extension of the specified type. Returns <see langword="null" /> if no extension of the specified type is configured.
        /// </summary>
        /// <typeparam name="TExtension"> The type of the extension to get. </typeparam>
        /// <returns> The extension, or <see langword="null" /> if none was found. </returns>
        public virtual TExtension? FindExtension<TExtension>()
            where TExtension : class, IDbContextOptionsExtension
            => ExtensionsMap.TryGetValue(typeof(TExtension), out var extension) ? (TExtension)extension : null;

        /// <summary>
        ///     Gets the extension of the specified type. Throws if no extension of the specified type is configured.
        /// </summary>
        /// <typeparam name="TExtension"> The type of the extension to get. </typeparam>
        /// <returns> The extension. </returns>
        public virtual TExtension GetExtension<TExtension>()
            where TExtension : class, IDbContextOptionsExtension
        {
            var extension = FindExtension<TExtension>();
            if (extension == null)
            {
                throw new InvalidOperationException(CoreStrings.OptionsExtensionNotFound(typeof(TExtension).ShortDisplayName()));
            }

            return extension;
        }

        /// <summary>
        ///     Adds the given extension to the underlying options and creates a new
        ///     <see cref="DbContextOptions" /> with the extension added.
        /// </summary>
        /// <typeparam name="TExtension"> The type of extension to be added. </typeparam>
        /// <param name="extension"> The extension to be added. </param>
        /// <returns> The new options instance with the given extension added. </returns>
        public abstract DbContextOptions WithExtension<TExtension>(TExtension extension)
            where TExtension : class, IDbContextOptionsExtension;

        private readonly ImmutableSortedDictionary<Type, IDbContextOptionsExtension> _extensionsMap;

        /// <summary>
        ///     Gets the extensions that store the configured options.
        /// </summary>
        protected virtual IImmutableDictionary<Type, IDbContextOptionsExtension> ExtensionsMap
            => _extensionsMap;

        /// <summary>
        ///     The type of context that these options are for. Will return <see cref="DbContext" /> if the
        ///     options are not built for a specific derived context.
        /// </summary>
        public abstract Type ContextType { get; }

        /// <summary>
        ///     Specifies that no further configuration of this options object should occur.
        /// </summary>
        public virtual void Freeze()
            => IsFrozen = true;

        /// <summary>
        ///     Returns <see langword="true" /> if <see cref="Freeze" /> has been called. A frozen options object cannot be further
        ///     configured with <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)" />.
        /// </summary>
        public virtual bool IsFrozen { get; private set; }

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => ReferenceEquals(this, obj)
                || (obj is DbContextOptions otherOptions && Equals(otherOptions));

        /// <summary>
        ///     Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="other"> The object to compare with the current object. </param>
        /// <returns>
        ///     <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.
        /// </returns>
        protected virtual bool Equals(DbContextOptions other)
            => _extensionsMap.Count == other._extensionsMap.Count
                && _extensionsMap.Zip(other._extensionsMap)
                    .All(p => p.First.Value.Info.ShouldUseSameServiceProvider(p.Second.Value.Info));

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            foreach (var dbContextOptionsExtension in _extensionsMap)
            {
                hashCode.Add(dbContextOptionsExtension.Key);
                hashCode.Add(dbContextOptionsExtension.Value.Info.GetServiceProviderHashCode());
            }

            return hashCode.ToHashCode();
        }
    }
}
