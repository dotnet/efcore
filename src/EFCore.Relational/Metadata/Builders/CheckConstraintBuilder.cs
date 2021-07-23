// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     Provides a simple API for configuring a check constraint.
    /// </summary>
    public class CheckConstraintBuilder : IInfrastructure<IConventionCheckConstraintBuilder>
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public CheckConstraintBuilder(IMutableCheckConstraint checkConstraint)
        {
            Check.NotNull(checkConstraint, nameof(checkConstraint));

            Builder = ((CheckConstraint)checkConstraint).Builder;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        protected virtual InternalCheckConstraintBuilder Builder { [DebuggerStepThrough] get; }

        /// <inheritdoc />
        IConventionCheckConstraintBuilder IInfrastructure<IConventionCheckConstraintBuilder>.Instance
        {
            [DebuggerStepThrough] get => Builder;
        }

        /// <summary>
        ///     The check constraint being configured.
        /// </summary>
        public virtual IMutableCheckConstraint Metadata
            => Builder.Metadata;

        /// <summary>
        ///     Sets the database name of the check constraint.
        /// </summary>
        /// <param name="name"> The database name of the check constraint. </param>
        /// <returns> The same builder instance so that multiple configuration calls can be chained. </returns>
        public virtual CheckConstraintBuilder HasName(string name)
        {
            Builder.HasName(name, ConfigurationSource.Explicit);

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
        // ReSharper disable once BaseObjectEqualsIsObjectEquals
        public override bool Equals(object? obj)
            => base.Equals(obj);

        /// <summary>
        ///     Serves as the default hash function.
        /// </summary>
        /// <returns> A hash code for the current object. </returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        // ReSharper disable once BaseObjectGetHashCodeCallInGetHashCode
        public override int GetHashCode()
            => base.GetHashCode();

        #endregion
    }
}
