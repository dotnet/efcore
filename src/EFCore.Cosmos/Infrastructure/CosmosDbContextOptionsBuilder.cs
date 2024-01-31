// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel;
using System.Net;
using Microsoft.EntityFrameworkCore.Cosmos.Infrastructure.Internal;

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     Allows Cosmos specific configuration to be performed on <see cref="DbContextOptions" />.
/// </summary>
/// <remarks>
///     <para>
///         Instances of this class are returned from a call to
///         <see cref="O:CosmosDbContextOptionsExtensions.UseCosmos{TContext}" />
///         and it is not designed to be directly constructed in your application code.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
///         <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
///     </para>
/// </remarks>
public class CosmosDbContextOptionsBuilder : ICosmosDbContextOptionsBuilderInfrastructure
{
    private readonly DbContextOptionsBuilder _optionsBuilder;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CosmosDbContextOptionsBuilder" /> class.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="optionsBuilder">The options builder.</param>
    public CosmosDbContextOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        _optionsBuilder = optionsBuilder;
    }

    /// <inheritdoc />
    DbContextOptionsBuilder ICosmosDbContextOptionsBuilderInfrastructure.OptionsBuilder
        => _optionsBuilder;

    /// <summary>
    ///     Configures the context to use the provided <see cref="IExecutionStrategy" />.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="getExecutionStrategy">A function that returns a new instance of an execution strategy.</param>
    public virtual CosmosDbContextOptionsBuilder ExecutionStrategy(
        Func<ExecutionStrategyDependencies, IExecutionStrategy> getExecutionStrategy)
        => WithOption(e => e.WithExecutionStrategyFactory(Check.NotNull(getExecutionStrategy, nameof(getExecutionStrategy))));

    /// <summary>
    ///     Configures the context to use the provided geo-replicated region.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="region">Azure Cosmos DB region name.</param>
    public virtual CosmosDbContextOptionsBuilder Region(string region)
        => WithOption(e => e.WithRegion(Check.NotNull(region, nameof(region))));

    /// <summary>
    ///     Configures the context to use the provided preferred regions for geo-replicated database accounts.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="regions">A list of Azure Cosmos DB region names.</param>
    public virtual CosmosDbContextOptionsBuilder PreferredRegions(IReadOnlyList<string> regions)
        => WithOption(e => e.WithPreferredRegions(Check.NotNull(regions, nameof(regions))));

    /// <summary>
    ///     Limits the operations to the provided endpoint.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="enable"><see langword="true" /> to limit the operations to the provided endpoint.</param>
    public virtual CosmosDbContextOptionsBuilder LimitToEndpoint(bool enable = true)
        => WithOption(e => e.WithLimitToEndpoint(Check.NotNull(enable, nameof(enable))));

    /// <summary>
    ///     Configures the context to use a specific <see cref="HttpClient" /> factory.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Use
    ///         <see href="https://docs.microsoft.com/dotnet/csharp/language-reference/operators/lambda-expressions">
    ///             static lambda expressions
    ///         </see>
    ///         to avoid creating multiple instances.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///         <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    ///     </para>
    /// </remarks>
    /// <param name="httpClientFactory">A function that returns an <see cref="HttpClient" />.</param>
    public virtual CosmosDbContextOptionsBuilder HttpClientFactory(Func<HttpClient>? httpClientFactory)
        => WithOption(e => e.WithHttpClientFactory(Check.NotNull(httpClientFactory, nameof(httpClientFactory))));

    /// <summary>
    ///     Configures the context to use the provided connection mode.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="connectionMode">Azure Cosmos DB connection mode.</param>
    public virtual CosmosDbContextOptionsBuilder ConnectionMode(ConnectionMode connectionMode)
        => WithOption(e => e.WithConnectionMode(Check.NotNull(connectionMode, nameof(connectionMode))));

    /// <summary>
    ///     Configures the proxy information used for web requests.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="proxy">The proxy information used for web requests.</param>
    public virtual CosmosDbContextOptionsBuilder WebProxy(IWebProxy proxy)
        => WithOption(e => e.WithWebProxy(Check.NotNull(proxy, nameof(proxy))));

    /// <summary>
    ///     Configures the timeout when connecting to the Azure Cosmos DB service.
    ///     The number specifies the time to wait for response to come back from network peer.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="timeout">Request timeout.</param>
    public virtual CosmosDbContextOptionsBuilder RequestTimeout(TimeSpan timeout)
        => WithOption(e => e.WithRequestTimeout(Check.NotNull(timeout, nameof(timeout))));

    /// <summary>
    ///     Configures the amount of time allowed for trying to establish a connection.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="timeout">Open TCP connection timeout.</param>
    public virtual CosmosDbContextOptionsBuilder OpenTcpConnectionTimeout(TimeSpan timeout)
        => WithOption(e => e.WithOpenTcpConnectionTimeout(Check.NotNull(timeout, nameof(timeout))));

    /// <summary>
    ///     Configures the amount of idle time after which unused connections are closed.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="timeout">Idle connection timeout.</param>
    public virtual CosmosDbContextOptionsBuilder IdleTcpConnectionTimeout(TimeSpan timeout)
        => WithOption(e => e.WithIdleTcpConnectionTimeout(Check.NotNull(timeout, nameof(timeout))));

    /// <summary>
    ///     Configures the maximum number of concurrent connections allowed for the target service endpoint
    ///     in the Azure Cosmos DB service.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="connectionLimit">The maximum number of concurrent connections allowed.</param>
    public virtual CosmosDbContextOptionsBuilder GatewayModeMaxConnectionLimit(int connectionLimit)
        => WithOption(e => e.WithGatewayModeMaxConnectionLimit(Check.NotNull(connectionLimit, nameof(connectionLimit))));

    /// <summary>
    ///     Configures the maximum number of TCP connections that may be opened to each Cosmos DB back-end.
    ///     Together with MaxRequestsPerTcpConnection, this setting limits the number of requests that are
    ///     simultaneously sent to a single Cosmos DB back-end (MaxRequestsPerTcpConnection x MaxTcpConnectionPerEndpoint).
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="connectionLimit">The maximum number of TCP connections that may be opened to each Cosmos DB back-end.</param>
    public virtual CosmosDbContextOptionsBuilder MaxTcpConnectionsPerEndpoint(int connectionLimit)
        => WithOption(e => e.WithMaxTcpConnectionsPerEndpoint(Check.NotNull(connectionLimit, nameof(connectionLimit))));

    /// <summary>
    ///     Configures the number of requests allowed simultaneously over a single TCP connection.
    ///     When more requests are in flight simultaneously, the direct/TCP client will open additional connections.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="requestLimit">The number of requests allowed simultaneously over a single TCP connection.</param>
    public virtual CosmosDbContextOptionsBuilder MaxRequestsPerTcpConnection(int requestLimit)
        => WithOption(e => e.WithMaxRequestsPerTcpConnection(Check.NotNull(requestLimit, nameof(requestLimit))));

    /// <summary>
    ///     Sets the boolean to only return the headers and status code in the Cosmos DB response for write item operation
    ///     like Create, Upsert, Patch and Replace. Setting the option to false will cause the response to have a null resource.
    ///     This reduces networking and CPU load by not sending the resource back over the network and serializing it on the client.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-dbcontext-options">Using DbContextOptions</see>, and
    ///     <see href="https://aka.ms/efcore-docs-cosmos">Accessing Azure Cosmos DB with EF Core</see> for more information and examples.
    /// </remarks>
    /// <param name="enabled"><see langword="false" /> to have null resource</param>
    public virtual CosmosDbContextOptionsBuilder ContentResponseOnWriteEnabled(bool enabled = true)
        => WithOption(e => e.ContentResponseOnWriteEnabled(Check.NotNull(enabled, nameof(enabled))));

    /// <summary>
    ///     Sets an option by cloning the extension used to store the settings. This ensures the builder
    ///     does not modify options that are already in use elsewhere.
    /// </summary>
    /// <param name="setAction">An action to set the option.</param>
    /// <returns>The same builder instance so that multiple calls can be chained.</returns>
    protected virtual CosmosDbContextOptionsBuilder WithOption(Func<CosmosOptionsExtension, CosmosOptionsExtension> setAction)
    {
        ((IDbContextOptionsBuilderInfrastructure)_optionsBuilder).AddOrUpdateExtension(
            setAction(_optionsBuilder.Options.FindExtension<CosmosOptionsExtension>() ?? new CosmosOptionsExtension()));

        return this;
    }

    #region Hidden System.Object members

    /// <summary>
    ///     Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string? ToString()
        => base.ToString();

    /// <summary>
    ///     Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <param name="obj">The object to compare with the current object.</param>
    /// <returns><see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object? obj)
        => base.Equals(obj);

    /// <summary>
    ///     Serves as the default hash function.
    /// </summary>
    /// <returns>A hash code for the current object.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
        => base.GetHashCode();

    #endregion
}
