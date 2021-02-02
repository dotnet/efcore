// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for Cosmos query events.
    /// </summary>
    public class CosmosQueryEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="containerId"> The ID of the Cosmos container being queried. </param>
        /// <param name="partitionKey"> The key of the Cosmos partition that the query is using. </param>
        /// <param name="parameters"> Name/values for each parameter in the Cosmos Query. </param>
        /// <param name="querySql"> The SQL representing the query. </param>
        /// <param name="logSensitiveData"> Indicates whether or not the application allows logging of sensitive data. </param>
        public CosmosQueryEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] string containerId,
            [CanBeNull] string? partitionKey,
            [NotNull] IReadOnlyList<(string Name, object? Value)> parameters,
            [NotNull] string querySql,
            bool logSensitiveData)
            : base(eventDefinition, messageGenerator)
        {
            ContainerId = containerId;
            PartitionKey = partitionKey;
            Parameters = parameters;
            QuerySql = querySql;
            LogSensitiveData = logSensitiveData;
        }

        /// <summary>
        ///     The ID of the Cosmos container being queried.
        /// </summary>
        public virtual string ContainerId { get; }

        /// <summary>
        ///     The key of the Cosmos partition that the query is using.
        /// </summary>
        public virtual string? PartitionKey { get; }

        /// <summary>
        ///     Name/values for each parameter in the Cosmos Query.
        /// </summary>
        public virtual IReadOnlyList<(string Name, object? Value)> Parameters { get; }

        /// <summary>
        ///     The SQL representing the query.
        /// </summary>
        public virtual string QuerySql { get; }

        /// <summary>
        ///     Indicates whether or not the application allows logging of sensitive data.
        /// </summary>
        public virtual bool LogSensitiveData { get; }
    }
}
