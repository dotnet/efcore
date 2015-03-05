using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace StackExchange.Redis
{
    /// <summary>
    /// Provides configuration controls of a redis server
    /// </summary>
    public partial interface IServer : IRedis
    {
        /// <summary>
        /// Gets the cluster configuration associated with this server, if known
        /// </summary>
        ClusterConfiguration ClusterConfiguration { get; }

        /// <summary>
        /// Gets the address of the connected server
        /// </summary>
        EndPoint EndPoint { get; }

        /// <summary>
        /// Gets the features available to the connected server
        /// </summary>
        RedisFeatures Features { get; }

        /// <summary>
        /// Gets whether the connection to the server is active and usable
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets whether the connected server is a replica / slave
        /// </summary>
        bool IsSlave { get; }

        /// <summary>
        /// Gets the operating mode of the connected server
        /// </summary>
        ServerType ServerType { get; }

        /// <summary>
        /// Gets the version of the connected server
        /// </summary>
        Version Version { get; }

        /// <summary>
        /// The CLIENT KILL command closes a given client connection identified by ip:port.
        /// The ip:port should match a line returned by the CLIENT LIST command.
        /// Due to the single-treaded nature of Redis, it is not possible to kill a client connection while it is executing a command.From the client point of view, the connection can never be closed in the middle of the execution of a command.However, the client will notice the connection has been closed only when the next command is sent (and results in network error).
        /// </summary>
        /// <remarks>http://redis.io/commands/client-kill</remarks>
        void ClientKill(EndPoint endpoint, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// The CLIENT KILL command closes a given client connection identified by ip:port.
        /// The ip:port should match a line returned by the CLIENT LIST command.
        /// Due to the single-treaded nature of Redis, it is not possible to kill a client connection while it is executing a command.From the client point of view, the connection can never be closed in the middle of the execution of a command.However, the client will notice the connection has been closed only when the next command is sent (and results in network error).
        /// </summary>
        /// <remarks>http://redis.io/commands/client-kill</remarks>
        Task ClientKillAsync(EndPoint endpoint, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// The CLIENT KILL command closes multiple connections that match the specified filters
        /// </summary>
        /// <returns>the number of clients killed.</returns>
        /// <remarks>http://redis.io/commands/client-kill</remarks>
        long ClientKill(long? id = null, ClientType? clientType = null, EndPoint endpoint = null, bool skipMe = true, CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// The CLIENT KILL command closes multiple connections that match the specified filters
        /// </summary>
        /// <returns>the number of clients killed.</returns>
        /// <remarks>http://redis.io/commands/client-kill</remarks>
        Task<long> ClientKillAsync(long? id = null, ClientType? clientType = null, EndPoint endpoint = null, bool skipMe = true, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// The CLIENT LIST command returns information and statistics about the client connections server in a mostly human readable format.
        /// </summary>
        /// <remarks>http://redis.io/commands/client-list</remarks>
        ClientInfo[] ClientList(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// The CLIENT LIST command returns information and statistics about the client connections server in a mostly human readable format.
        /// </summary>
        /// <remarks>http://redis.io/commands/client-list</remarks>
        Task<ClientInfo[]> ClientListAsync(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Obtains the current CLUSTER NODES output from a cluster server
        /// </summary>
        ClusterConfiguration ClusterNodes(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Obtains the current CLUSTER NODES output from a cluster server
        /// </summary>
        Task<ClusterConfiguration> ClusterNodesAsync(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Obtains the current raw CLUSTER NODES output from a cluster server
        /// </summary>
        string ClusterNodesRaw(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Obtains the current raw CLUSTER NODES output from a cluster server
        /// </summary>
        Task<string> ClusterNodesRawAsync(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Get all configuration parameters matching the specified pattern.
        /// </summary>
        /// <returns>All matching configuration parameters.</returns>
        /// <remarks>http://redis.io/commands/config-get</remarks>
        KeyValuePair<string, string>[] ConfigGet(RedisValue pattern = default(RedisValue), CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Get all configuration parameters matching the specified pattern.
        /// </summary>
        /// <returns>All matching configuration parameters.</returns>
        /// <remarks>http://redis.io/commands/config-get</remarks>
        Task<KeyValuePair<string, string>[]> ConfigGetAsync(RedisValue pattern = default(RedisValue), CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Resets the statistics reported by Redis using the INFO command.
        /// </summary>
        /// <remarks>http://redis.io/commands/config-resetstat</remarks>
        void ConfigResetStatistics(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Resets the statistics reported by Redis using the INFO command.
        /// </summary>
        /// <remarks>http://redis.io/commands/config-resetstat</remarks>
        Task ConfigResetStatisticsAsync(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// The CONFIG REWRITE command rewrites the redis.conf file the server was started with, applying the minimal changes needed to make it reflecting the configuration currently used by the server, that may be different compared to the original one because of the use of the CONFIG SET command.
        /// </summary>
        /// <remarks>http://redis.io/commands/config-rewrite</remarks>
        void ConfigRewrite(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// The CONFIG REWRITE command rewrites the redis.conf file the server was started with, applying the minimal changes needed to make it reflecting the configuration currently used by the server, that may be different compared to the original one because of the use of the CONFIG SET command.
        /// </summary>
        /// <remarks>http://redis.io/commands/config-rewrite</remarks>
        Task ConfigRewriteAsync(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// The CONFIG SET command is used in order to reconfigure the server at runtime without the need to restart Redis. You can change both trivial parameters or switch from one to another persistence option using this command.
        /// </summary>
        /// <remarks>http://redis.io/commands/config-set</remarks>
        void ConfigSet(RedisValue setting, RedisValue value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// The CONFIG SET command is used in order to reconfigure the server at runtime without the need to restart Redis. You can change both trivial parameters or switch from one to another persistence option using this command.
        /// </summary>
        /// <remarks>http://redis.io/commands/config-set</remarks>
        Task ConfigSetAsync(RedisValue setting, RedisValue value, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Return the number of keys in the database.
        /// </summary>
        /// <remarks>http://redis.io/commands/dbsize</remarks>
        long DatabaseSize(int database = 0, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Return the number of keys in the database.
        /// </summary>
        /// <remarks>http://redis.io/commands/dbsize</remarks>
        Task<long> DatabaseSizeAsync(int database = 0, CommandFlags flags = CommandFlags.None);


        /// <summary>
        /// Delete all the keys of all databases on the server.
        /// </summary>
        /// <remarks>http://redis.io/commands/flushall</remarks>
        void FlushAllDatabases(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Delete all the keys of all databases on the server.
        /// </summary>
        /// <remarks>http://redis.io/commands/flushall</remarks>
        Task FlushAllDatabasesAsync(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Delete all the keys of the database.
        /// </summary>
        /// <remarks>http://redis.io/commands/flushdb</remarks>
        void FlushDatabase(int database = 0, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Delete all the keys of the database.
        /// </summary>
        /// <remarks>http://redis.io/commands/flushdb</remarks>
        Task FlushDatabaseAsync(int database = 0, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Get summary statistics associates with this server
        /// </summary>
        ServerCounters GetCounters();
        /// <summary>
        /// The INFO command returns information and statistics about the server in a format that is simple to parse by computers and easy to read by humans.
        /// </summary>
        /// <remarks>http://redis.io/commands/info</remarks>
        IGrouping<string, KeyValuePair<string, string>>[] Info(RedisValue section = default(RedisValue), CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// The INFO command returns information and statistics about the server in a format that is simple to parse by computers and easy to read by humans.
        /// </summary>
        /// <remarks>http://redis.io/commands/info</remarks>
        Task<IGrouping<string, KeyValuePair<string, string>>[]> InfoAsync(RedisValue section = default(RedisValue), CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// The INFO command returns information and statistics about the server in a format that is simple to parse by computers and easy to read by humans.
        /// </summary>
        /// <remarks>http://redis.io/commands/info</remarks>
        string InfoRaw(RedisValue section = default(RedisValue), CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// The INFO command returns information and statistics about the server in a format that is simple to parse by computers and easy to read by humans.
        /// </summary>
        /// <remarks>http://redis.io/commands/info</remarks>
        Task<string> InfoRawAsync(RedisValue section = default(RedisValue), CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns all keys matching pattern; the KEYS or SCAN commands will be used based on the server capabilities.
        /// </summary>
        /// <remarks>Warning: consider KEYS as a command that should only be used in production environments with extreme care.</remarks>
        /// <remarks>http://redis.io/commands/keys</remarks>
        /// <remarks>http://redis.io/commands/scan</remarks>
        IEnumerable<RedisKey> Keys(int database, RedisValue pattern, int pageSize, CommandFlags flags);

        /// <summary>
        /// Returns all keys matching pattern; the KEYS or SCAN commands will be used based on the server capabilities; note: to resume an iteration via <i>cursor</i>, cast the original enumerable or enumerator to <i>IScanningCursor</i>.
        /// </summary>
        /// <remarks>Warning: consider KEYS as a command that should only be used in production environments with extreme care.</remarks>
        /// <remarks>http://redis.io/commands/keys</remarks>
        /// <remarks>http://redis.io/commands/scan</remarks>
        IEnumerable<RedisKey> Keys(int database = 0, RedisValue pattern = default(RedisValue), int pageSize = RedisBase.CursorUtils.DefaultPageSize, long cursor = RedisBase.CursorUtils.Origin, int pageOffset = 0, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Return the time of the last DB save executed with success. A client may check if a BGSAVE command succeeded reading the LASTSAVE value, then issuing a BGSAVE command and checking at regular intervals every N seconds if LASTSAVE changed.
        /// </summary>
        /// <remarks>http://redis.io/commands/lastsave</remarks>
        DateTime LastSave(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Return the time of the last DB save executed with success. A client may check if a BGSAVE command succeeded reading the LASTSAVE value, then issuing a BGSAVE command and checking at regular intervals every N seconds if LASTSAVE changed.
        /// </summary>
        /// <remarks>http://redis.io/commands/lastsave</remarks>
        Task<DateTime> LastSaveAsync(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Promote the selected node to be master
        /// </summary>
        void MakeMaster(ReplicationChangeOptions options, TextWriter log = null);

        /// <summary>
        /// Explicitly request the database to persist the current state to disk
        /// </summary>
        /// <remarks>http://redis.io/commands/bgrewriteaof</remarks>
        /// <remarks>http://redis.io/commands/bgsave</remarks>
        /// <remarks>http://redis.io/commands/save</remarks>
        /// <remarks>http://redis.io/topics/persistence</remarks>
        void Save(SaveType type, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Explicitly request the database to persist the current state to disk
        /// </summary>
        /// <remarks>http://redis.io/commands/bgrewriteaof</remarks>
        /// <remarks>http://redis.io/commands/bgsave</remarks>
        /// <remarks>http://redis.io/commands/save</remarks>
        /// <remarks>http://redis.io/topics/persistence</remarks>
        Task SaveAsync(SaveType type, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Inidicates whether the specified script is defined on the server
        /// </summary>
        bool ScriptExists(string script, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Inidicates whether the specified script hash is defined on the server
        /// </summary>
        bool ScriptExists(byte[] sha1, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Inidicates whether the specified script is defined on the server
        /// </summary>
        Task<bool> ScriptExistsAsync(string script, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Inidicates whether the specified script hash is defined on the server
        /// </summary>
        Task<bool> ScriptExistsAsync(byte[] sha1, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Removes all cached scripts on this server
        /// </summary>
        void ScriptFlush(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Removes all cached scripts on this server
        /// </summary>
        Task ScriptFlushAsync(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Explicitly defines a script on the server
        /// </summary>
        byte[] ScriptLoad(string script, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Explicitly defines a script on the server
        /// </summary>
        Task<byte[]> ScriptLoadAsync(string script, CommandFlags flags = CommandFlags.None);

        /// <summary>Asks the redis server to shutdown, killing all connections. Please FULLY read the notes on the SHUTDOWN command.</summary>
        /// <remarks>http://redis.io/commands/shutdown</remarks>
        void Shutdown(ShutdownMode shutdownMode = ShutdownMode.Default, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// The SLAVEOF command can change the replication settings of a slave on the fly. If a Redis server is already acting as slave, specifying a null master will turn off the replication, turning the Redis server into a MASTER. Specifying a non-null master will make the server a slave of another server listening at the specified hostname and port.
        /// </summary>
        /// <remarks>http://redis.io/commands/slaveof</remarks>
        void SlaveOf(EndPoint master, CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// The SLAVEOF command can change the replication settings of a slave on the fly. If a Redis server is already acting as slave, specifying a null master will turn off the replication, turning the Redis server into a MASTER. Specifying a non-null master will make the server a slave of another server listening at the specified hostname and port.
        /// </summary>
        /// <remarks>http://redis.io/commands/slaveof</remarks>
        Task SlaveOfAsync(EndPoint master, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// To read the slow log the SLOWLOG GET command is used, that returns every entry in the slow log. It is possible to return only the N most recent entries passing an additional argument to the command (for instance SLOWLOG GET 10).
        /// </summary>
        /// <remarks>http://redis.io/commands/slowlog</remarks>
        CommandTrace[] SlowlogGet(int count = 0, CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// To read the slow log the SLOWLOG GET command is used, that returns every entry in the slow log. It is possible to return only the N most recent entries passing an additional argument to the command (for instance SLOWLOG GET 10).
        /// </summary>
        /// <remarks>http://redis.io/commands/slowlog</remarks>
        Task<CommandTrace[]> SlowlogGetAsync(int count = 0, CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// You can reset the slow log using the SLOWLOG RESET command. Once deleted the information is lost forever.
        /// </summary>
        /// <remarks>http://redis.io/commands/slowlog</remarks>
        void SlowlogReset(CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// You can reset the slow log using the SLOWLOG RESET command. Once deleted the information is lost forever.
        /// </summary>
        /// <remarks>http://redis.io/commands/slowlog</remarks>
        Task SlowlogResetAsync(CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// Lists the currently active channels. An active channel is a Pub/Sub channel with one ore more subscribers (not including clients subscribed to patterns).
        /// </summary>
        /// <returns> a list of active channels, optionally matching the specified pattern.</returns>
        /// <remarks>http://redis.io/commands/pubsub</remarks>
        RedisChannel[] SubscriptionChannels(RedisChannel pattern = default(RedisChannel), CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Lists the currently active channels. An active channel is a Pub/Sub channel with one ore more subscribers (not including clients subscribed to patterns).
        /// </summary>
        /// <returns> a list of active channels, optionally matching the specified pattern.</returns>
        /// <remarks>http://redis.io/commands/pubsub</remarks>
        Task<RedisChannel[]> SubscriptionChannelsAsync(RedisChannel pattern = default(RedisChannel), CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the number of subscriptions to patterns (that are performed using the PSUBSCRIBE command). Note that this is not just the count of clients subscribed to patterns but the total number of patterns all the clients are subscribed to.
        /// </summary>
        /// <returns>the number of patterns all the clients are subscribed to.</returns>
        /// <remarks>http://redis.io/commands/pubsub</remarks>
        long SubscriptionPatternCount(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the number of subscriptions to patterns (that are performed using the PSUBSCRIBE command). Note that this is not just the count of clients subscribed to patterns but the total number of patterns all the clients are subscribed to.
        /// </summary>
        /// <returns>the number of patterns all the clients are subscribed to.</returns>
        /// <remarks>http://redis.io/commands/pubsub</remarks>
        Task<long> SubscriptionPatternCountAsync(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the number of subscribers (not counting clients subscribed to patterns) for the specified channel.
        /// </summary>
        /// <remarks>http://redis.io/commands/pubsub</remarks>
        long SubscriptionSubscriberCount(RedisChannel channel, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the number of subscribers (not counting clients subscribed to patterns) for the specified channel.
        /// </summary>
        /// <remarks>http://redis.io/commands/pubsub</remarks>
        Task<long> SubscriptionSubscriberCountAsync(RedisChannel channel, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// The TIME command returns the current server time.
        /// </summary>
        /// <returns>The server's current time.</returns>
        /// <remarks>http://redis.io/commands/time</remarks>
        DateTime Time(CommandFlags flags = CommandFlags.None);
        /// <summary>
        /// The TIME command returns the current server time.
        /// </summary>
        /// <returns>The server's current time.</returns>
        /// <remarks>http://redis.io/commands/time</remarks>
        Task<DateTime> TimeAsync(CommandFlags flags = CommandFlags.None);

        #region Sentinel

        /// <summary>
        /// Returns the ip and port number of the master with that name. 
        /// If a failover is in progress or terminated successfully for this master it returns the address and port of the promoted slave.
        /// </summary>
        /// <param name="serviceName">the sentinel service name</param>
        /// <param name="flags"></param>
        /// <returns>the master ip and port</returns>
        /// <remarks>http://redis.io/topics/sentinel</remarks>
        EndPoint SentinelGetMasterAddressByName(string serviceName, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Returns the ip and port number of the master with that name. 
        /// If a failover is in progress or terminated successfully for this master it returns the address and port of the promoted slave.
        /// </summary>
        /// <param name="serviceName">the sentinel service name</param>
        /// <param name="flags"></param>
        /// <returns>the master ip and port</returns>
        /// <remarks>http://redis.io/topics/sentinel</remarks>
        Task<EndPoint> SentinelGetMasterAddressByNameAsync(string serviceName, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Show the state and info of the specified master.
        /// </summary>
        /// <param name="serviceName">the sentinel service name</param>
        /// <param name="flags"></param>
        /// <returns>the master state as KeyValuePairs</returns>
        /// <remarks>http://redis.io/topics/sentinel</remarks>
        KeyValuePair<string, string>[] SentinelMaster(string serviceName, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Force a failover as if the master was not reachable, and without asking for agreement to other Sentinels 
        /// (however a new version of the configuration will be published so that the other Sentinels will update their configurations).
        /// </summary>
        /// <param name="serviceName">the sentinel service name</param>
        /// <param name="flags"></param>
        /// <returns>the master state as KeyValuePairs</returns>
        /// <remarks>http://redis.io/topics/sentinel</remarks>
        Task<KeyValuePair<string, string>[]> SentinelMasterAsync(string serviceName, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Show a list of monitored masters and their state.
        /// </summary>
        /// <param name="flags"></param>
        /// <returns>an array of master state KeyValuePair arrays</returns>
        /// <remarks>http://redis.io/topics/sentinel</remarks>
        KeyValuePair<string, string>[][] SentinelMasters(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Show a list of monitored masters and their state.
        /// </summary>
        /// <param name="flags"></param>
        /// <returns>an array of master state KeyValuePair arrays</returns>
        /// <remarks>http://redis.io/topics/sentinel</remarks>
        Task<KeyValuePair<string, string>[][]> SentinelMastersAsync(CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Show a list of slaves for this master, and their state.
        /// </summary>
        /// <param name="serviceName">the sentinel service name</param>
        /// <param name="flags"></param>
        /// <returns>an array of slave state KeyValuePair arrays</returns>
        /// <remarks>http://redis.io/topics/sentinel</remarks>
        KeyValuePair<string, string>[][] SentinelSlaves(string serviceName, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Show a list of slaves for this master, and their state.
        /// </summary>
        /// <param name="serviceName">the sentinel service name</param>
        /// <param name="flags"></param>
        /// <returns>an array of slave state KeyValuePair arrays</returns>
        /// <remarks>http://redis.io/topics/sentinel</remarks>
        Task<KeyValuePair<string, string>[][]> SentinelSlavesAsync(string serviceName, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Force a failover as if the master was not reachable, and without asking for agreement to other Sentinels 
        /// (however a new version of the configuration will be published so that the other Sentinels will update their configurations).
        /// </summary>
        /// <param name="serviceName">the sentinel service name</param>
        /// <param name="flags"></param>
        /// <remarks>http://redis.io/topics/sentinel</remarks>
        void SentinelFailover(string serviceName, CommandFlags flags = CommandFlags.None);

        /// <summary>
        /// Force a failover as if the master was not reachable, and without asking for agreement to other Sentinels 
        /// (however a new version of the configuration will be published so that the other Sentinels will update their configurations).
        /// </summary>
        /// <param name="serviceName">the sentinel service name</param>
        /// <param name="flags"></param>
        /// <remarks>http://redis.io/topics/sentinel</remarks>
        Task SentinelFailoverAsync(string serviceName, CommandFlags flags = CommandFlags.None);

        #endregion
    }



}