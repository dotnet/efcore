// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using JetBrains.Annotations;

#nullable enable

namespace Microsoft.EntityFrameworkCore.Diagnostics
{
    /// <summary>
    ///     A <see cref="DiagnosticSource" /> event payload class for Cosmos read-item events.
    /// </summary>
    public class CosmosReadItemEventData : EventData
    {
        /// <summary>
        ///     Constructs the event payload.
        /// </summary>
        /// <param name="eventDefinition"> The event definition. </param>
        /// <param name="messageGenerator"> A delegate that generates a log message for this event. </param>
        /// <param name="resourceId"> The ID of the resource being read. </param>
        /// <param name="containerId"> The ID of the Cosmos container being queried. </param>
        /// <param name="partitionKey"> The key of the Cosmos partition that the query is using. </param>
        /// <param name="logSensitiveData"> Indicates whether or not the application allows logging of sensitive data. </param>
        public CosmosReadItemEventData(
            [NotNull] EventDefinitionBase eventDefinition,
            [NotNull] Func<EventDefinitionBase, EventData, string> messageGenerator,
            [NotNull] string resourceId,
            [NotNull] string containerId,
            [CanBeNull] string? partitionKey,
            bool logSensitiveData)
            : base(eventDefinition, messageGenerator)
        {
            ResourceId = resourceId;
            ContainerId = containerId;
            PartitionKey = partitionKey;
            LogSensitiveData = logSensitiveData;
        }

        /// <summary>
        ///     The ID of the Cosmos container being queried.
        /// </summary>
        public virtual string ContainerId { get; }

        /// <summary>
        ///     The ID of the resource being read.
        /// </summary>
        public virtual string ResourceId { get; }

        /// <summary>
        ///     The key of the Cosmos partition that the query is using.
        /// </summary>
        public virtual string? PartitionKey { get; }

        /// <summary>
        ///     Indicates whether or not the application allows logging of sensitive data.
        /// </summary>
        public virtual bool LogSensitiveData { get; }
    }
}
