// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InMemoryTable<TKey> : IInMemoryTable
    {
        private readonly IPrincipalKeyValueFactory<TKey> _keyValueFactory;
        private readonly bool _sensitiveLoggingEnabled;
        private readonly Dictionary<TKey, object[]> _rows;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InMemoryTable([NotNull] IPrincipalKeyValueFactory<TKey> keyValueFactory, bool sensitiveLoggingEnabled)
        {
            _keyValueFactory = keyValueFactory;
            _sensitiveLoggingEnabled = sensitiveLoggingEnabled;
            _rows = new Dictionary<TKey, object[]>(keyValueFactory.EqualityComparer);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IReadOnlyList<object[]> SnapshotRows()
            => _rows.Values.ToList();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Create(IUpdateEntry entry)
            => _rows.Add(CreateKey(entry), CreateValueBuffer(entry));

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Delete(IUpdateEntry entry)
        {
            var key = CreateKey(entry);

            if (_rows.ContainsKey(key))
            {
                var properties = entry.EntityType.GetProperties().ToList();
                var concurrencyConflicts = new Dictionary<IProperty, object>();

                for (var index = 0; index < properties.Count; index++)
                {
                    IsConcurrencyConflict(entry, properties[index], _rows[key][index], concurrencyConflicts);
                }

                if (concurrencyConflicts.Any())
                {
                    ThrowUpdateConcurrencyException(entry, concurrencyConflicts);
                }

                _rows.Remove(key);
            }
            else
            {
                throw new DbUpdateConcurrencyException(InMemoryStrings.UpdateConcurrencyException, new[] { entry });
            }
        }

        private static bool IsConcurrencyConflict(
            IUpdateEntry entry,
            IProperty property,
            object rowValue,
            Dictionary<IProperty, object> concurrencyConflicts)
        {
            if (property.IsConcurrencyToken)
            {
                var originalValue = entry.GetOriginalValue(property);

                var revertPatchBehavior = AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue12214", out var isEnabled)
                                          && isEnabled;

                if ((revertPatchBehavior
                    && !Equals(rowValue, originalValue))
                    || (!revertPatchBehavior
                        && !StructuralComparisons.StructuralEqualityComparer.Equals(rowValue, originalValue)))
                {
                    concurrencyConflicts.Add(property, rowValue);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Update(IUpdateEntry entry)
        {
            var key = CreateKey(entry);

            if (_rows.ContainsKey(key))
            {
                var properties = entry.EntityType.GetProperties().ToList();
                var valueBuffer = new object[properties.Count];
                var concurrencyConflicts = new Dictionary<IProperty, object>();

                for (var index = 0; index < valueBuffer.Length; index++)
                {
                    if (IsConcurrencyConflict(entry, properties[index], _rows[key][index], concurrencyConflicts))
                    {
                        continue;
                    }

                    valueBuffer[index] = entry.IsModified(properties[index])
                        ? entry.GetCurrentValue(properties[index])
                        : _rows[key][index];
                }

                if (concurrencyConflicts.Any())
                {
                    ThrowUpdateConcurrencyException(entry, concurrencyConflicts);
                }

                _rows[key] = valueBuffer;
            }
            else
            {
                throw new DbUpdateConcurrencyException(InMemoryStrings.UpdateConcurrencyException, new[] { entry });
            }
        }

        private TKey CreateKey(IUpdateEntry entry)
            => _keyValueFactory.CreateFromCurrentValues((InternalEntityEntry)entry);

        private static object[] CreateValueBuffer(IUpdateEntry entry)
            => entry.EntityType.GetProperties().Select(entry.GetCurrentValue).ToArray();

        /// <summary>
        ///     Throws an exception indicating that concurrency conflicts were detected.
        /// </summary>
        /// <param name="entry"> The update entry which resulted in the conflict(s). </param>
        /// <param name="concurrencyConflicts"> The conflicting properties with their associated database values. </param>
        protected virtual void ThrowUpdateConcurrencyException([NotNull] IUpdateEntry entry, [NotNull] Dictionary<IProperty, object> concurrencyConflicts)
        {
            Check.NotNull(entry, nameof(entry));
            Check.NotNull(concurrencyConflicts, nameof(concurrencyConflicts));

            if (_sensitiveLoggingEnabled)
            {
                throw new DbUpdateConcurrencyException(
                    InMemoryStrings.UpdateConcurrencyTokenExceptionSensitive(
                        entry.EntityType.DisplayName(),
                        entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey().Properties),
                        entry.BuildOriginalValuesString(concurrencyConflicts.Keys),
                        "{" + string.Join(", ", concurrencyConflicts.Select(c => c.Key.Name + ": " + Convert.ToString(c.Value, CultureInfo.InvariantCulture))) + "}"),
                    new[] { entry });
            }

            throw new DbUpdateConcurrencyException(
                InMemoryStrings.UpdateConcurrencyTokenException(
                    entry.EntityType.DisplayName(),
                    Property.Format(concurrencyConflicts.Keys)),
                new[] { entry });
        }
    }
}
