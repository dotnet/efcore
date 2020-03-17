// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

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
    public class NavigationBuilder : IInfrastructure<IConventionSkipNavigationBuilder>, IInfrastructure<IConventionNavigationBuilder>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public NavigationBuilder([NotNull] IMutableNavigationBase navigationOrSkipNavigation)
        {
            Check.NotNull(navigationOrSkipNavigation, nameof(navigationOrSkipNavigation));

            NavBuilder = (navigationOrSkipNavigation as Navigation)?.Builder;
            SkipNavBuilder = (navigationOrSkipNavigation as SkipNavigation)?.Builder;
            Metadata = navigationOrSkipNavigation;

            Check.DebugAssert(NavBuilder != null || SkipNavBuilder != null, "Expected either a Navigation or SkipNavigation");
        }

        private InternalNavigationBuilder NavBuilder { get; }

        private InternalSkipNavigationBuilder SkipNavBuilder { get; }

        /// <summary>
        ///     The navigation being configured.
        /// </summary>
        public virtual IMutableNavigationBase Metadata { get; private set; }

        /// <summary>
        ///     Adds or updates an annotation on the navigation property. If an annotation
        ///     with the key specified in <paramref name="annotation" /> already exists
        ///     its value will be updated.
        /// </summary>
        /// <param name="annotation"> The key of the annotation to be added or updated. </param>
        /// <param name="value"> The value to be stored in the annotation. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual NavigationBuilder HasAnnotation([NotNull] string annotation, [NotNull] object value)
        {
            Check.NotEmpty(annotation, nameof(annotation));
            Check.NotNull(value, nameof(value));

            if (NavBuilder != null)
            {
                NavBuilder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);
            }
            else
            {
                SkipNavBuilder.HasAnnotation(annotation, value, ConfigurationSource.Explicit);
            }

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Sets the <see cref="PropertyAccessMode" /> to use for this property.
        ///     </para>
        ///     <para>
        ///         By default, the backing field, if one is found by convention or has been specified, is used when
        ///         new objects are constructed, typically when entities are queried from the database.
        ///         Properties are used for all other accesses.  Calling this method will change that behavior
        ///         for this property as described in the <see cref="PropertyAccessMode" /> enum.
        ///     </para>
        ///     <para>
        ///         Calling this method overrides for this property any access mode that was set on the
        ///         entity type or model.
        ///     </para>
        /// </summary>
        /// <param name="propertyAccessMode"> The <see cref="PropertyAccessMode" /> to use for this property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual NavigationBuilder UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
        {
            if (NavBuilder != null)
            {
                NavBuilder.UsePropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);
            }
            else
            {
                SkipNavBuilder.UsePropertyAccessMode(propertyAccessMode, ConfigurationSource.Explicit);
            }

            return this;
        }

        /// <summary>
        ///     <para>
        ///         Sets a backing field to use for this navigation property.
        ///     </para>
        /// </summary>
        /// <param name="fieldName"> The name of the field to use for this navigation property. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual NavigationBuilder HasField([CanBeNull] string fieldName)
        {
            if (NavBuilder != null)
            {
                NavBuilder.HasField(fieldName, ConfigurationSource.Explicit);
            }
            else
            {
                SkipNavBuilder.HasField(fieldName, ConfigurationSource.Explicit);
            }

            return this;
        }

        IConventionSkipNavigationBuilder IInfrastructure<IConventionSkipNavigationBuilder>.Instance => SkipNavBuilder;

        IConventionNavigationBuilder IInfrastructure<IConventionNavigationBuilder>.Instance => NavBuilder;


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
        // ReSharper disable once BaseObjectEqualsIsObjectEquals
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode() => base.GetHashCode();

        #endregion
    }
}
