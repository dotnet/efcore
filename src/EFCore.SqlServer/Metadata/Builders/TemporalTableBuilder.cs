// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    /// <summary>
    ///     <para>
    ///         Instances of this class are returned from methods when using the <see cref="ModelBuilder" /> API
    ///         and it is not designed to be directly constructed in your application code.
    ///     </para>
    /// </summary>
    public class TemporalTableBuilder
    {
        private readonly EntityTypeBuilder _entityTypeBuilder;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        [EntityFrameworkInternal]
        public TemporalTableBuilder(EntityTypeBuilder entityTypeBuilder)
        {
            _entityTypeBuilder = entityTypeBuilder;
        }

        /// <summary>
        ///     Configures a history table for the entity mapped to a temporal table.
        /// </summary>
        /// <param name="name"> The name of the history table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual TemporalTableBuilder WithHistoryTable(string name)
        {
            _entityTypeBuilder.Metadata.SetTemporalHistoryTableName(name);

            return this;
        }

        /// <summary>
        ///     Configures a history table for the entity mapped to a temporal table.
        /// </summary>
        /// <param name="name"> The name of the history table. </param>
        /// <param name="schema"> The schema of the history table. </param>
        /// <returns> The same builder instance so that multiple calls can be chained. </returns>
        public virtual TemporalTableBuilder WithHistoryTable(string name, string? schema)
        {
            _entityTypeBuilder.Metadata.SetTemporalHistoryTableName(name);
            _entityTypeBuilder.Metadata.SetTemporalHistoryTableSchema(schema);

            return this;
        }

        /// <summary>
        ///     Returns an object that can be used to configure a period start property of the entity type mapped to a temporal table.
        /// </summary>
        /// <param name="propertyName"> The name of the period start property. </param>
        /// <returns> An object that can be used to configure the period start property. </returns>
        public virtual TemporalPeriodPropertyBuilder HasPeriodStart(string propertyName)
        {
            _entityTypeBuilder.Metadata.SetTemporalPeriodStartPropertyName(propertyName);

            return new TemporalPeriodPropertyBuilder(_entityTypeBuilder, propertyName);
        }

        /// <summary>
        ///     Returns an object that can be used to configure a period end property of the entity type mapped to a temporal table.
        /// </summary>
        /// <param name="propertyName"> The name of the period end property. </param>
        /// <returns> An object that can be used to configure the period end property. </returns>
        public virtual TemporalPeriodPropertyBuilder HasPeriodEnd(string propertyName)
        {
            _entityTypeBuilder.Metadata.SetTemporalPeriodEndPropertyName(propertyName);

            return new TemporalPeriodPropertyBuilder(_entityTypeBuilder, propertyName);
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
