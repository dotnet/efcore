using System;
using System.Net;
using System.Threading.Tasks;

namespace StackExchange.Redis
{
    /// <summary>
    /// A redis connection used as the subscriber in a pub/sub scenario
    /// </summary>
    public interface ISubscriber : IRedis
    {

        /// <summary>
        /// Inidicate exactly which redis server we are talking to
        /// </summary>
        [IgnoreNamePrefix]
        EndPoint IdentifyEndpoint(RedisChannel channel, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Inidicate exactly which redis server we are talking to
        /// </summary>
        [IgnoreNamePrefix]
        Task<EndPoint> IdentifyEndpointAsync(RedisChannel channel, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Indicates whether the instance can communicate with the server;
        /// if a channel is specified, the existing subscription map is queried to
        /// resolve the server responsible for that subscription - otherwise the
        /// server is chosen aribtraily from the masters.
        /// </summary>
        bool IsConnected(RedisChannel channel = default(RedisChannel));

        /// <summary>
        /// Posts a message to the given channel.
        /// </summary>
        /// <returns>the number of clients that received the message.</returns>
        /// <remarks>http://redis.io/commands/publish</remarks>
        long Publish(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// Posts a message to the given channel.
        /// </summary>
        /// <returns>the number of clients that received the message.</returns>
        /// <remarks>http://redis.io/commands/publish</remarks>
        Task<long> PublishAsync(RedisChannel channel, RedisValue message, CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// Subscribe to perform some operation when a change to the preferred/active node is broadcast.
        /// </summary>
        /// <remarks>http://redis.io/commands/subscribe</remarks>
        /// <remarks>http://redis.io/commands/psubscribe</remarks>
        void Subscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Subscribe to perform some operation when a change to the preferred/active node is broadcast.
        /// </summary>
        /// <remarks>http://redis.io/commands/subscribe</remarks>
        /// <remarks>http://redis.io/commands/psubscribe</remarks>
        Task SubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> handler, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Inidicate to which redis server we are actively subscribed for a given channel; returns null if
        /// the channel is not actively subscribed
        /// </summary>
        [IgnoreNamePrefix]
        EndPoint SubscribedEndpoint(RedisChannel channel);

        /// <summary>
        /// Unsubscribe from a specified message channel; note; if no handler is specified, the subscription is cancelled regardless
        /// of the subscribers; if a handler is specified, the subscription is only cancelled if this handler is the 
        /// last handler remaining against the channel
        /// </summary>
        /// <remarks>http://redis.io/commands/unsubscribe</remarks>
        /// <remarks>http://redis.io/commands/punsubscribe</remarks>
        void Unsubscribe(RedisChannel channel, Action<RedisChannel, RedisValue> handler = null, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Unsubscribe all subscriptions on this instance
        /// </summary>
        /// <remarks>http://redis.io/commands/unsubscribe</remarks>
        /// <remarks>http://redis.io/commands/punsubscribe</remarks>
        void UnsubscribeAll(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Unsubscribe all subscriptions on this instance
        /// </summary>
        /// <remarks>http://redis.io/commands/unsubscribe</remarks>
        /// <remarks>http://redis.io/commands/punsubscribe</remarks>
        Task UnsubscribeAllAsync(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Unsubscribe from a specified message channel; note; if no handler is specified, the subscription is cancelled regardless
        /// of the subscribers; if a handler is specified, the subscription is only cancelled if this handler is the 
        /// last handler remaining against the channel
        /// </summary>
        /// <remarks>http://redis.io/commands/unsubscribe</remarks>
        /// <remarks>http://redis.io/commands/punsubscribe</remarks>
        Task UnsubscribeAsync(RedisChannel channel, Action<RedisChannel, RedisValue> handler = null, CommandFlags flags = CommandFlags.None);
    }
}