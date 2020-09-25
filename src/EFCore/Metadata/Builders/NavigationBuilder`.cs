// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Provides a simple API for configuring a <see cref="INavigation" /> or <see cref="ISkipNavigation" />.
    ///     </para>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class NavigationBuilder<TSource, TTarget> : NavigationBuilder
        where TSource : class
        where TTarget : class
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public NavigationBuilder([NotNull] IMutableNavigationBase navigationOrSkipNavigation)
            : base(navigationOrSkipNavigation)
        {
        }

        /// <inheritdoc cref="NavigationBuilder.HasAnnotation" />
        public new virtual NavigationBuilder<TSource, TTarget> HasAnnotation([NotNull] string annotation, [NotNull] object value)
            => (NavigationBuilder<TSource, TTarget>)base.HasAnnotation(annotation, value);

        /// <inheritdoc cref="NavigationBuilder.UsePropertyAccessMode" />
        public new virtual NavigationBuilder<TSource, TTarget> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => (NavigationBuilder<TSource, TTarget>)base.UsePropertyAccessMode(propertyAccessMode);

        /// <inheritdoc cref="NavigationBuilder.HasField" />
        public new virtual NavigationBuilder<TSource, TTarget> HasField([CanBeNull] string fieldName)
            => (NavigationBuilder<TSource, TTarget>)base.HasField(fieldName);

        /// <inheritdoc cref="NavigationBuilder.AutoInclude" />
        public new virtual NavigationBuilder<TSource, TTarget> AutoInclude(bool autoInclude = true)
            => (NavigationBuilder<TSource, TTarget>)base.AutoInclude(autoInclude);

        /// <inheritdoc cref="NavigationBuilder.IsRequired" />
        public new virtual NavigationBuilder<TSource, TTarget> IsRequired(bool required = true)
            => (NavigationBuilder<TSource, TTarget>)base.IsRequired(required);

        #region Hidden System.Object members

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
            => base.ToString();

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectEqualsIsObjectEquals
        public override bool Equals(object obj)
            => base.Equals(obj);

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode()
            => base.GetHashCode();

        #endregion
    }
}
