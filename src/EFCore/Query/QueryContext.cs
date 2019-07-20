// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     The principal data structure used by a compiled query during execution.
    /// </summary>
    public class QueryContext : IParameterValues
    {
        private readonly IDictionary<string, object> _parameterValues = new Dictionary<string, object>();

        /// <summary>
        ///     <para>
        ///         Creates a new <see cref="QueryContext"/> instance.
        ///     </para>
        ///     <para>
        ///         This type is typically used by database providers (and other extensions). It is generally
        ///         not used in application code.
        ///     </para>
        /// </summary>
        /// <param name="dependencies"> The dependencies to use. </param>
        public QueryContext(
            [NotNull] QueryContextDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Gets the current DbContext.
        /// </summary>
        public virtual DbContext Context => Dependencies.CurrentContext.Context;

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual QueryContextDependencies Dependencies { get; }

        /// <summary>
        ///     The state manager.
        /// </summary>
        /// <value>
        ///     The state manager.
        /// </value>
        public virtual IStateManager StateManager
            => Dependencies.StateManager;

        /// <summary>
        ///     The query provider.
        /// </summary>
        /// <value>
        ///     The query provider.
        /// </value>
        public virtual IQueryProvider QueryProvider
            => Dependencies.QueryProvider;

        /// <summary>
        ///     Gets the concurrency detector.
        /// </summary>
        /// <value>
        ///     The concurrency detector.
        /// </value>
        public virtual IConcurrencyDetector ConcurrencyDetector
            => Dependencies.ConcurrencyDetector;

        /// <summary>
        ///     Gets or sets the cancellation token.
        /// </summary>
        /// <value>
        ///     The cancellation token.
        /// </value>
        public virtual CancellationToken CancellationToken { get; set; }

        /// <summary>
        ///     Gets or sets the cancellation token.
        /// </summary>
        /// <value>
        ///     The cancellation token.
        /// </value>
        public virtual IDiagnosticsLogger<DbLoggerCategory.Database.Command> CommandLogger
            => Dependencies.CommandLogger;

        /// <summary>
        ///     Gets or sets the cancellation token.
        /// </summary>
        /// <value>
        ///     The cancellation token.
        /// </value>
        public virtual IDiagnosticsLogger<DbLoggerCategory.Query> QueryLogger
            => Dependencies.QueryLogger;

        /// <summary>
        ///     The parameter values.
        /// </summary>
        public virtual IReadOnlyDictionary<string, object> ParameterValues
            => (IReadOnlyDictionary<string, object>)_parameterValues;

        /// <summary>
        ///     Adds a parameter.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <param name="value"> The value. </param>
        public virtual void AddParameter(string name, object value)
        {
            Check.NotEmpty(name, nameof(name));

            _parameterValues.Add(name, value);
        }

        /// <summary>
        ///     Sets a parameter value.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <param name="value"> The value. </param>
        public virtual void SetParameter(string name, object value)
        {
            Check.NotEmpty(name, nameof(name));

            _parameterValues[name] = value;
        }

        /// <summary>
        ///     Removes a parameter by name.
        /// </summary>
        /// <param name="name"> The name. </param>
        /// <returns>
        ///     The parameter value.
        /// </returns>
        public virtual object RemoveParameter(string name)
        {
            Check.NotEmpty(name, nameof(name));

            var value = _parameterValues[name];

            _parameterValues.Remove(name);

            return value;
        }

        /// <summary>
        ///     Notify the state manager that a tracking query is starting.
        /// </summary>
        public virtual void BeginTrackingQuery() => StateManager.BeginTrackingQuery();

        public virtual InternalEntityEntry StartTracking(
            IEntityType entityType,
            object entity,
            ValueBuffer valueBuffer)
            => StateManager.StartTrackingFromQuery(entityType, entity, valueBuffer, handledForeignKeys: null);
    }
}
