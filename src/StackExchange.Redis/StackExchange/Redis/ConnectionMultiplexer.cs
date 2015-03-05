using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if NET40
using Microsoft.Runtime.CompilerServices;
#else
using System.Runtime.CompilerServices;
#endif

namespace StackExchange.Redis
{
    internal static partial class TaskExtensions
    {
        private static readonly Action<Task> observeErrors = ObverveErrors;
        private static void ObverveErrors(this Task task)
        {
            if (task != null) GC.KeepAlive(task.Exception);
        }

        public  static Task ObserveErrors(this Task task)
        {
            if (task != null) task.ContinueWith(observeErrors, TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }
        public static Task<T> ObserveErrors<T>(this Task<T> task)
        {
            if (task != null) task.ContinueWith(observeErrors, TaskContinuationOptions.OnlyOnFaulted);
            return task;
        }

        public static ConfiguredTaskAwaitable ForAwait(this Task task)
        {
            return task.ConfigureAwait(false);
        }
        public static ConfiguredTaskAwaitable<T> ForAwait<T>(this Task<T> task)
        {
            return task.ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Represents an inter-related group of connections to redis servers
    /// </summary>
    public sealed partial class ConnectionMultiplexer : IDisposable
    {
        /// <summary>
        /// Get summary statistics associates with this server
        /// </summary>
        public ServerCounters GetCounters()
        {
            var snapshot = serverSnapshot;

            var counters = new ServerCounters(null);
            for (int i = 0; i < snapshot.Length; i++)
            {
                counters.Add(snapshot[i].GetCounters());
            }
            unprocessableCompletionManager.GetCounters(counters.Other);
            return counters;
        }

        /// <summary>
        /// Gets the client-name that will be used on all new connections
        /// </summary>
        public string ClientName { get { return configuration.ClientName ?? Environment.MachineName; } }

        /// <summary>
        /// Gets the configuration of the connection
        /// </summary>
        public string Configuration
        {
            get { return configuration.ToString(); }
        }

        internal void OnConnectionFailed(EndPoint endpoint, ConnectionType connectionType, ConnectionFailureType failureType, Exception exception, bool reconfigure)
        {
            if (isDisposed) return;
            var handler = ConnectionFailed;
            if (handler != null)
            {
                unprocessableCompletionManager.CompleteSyncOrAsync(
                    new ConnectionFailedEventArgs(handler, this, endpoint, connectionType, failureType, exception)
                );
            }
            if (reconfigure)
            {
                ReconfigureIfNeeded(endpoint, false, "connection failed");
            }
        }
        internal void OnInternalError(Exception exception, EndPoint endpoint = null, ConnectionType connectionType = ConnectionType.None, [System.Runtime.CompilerServices.CallerMemberName] string origin = null)
        {
            try
            {
                Trace("Internal error: " + origin + ", " + exception == null ? "unknown" : exception.Message);
                if (isDisposed) return;
                var handler = InternalError;
                if (handler != null)
                {
                    unprocessableCompletionManager.CompleteSyncOrAsync(
                        new InternalErrorEventArgs(handler, this, endpoint, connectionType, exception, origin)
                    );
                }
            }
            catch
            { // our internal error event failed; whatcha gonna do, exactly?
            }
        }

        internal void OnConnectionRestored(EndPoint endpoint, ConnectionType connectionType)
        {
            if (isDisposed) return;
            var handler = ConnectionRestored;
            if (handler != null)
            {
                unprocessableCompletionManager.CompleteSyncOrAsync(
                    new ConnectionFailedEventArgs(handler, this, endpoint, connectionType, ConnectionFailureType.None, null)
                );
            }
            ReconfigureIfNeeded(endpoint, false, "connection restored");
        }


        private void OnEndpointChanged(EndPoint endpoint, EventHandler<EndPointEventArgs> handler)
        {
            if (isDisposed) return;
            if (handler != null)
            {
                unprocessableCompletionManager.CompleteSyncOrAsync(
                    new EndPointEventArgs(handler, this, endpoint)
                );
            }
        }
        internal void OnConfigurationChanged(EndPoint endpoint)
        {
            OnEndpointChanged(endpoint, ConfigurationChanged);
        }
        internal void OnConfigurationChangedBroadcast(EndPoint endpoint)
        {
            OnEndpointChanged(endpoint, ConfigurationChangedBroadcast);
        }

        /// <summary>
        /// A server replied with an error message;
        /// </summary>
        public event EventHandler<RedisErrorEventArgs> ErrorMessage;
        internal void OnErrorMessage(EndPoint endpoint, string message)
        {
            if (isDisposed) return;
            var handler = ErrorMessage;
            if (handler != null)
            {
                unprocessableCompletionManager.CompleteSyncOrAsync(
                    new RedisErrorEventArgs(handler, this, endpoint, message)
                );
            }
        }

#if !NET40
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        static void Write<T>(ZipArchive zip, string name, Task task, Action<T, StreamWriter> callback)
        {
            var entry = zip.CreateEntry(name,
#if __MonoCS__
                CompressionLevel.Fastest
#else
                CompressionLevel.Optimal
#endif
                );
            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream))
            {
                TaskStatus status = task.Status;
                switch (status)
                {
                    case TaskStatus.RanToCompletion:
                        T val = ((Task<T>)task).Result;
                        callback(val, writer);
                        break;
                    case TaskStatus.Faulted:
                        writer.WriteLine(string.Join(", ", task.Exception.InnerExceptions.Select(x => x.Message)));
                        break;
                    default:
                        writer.WriteLine(status.ToString());
                        break;
                }
            }
        }
        /// <summary>
        /// Write the configuration of all servers to an output stream
        /// </summary>
        public void ExportConfiguration(Stream destination, ExportOptions options = ExportOptions.All)
        {
            if (destination == null) throw new ArgumentNullException("destination");

            // what is possible, given the command map?
            ExportOptions mask = 0;
            if (CommandMap.IsAvailable(RedisCommand.INFO)) mask |= ExportOptions.Info;
            if (CommandMap.IsAvailable(RedisCommand.CONFIG)) mask |= ExportOptions.Config;
            if (CommandMap.IsAvailable(RedisCommand.CLIENT)) mask |= ExportOptions.Client;
            if (CommandMap.IsAvailable(RedisCommand.CLUSTER)) mask |= ExportOptions.Cluster;
            options &= mask;

            using (var zip = new ZipArchive(destination, ZipArchiveMode.Create, true))
            {
                var arr = serverSnapshot;
                foreach (var server in arr)
                {
                    const CommandFlags flags = CommandFlags.None;
                    if (!server.IsConnected) continue;
                    var api = GetServer(server.EndPoint);

                    List<Task> tasks = new List<Task>();
                    if ((options & ExportOptions.Info) != 0)
                    {
                        tasks.Add(api.InfoRawAsync(flags: flags));
                    }
                    if ((options & ExportOptions.Config) != 0)
                    {
                        tasks.Add(api.ConfigGetAsync(flags: flags));
                    }
                    if ((options & ExportOptions.Client) != 0)
                    {
                        tasks.Add(api.ClientListAsync(flags: flags));
                    }
                    if ((options & ExportOptions.Cluster) != 0)
                    {
                        tasks.Add(api.ClusterNodesRawAsync(flags: flags));
                    }

                    WaitAllIgnoreErrors(tasks.ToArray());

                    int index = 0;
                    var prefix = Format.ToString(server.EndPoint);
                    if ((options & ExportOptions.Info) != 0)
                    {
                        Write<string>(zip, prefix + "/info.txt", tasks[index++], WriteNormalizingLineEndings);
                    }
                    if ((options & ExportOptions.Config) != 0)
                    {
                        Write<KeyValuePair<string, string>[]>(zip, prefix + "/config.txt", tasks[index++], (settings, writer) =>
                        {
                            foreach (var setting in settings)
                            {
                                writer.WriteLine("{0}={1}", setting.Key, setting.Value);
                            }
                        });
                    }
                    if ((options & ExportOptions.Client) != 0)
                    {
                        Write<ClientInfo[]>(zip, prefix + "/clients.txt", tasks[index++], (clients, writer) =>
                        {
                            foreach (var client in clients)
                            {
                                writer.WriteLine(client.Raw);
                            }
                        });
                    }
                    if ((options & ExportOptions.Cluster) != 0)
                    {
                        Write<string>(zip, prefix + "/nodes.txt", tasks[index++], WriteNormalizingLineEndings);
                    }
                }
            }
        }
#endif

        internal void MakeMaster(ServerEndPoint server, ReplicationChangeOptions options, TextWriter log)
        {
            CommandMap.AssertAvailable(RedisCommand.SLAVEOF);
            if (!configuration.AllowAdmin) throw ExceptionFactory.AdminModeNotEnabled(IncludeDetailInExceptions, RedisCommand.SLAVEOF, null, server);

            if (server == null) throw new ArgumentNullException("server");
            var srv = new RedisServer(this, server, null);
            if (!srv.IsConnected) throw ExceptionFactory.NoConnectionAvailable(IncludeDetailInExceptions, RedisCommand.SLAVEOF, null, server);

            if (log == null) log = TextWriter.Null;
            CommandMap.AssertAvailable(RedisCommand.SLAVEOF);

            const CommandFlags flags = CommandFlags.NoRedirect | CommandFlags.HighPriority;
            Message msg;

            LogLocked(log, "Checking {0} is available...", Format.ToString(srv.EndPoint));
            try
            {
                srv.Ping(flags); // if it isn't happy, we're not happy
            } catch (Exception ex)
            {
                LogLocked(log, "Operation failed on {0}, aborting: {1}", Format.ToString(srv.EndPoint), ex.Message);
                throw;
            }

            var nodes = serverSnapshot;
            RedisValue newMaster = Format.ToString(server.EndPoint);

            RedisKey tieBreakerKey = default(RedisKey);
            // try and write this everywhere; don't worry if some folks reject our advances
            if ((options & ReplicationChangeOptions.SetTiebreaker) != 0 && !string.IsNullOrWhiteSpace(configuration.TieBreaker)
                && CommandMap.IsAvailable(RedisCommand.SET))
            {
                tieBreakerKey = configuration.TieBreaker;

                foreach (var node in nodes)
                {
                    if (!node.IsConnected) continue;
                    LogLocked(log, "Attempting to set tie-breaker on {0}...", Format.ToString(node.EndPoint));
                    msg = Message.Create(0, flags, RedisCommand.SET, tieBreakerKey, newMaster);
                    node.QueueDirectFireAndForget(msg, ResultProcessor.DemandOK);
                }
            }

            // deslave...
            LogLocked(log, "Making {0} a master...", Format.ToString(srv.EndPoint));
            try
            {
                srv.SlaveOf(null, flags);
            } catch (Exception ex)
            {
                LogLocked(log, "Operation failed on {0}, aborting: {1}", Format.ToString(srv.EndPoint), ex.Message);
                throw;
            }

            // also, in case it was a slave a moment ago, and hasn't got the tie-breaker yet, we re-send the tie-breaker to this one
            if (!tieBreakerKey.IsNull)
            {
                LogLocked(log, "Resending tie-breaker to {0}...", Format.ToString(server.EndPoint));
                msg = Message.Create(0, flags, RedisCommand.SET, tieBreakerKey, newMaster);
                server.QueueDirectFireAndForget(msg, ResultProcessor.DemandOK);
            }



            // try and broadcast this everywhere, to catch the maximum audience
            if ((options & ReplicationChangeOptions.Broadcast) != 0 && ConfigurationChangedChannel != null
                && CommandMap.IsAvailable(RedisCommand.PUBLISH))
            {
                RedisValue channel = ConfigurationChangedChannel;
                foreach (var node in nodes)
                {
                    if (!node.IsConnected) continue;
                    LogLocked(log, "Broadcasting via {0}...", Format.ToString(node.EndPoint));
                    msg = Message.Create(-1, flags, RedisCommand.PUBLISH, channel, newMaster);
                    node.QueueDirectFireAndForget(msg, ResultProcessor.Int64);
                }
            }


            if ((options & ReplicationChangeOptions.EnslaveSubordinates) != 0)
            {
                foreach (var node in nodes)
                {
                    if (node == server || node.ServerType != ServerType.Standalone) continue;

                    LogLocked(log, "Enslaving {0}...", Format.ToString(node.EndPoint));
                    msg = RedisServer.CreateSlaveOfMessage(server.EndPoint, flags);
                    node.QueueDirectFireAndForget(msg, ResultProcessor.DemandOK);
                }
            }

            // and reconfigure the muxer
            LogLocked(log, "Reconfiguring all endpoints...");
            if (!ReconfigureAsync(false, true, log, srv.EndPoint, "make master").ObserveErrors().Wait(5000))
            {
                LogLocked(log, "Verifying the configuration was incomplete; please verify");
            }
        }

        /// <summary>
        /// Used internally to synchronize loggine without depending on locking the log instance
        /// </summary>
        private object LogSyncLock { get { return UniqueId; } } // we know this has strong identity: readonly and unique to us

        internal void LogLocked(TextWriter log, string line)
        {
            lock (LogSyncLock) { log.WriteLine(line); }
        }
        internal void LogLocked(TextWriter log, string line, object arg)
        {
            lock (LogSyncLock) { log.WriteLine(line, arg); }
        }
        internal void LogLocked(TextWriter log, string line, object arg0, object arg1)
        {
            lock (LogSyncLock) { log.WriteLine(line, arg0, arg1); }
        }
        internal void LogLocked(TextWriter log, string line, object arg0, object arg1, object arg2)
        {
            lock (LogSyncLock) { log.WriteLine(line, arg0, arg1, arg2); }
        }
        internal void LogLocked(TextWriter log, string line, params object[] args)
        {
            lock (LogSyncLock) { log.WriteLine(line, args); }
        }

        internal void CheckMessage(Message message)
        {
            if (!configuration.AllowAdmin && message.IsAdmin)
                throw ExceptionFactory.AdminModeNotEnabled(IncludeDetailInExceptions, message.Command, message, null);
            CommandMap.AssertAvailable(message.Command);
        }

        static void WriteNormalizingLineEndings(string source, StreamWriter writer)
        {
            using (var reader = new StringReader(source))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    writer.WriteLine(line); // normalize line endings
            }
        }

        /// <summary>
        /// Raised whenever a physical connection fails
        /// </summary>
        public event EventHandler<ConnectionFailedEventArgs> ConnectionFailed;

        /// <summary>
        /// Raised whenever an internal error occurs (this is primarily for debugging)
        /// </summary>
        public event EventHandler<InternalErrorEventArgs> InternalError;

        /// <summary>
        /// Raised whenever a physical connection is established
        /// </summary>
        public event EventHandler<ConnectionFailedEventArgs> ConnectionRestored;

        /// <summary>
        /// Raised when configuration changes are detected
        /// </summary>
        public event EventHandler<EndPointEventArgs> ConfigurationChanged;

        /// <summary>
        /// Raised when nodes are explicitly requested to reconfigure via broadcast;
        /// this usually means master/slave changes
        /// </summary>
        public event EventHandler<EndPointEventArgs> ConfigurationChangedBroadcast;

        /// <summary>
        /// Gets the timeout associated with the connections
        /// </summary>
        public int TimeoutMilliseconds
        {
            get { return timeoutMilliseconds; }
        }

        /// <summary>
        /// Gets all endpoints defined on the server
        /// </summary>
        /// <returns></returns>
        public EndPoint[] GetEndPoints(bool configuredOnly = false)
        {
            if (configuredOnly) return configuration.EndPoints.ToArray();

            return Array.ConvertAll(serverSnapshot, x => x.EndPoint);
        }

        private readonly int timeoutMilliseconds;

        private readonly ConfigurationOptions configuration;


        internal bool TryResend(int hashSlot, Message message, EndPoint endpoint, bool isMoved)
        {
            return serverSelectionStrategy.TryResend(hashSlot, message, endpoint, isMoved);
        }


        /// <summary>
        /// Wait for a given asynchronous operation to complete (or timeout)
        /// </summary>
        public void Wait(Task task)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (!task.Wait(timeoutMilliseconds)) throw new TimeoutException();
        }

        /// <summary>
        /// Wait for a given asynchronous operation to complete (or timeout)
        /// </summary>

        public T Wait<T>(Task<T> task)
        {
            if (task == null) throw new ArgumentNullException("task");
            if (!task.Wait(timeoutMilliseconds)) throw new TimeoutException();
            return task.Result;
        }
        /// <summary>
        /// Wait for the given asynchronous operations to complete (or timeout)
        /// </summary>
        public void WaitAll(params Task[] tasks)
        {
            if (tasks == null) throw new ArgumentNullException("tasks");
            if (tasks.Length == 0) return;
            if (!Task.WaitAll(tasks, timeoutMilliseconds)) throw new TimeoutException();
        }

        private bool WaitAllIgnoreErrors(Task[] tasks)
        {
            return WaitAllIgnoreErrors(tasks, timeoutMilliseconds);
        }
        private static bool WaitAllIgnoreErrors(Task[] tasks, int timeout)
        {
            if (tasks == null) throw new ArgumentNullException("tasks");
            if (tasks.Length == 0) return true;
            var watch = Stopwatch.StartNew();
            try
            {
                // if none error, great
                if (Task.WaitAll(tasks, timeout)) return true;
            }
            catch
            { }
            // if we get problems, need to give the non-failing ones time to finish
            // to be fair and reasonable
            for (int i = 0; i < tasks.Length; i++)
            {
                var task = tasks[i];
                if (!task.IsCanceled && !task.IsCompleted && !task.IsFaulted)
                {
                    var remaining = timeout - checked((int)watch.ElapsedMilliseconds);
                    if (remaining <= 0) return false;
                    try
                    {
                        task.Wait(remaining);
                    }
                    catch
                    { }
                }
            }
            return false;
        }
        private static async Task<bool> WaitAllIgnoreErrorsAsync(Task[] tasks, int timeoutMilliseconds)
        {
            if (tasks == null) throw new ArgumentNullException("tasks");
            if (tasks.Length == 0) return true;
            var watch = Stopwatch.StartNew();

            try
            {
                // if none error, great

#if NET40
                var allTasks = TaskEx.WhenAll(tasks).ObserveErrors();
                var any = TaskEx.WhenAny(allTasks, TaskEx.Delay(timeoutMilliseconds)).ObserveErrors();
#else
                var allTasks = Task.WhenAll(tasks).ObserveErrors();
                var any = Task.WhenAny(allTasks, Task.Delay(timeoutMilliseconds)).ObserveErrors();
#endif
                return await any.ForAwait() == allTasks;
            }
            catch
            { }

            // if we get problems, need to give the non-failing ones time to finish
            // to be fair and reasonable
            for (int i = 0; i < tasks.Length; i++)
            {
                var task = tasks[i];
                if (!task.IsCanceled && !task.IsCompleted && !task.IsFaulted)
                {
                    var remaining = timeoutMilliseconds - checked((int)watch.ElapsedMilliseconds);
                    if (remaining <= 0) return false;
                    try
                    {
#if NET40
                        var any = TaskEx.WhenAny(task, TaskEx.Delay(remaining)).ObserveErrors();
#else
                        var any = Task.WhenAny(task, Task.Delay(remaining)).ObserveErrors();
#endif
                        await any.ForAwait();
                    }
                    catch
                    { }
                }
            }
            return false;
        }


        /// <summary>
        /// Raised when a hash-slot has been relocated
        /// </summary>
        public event EventHandler<HashSlotMovedEventArgs> HashSlotMoved;

        internal void OnHashSlotMoved(int hashSlot, EndPoint old, EndPoint @new)
        {
            var handler = HashSlotMoved;
            if (handler != null)
            {
                unprocessableCompletionManager.CompleteSyncOrAsync(
                    new HashSlotMovedEventArgs(handler, this, hashSlot, old, @new)
                );
            }
        }

        /// <summary>
        /// Compute the hash-slot of a specified key
        /// </summary>
        public int HashSlot(RedisKey key)
        {
            return serverSelectionStrategy.HashSlot(key);
        }



        internal ServerEndPoint AnyConnected(ServerType serverType, uint startOffset, RedisCommand command, CommandFlags flags)
        {
            var tmp = serverSnapshot;
            int len = tmp.Length;
            ServerEndPoint fallback = null;
            for (int i = 0; i < len; i++)
            {
                var server = tmp[(int)(((uint)i + startOffset) % len)];
                if (server != null && server.ServerType == serverType && server.IsSelectable(command))
                {
                    if (server.IsSlave)
                    {
                        switch (flags)
                        {
                            case CommandFlags.DemandSlave:
                            case CommandFlags.PreferSlave:
                                return server;
                            case CommandFlags.PreferMaster:
                                fallback = server;
                                break;
                        }
                    } else
                    {
                        switch (flags)
                        {
                            case CommandFlags.DemandMaster:
                            case CommandFlags.PreferMaster:
                                return server;
                            case CommandFlags.PreferSlave:
                                fallback = server;
                                break;
                        }
                    }
                }
            }
            return fallback;
        }

        volatile bool isDisposed;
        internal bool IsDisposed { get { return isDisposed; } }
        /// <summary>
        /// Create a new ConnectionMultiplexer instance
        /// </summary>
        public static async Task<ConnectionMultiplexer> ConnectAsync(string configuration, TextWriter log = null)
        {
            IDisposable killMe = null;
            try
            {
                var muxer = CreateMultiplexer(configuration);
                killMe = muxer;
                bool configured = await muxer.ReconfigureAsync(true, false, log, null, "connect").ObserveErrors().ForAwait();
                if (!configured)
                {
                    throw ExceptionFactory.UnableToConnect(muxer.failureMessage);
                }
                killMe = null;
                return muxer;
            } finally
            {
                if (killMe != null) try { killMe.Dispose(); } catch { }
            }
        }

        /// <summary>
        /// Create a new ConnectionMultiplexer instance
        /// </summary>
        public static async Task<ConnectionMultiplexer> ConnectAsync(ConfigurationOptions configuration, TextWriter log = null)
        {
            IDisposable killMe = null;
            try
            {
                var muxer = CreateMultiplexer(configuration);
                killMe = muxer;
                bool configured = await muxer.ReconfigureAsync(true, false, log, null, "connect").ObserveErrors().ForAwait();
                if (!configured)
                {
                    throw ExceptionFactory.UnableToConnect(muxer.failureMessage);
                }
                killMe = null;
                return muxer;
            } finally
            {
                if (killMe != null) try { killMe.Dispose(); } catch { }
            }
        }

        static ConnectionMultiplexer CreateMultiplexer(object configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            ConfigurationOptions config;
            if (configuration is string)
            {
                config = ConfigurationOptions.Parse((string)configuration);
            } else if (configuration is ConfigurationOptions)
            {
                config = ((ConfigurationOptions)configuration).Clone();
            } else
            {
                throw new ArgumentException("configuration");
            }
            if (config.EndPoints.Count == 0) throw new ArgumentException("No endpoints specified", "configuration");
            config.SetDefaultPorts();
            return new ConnectionMultiplexer(config);
        }
        /// <summary>
        /// Create a new ConnectionMultiplexer instance
        /// </summary>
        public static ConnectionMultiplexer Connect(string configuration, TextWriter log = null)
        {
            IDisposable killMe = null;
            try
            {
                var muxer = CreateMultiplexer(configuration);
                killMe = muxer;
                // note that task has timeouts internally, so it might take *just over* the reegular timeout
                var task = muxer.ReconfigureAsync(true, false, log, null, "connect");
                if (!task.Wait(muxer.SyncConnectTimeout(true)))
                {
                    task.ObserveErrors();
                    if (muxer.RawConfig.AbortOnConnectFail)
                    {
                        throw new TimeoutException();
                    }
                }
                if(!task.Result) throw ExceptionFactory.UnableToConnect(muxer.failureMessage);
                killMe = null;
                return muxer;
            }
            finally
            {
                if (killMe != null) try { killMe.Dispose(); } catch { }
            }            
        }
        /// <summary>
        /// Create a new ConnectionMultiplexer instance
        /// </summary>
        public static ConnectionMultiplexer Connect(ConfigurationOptions configuration, TextWriter log = null)
        {
            IDisposable killMe = null;
            try
            {
                var muxer = CreateMultiplexer(configuration);
                killMe = muxer;
                // note that task has timeouts internally, so it might take *just over* the reegular timeout
                var task = muxer.ReconfigureAsync(true, false, log, null, "connect");
                if (!task.Wait(muxer.SyncConnectTimeout(true)))
                {
                    task.ObserveErrors();
                    if (muxer.RawConfig.AbortOnConnectFail)
                    {
                        throw new TimeoutException();
                    }
                }
                if (!task.Result) throw ExceptionFactory.UnableToConnect(muxer.failureMessage);
                killMe = null;
                return muxer;
            }
            finally
            {
                if (killMe != null) try { killMe.Dispose(); } catch { }
            }
        }

        private string failureMessage;
        private readonly Hashtable servers = new Hashtable();
        private volatile ServerEndPoint[] serverSnapshot = NilServers;

        private static readonly ServerEndPoint[] NilServers = new ServerEndPoint[0];

        internal ServerEndPoint GetServerEndPoint(EndPoint endpoint)
        {
            if (endpoint == null) return null;
            var server = (ServerEndPoint)servers[endpoint];
            if (server == null)
            {
                lock (servers)
                {
                    server = (ServerEndPoint)servers[endpoint];
                    if (server == null)
                    {
                        if (isDisposed) throw new ObjectDisposedException(ToString());

                        server = new ServerEndPoint(this, endpoint);
                        servers.Add(endpoint, server);

                        var newSnapshot = serverSnapshot;
                        Array.Resize(ref newSnapshot, newSnapshot.Length + 1);
                        newSnapshot[newSnapshot.Length - 1] = server;
                        serverSnapshot = newSnapshot;
                    }

                }
            }
            return server;
        }

        internal readonly CommandMap CommandMap;
        private ConnectionMultiplexer(ConfigurationOptions configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            IncludeDetailInExceptions = true;
            
            this.configuration = configuration;
            
            var map = this.CommandMap = configuration.CommandMap;
            if (!string.IsNullOrWhiteSpace(configuration.Password)) map.AssertAvailable(RedisCommand.AUTH);

            if(!map.IsAvailable(RedisCommand.ECHO) && !map.IsAvailable(RedisCommand.PING) && !map.IsAvailable(RedisCommand.TIME))
            { // I mean really, give me a CHANCE! I need *something* to check the server is available to me...
                // see also: SendTracer (matching logic)
                map.AssertAvailable(RedisCommand.EXISTS);
            }

            PreserveAsyncOrder = true; // safest default
            this.timeoutMilliseconds = configuration.SyncTimeout;

            OnCreateReaderWriter(configuration);
            unprocessableCompletionManager = new CompletionManager(this, "multiplexer");
            serverSelectionStrategy = new ServerSelectionStrategy(this);

            var configChannel = configuration.ConfigurationChannel;
            if (!string.IsNullOrWhiteSpace(configChannel))
            {
                ConfigurationChangedChannel = Encoding.UTF8.GetBytes(configChannel);
            }
            lastHeartbeatTicks = Environment.TickCount;
        }

        partial void OnCreateReaderWriter(ConfigurationOptions configuration);

        internal const int MillisecondsPerHeartbeat = 1000;

        private static readonly TimerCallback heartbeat = state =>
        {
            ((ConnectionMultiplexer)state).OnHeartbeat();
        };
        private void OnHeartbeat()
        {
            try
            {
                int now = Environment.TickCount;
                Interlocked.Exchange(ref lastHeartbeatTicks, now);
                Interlocked.Exchange(ref lastGlobalHeartbeatTicks, now);
                Trace("heartbeat");

                var tmp = serverSnapshot;
                for (int i = 0; i < tmp.Length; i++)
                    tmp[i].OnHeartbeat();
            } catch(Exception ex)
            {
                OnInternalError(ex);
            }
        }

        private int lastHeartbeatTicks;
        private static int lastGlobalHeartbeatTicks = Environment.TickCount;
        internal long LastHeartbeatSecondsAgo {
            get {
                if (pulse == null) return -1;
                return unchecked(Environment.TickCount - Thread.VolatileRead(ref lastHeartbeatTicks)) / 1000;
            }
        }
        internal static long LastGlobalHeartbeatSecondsAgo
        { get { return unchecked(Environment.TickCount - Thread.VolatileRead(ref lastGlobalHeartbeatTicks)) / 1000; } }

        internal CompletionManager UnprocessableCompletionManager { get { return unprocessableCompletionManager; } }

        /// <summary>
        /// Obtain a pub/sub subscriber connection to the specified server
        /// </summary>
        public ISubscriber GetSubscriber(object asyncState = null)
        {
            if (RawConfig.Proxy == Proxy.Twemproxy) throw new NotSupportedException("The pub/sub API is not available via twemproxy");
            return new RedisSubscriber(this, asyncState);
        }
        /// <summary>
        /// Obtain an interactive connection to a database inside redis
        /// </summary>
        public IDatabase GetDatabase(int db = 0, object asyncState = null)
        {
            if (db < 0) throw new ArgumentOutOfRangeException("db");
            if (db != 0 && RawConfig.Proxy == Proxy.Twemproxy) throw new NotSupportedException("Twemproxy only supports database 0");
            return new RedisDatabase(this, db, asyncState);
        }


        /// <summary>
        /// Obtain a configuration API for an individual server
        /// </summary>
        public IServer GetServer(string host, int port, object asyncState = null)
        {
            return GetServer(Format.ParseEndPoint(host, port), asyncState);
        }
        /// <summary>
        /// Obtain a configuration API for an individual server
        /// </summary>
        public IServer GetServer(string hostAndPort, object asyncState = null)
        {
            return GetServer(Format.TryParseEndPoint(hostAndPort), asyncState);
        }
        /// <summary>
        /// Obtain a configuration API for an individual server
        /// </summary>
        public IServer GetServer(IPAddress host, int port)
        {
            return GetServer(new IPEndPoint(host, port));
        }

        /// <summary>
        /// Obtain a configuration API for an individual server
        /// </summary>
        public IServer GetServer(EndPoint endpoint, object asyncState = null)
        {
            if (endpoint == null) throw new ArgumentNullException("endpoint");
            if (RawConfig.Proxy == Proxy.Twemproxy) throw new NotSupportedException("The server API is not available via twemproxy");
            var server = (ServerEndPoint)servers[endpoint];
            if (server == null) throw new ArgumentException("The specified endpoint is not defined", "endpoint");
            return new RedisServer(this, server, asyncState);
        }


        [Conditional("VERBOSE")]
        internal void Trace(string message, [System.Runtime.CompilerServices.CallerMemberName] string category = null)
        {
            OnTrace(message, category);
        }
        [Conditional("VERBOSE")]
        internal void Trace(bool condition, string message, [System.Runtime.CompilerServices.CallerMemberName] string category = null)
        {
            if (condition) OnTrace(message, category);
        }

        partial void OnTrace(string message, string category);
        static partial void OnTraceWithoutContext(string message, string category);

        [Conditional("VERBOSE")]
        internal static void TraceWithoutContext(string message, [System.Runtime.CompilerServices.CallerMemberName] string category = null)
        {
            OnTraceWithoutContext(message, category);
        }
        [Conditional("VERBOSE")]
        internal static void TraceWithoutContext(bool condition, string message, [System.Runtime.CompilerServices.CallerMemberName] string category = null)
        {
            if(condition) OnTraceWithoutContext(message, category);
        }

        private readonly CompletionManager unprocessableCompletionManager;

        /// <summary>
        /// The number of operations that have been performed on all connections
        /// </summary>
        public long OperationCount {
            get
            {
                long total = 0;
                var snapshot = serverSnapshot;
                for (int i = 0; i < snapshot.Length; i++) total += snapshot[i].OperationCount;
                return total;
            }
        }

        string activeConfigCause;

        internal bool ReconfigureIfNeeded(EndPoint blame, bool fromBroadcast, string cause, bool publishReconfigure = false, CommandFlags flags = CommandFlags.None)
        {
            if (fromBroadcast)
            {
                OnConfigurationChangedBroadcast(blame);
            }
            string activeCause = Interlocked.CompareExchange(ref activeConfigCause, null, null);
            if (activeCause == null)
            {
                bool reconfigureAll = fromBroadcast || publishReconfigure;
                Trace("Configuration change detected; checking nodes", "Configuration");
                ReconfigureAsync(false, reconfigureAll, null, blame, cause, publishReconfigure, flags).ObserveErrors();
                return true;
            } else
            {
                Trace("Configuration change skipped; already in progress via " + activeCause, "Configuration");
                return false;
            }
        }

        /// <summary>
        /// Reconfigure the current connections based on the existing configuration
        /// </summary>
        public Task<bool> ConfigureAsync(TextWriter log = null)
        {
            return ReconfigureAsync(false, true, log, null, "configure").ObserveErrors();
        }
        /// <summary>
        /// Reconfigure the current connections based on the existing configuration
        /// </summary>
        public bool Configure(TextWriter log = null)
        {
            // note we expect ReconfigureAsync to internally allow [n] duration,
            // so to avoid near misses, here we wait 2*[n]
            var task = ReconfigureAsync(false, true, log, null, "configure");
            if (!task.Wait(SyncConnectTimeout(false)))
            {
                task.ObserveErrors();
                if (configuration.AbortOnConnectFail)
                {
                    throw new TimeoutException();
                }
                return false;
            }
            return task.Result;
        }

        internal int SyncConnectTimeout(bool forConnect)
        {
            int retryCount = forConnect ? RawConfig.ConnectRetry : 1;
            if (retryCount <= 0) retryCount = 1;

            int timeout = configuration.ConnectTimeout;            
            if (timeout >= int.MaxValue / retryCount) return int.MaxValue;

            timeout *= retryCount;
            if (timeout >= int.MaxValue - 500) return int.MaxValue;
            return timeout + Math.Min(500, timeout);
        }
        /// <summary>
        /// Provides a text overview of the status of all connections
        /// </summary>
        public string GetStatus()
        {
            using(var sw = new StringWriter())
            {
                GetStatus(sw);
                return sw.ToString();
            }
        }
        /// <summary>
        /// Provides a text overview of the status of all connections
        /// </summary>
        public void GetStatus(TextWriter log)
        {
            if (log == null) return;

            var tmp = serverSnapshot;
            foreach (var server in tmp)
            {
                LogLocked(log, server.Summary());
                LogLocked(log, server.GetCounters().ToString());
                LogLocked(log, server.GetProfile());
            }
            LogLocked(log, "Sync timeouts: {0}; fire and forget: {1}; last heartbeat: {2}s ago",
                Interlocked.Read(ref syncTimeouts), Interlocked.Read(ref fireAndForgets), LastHeartbeatSecondsAgo);
        }
        internal async Task<bool> ReconfigureAsync(bool first, bool reconfigureAll, TextWriter log, EndPoint blame, string cause, bool publishReconfigure = false, CommandFlags publishReconfigureFlags = CommandFlags.None)
        {
            if (isDisposed) throw new ObjectDisposedException(ToString());
            bool showStats = true;
            if (log == null)
            {
                log = TextWriter.Null;
                showStats = false;
            }
            bool ranThisCall = false;
            try
            {   // note that "activeReconfigs" starts at one; we don't need to set it the first time
                ranThisCall = first || Interlocked.CompareExchange(ref activeConfigCause, cause, null) == null;

                if (!ranThisCall)
                {
                    LogLocked(log, "Reconfiguration was already in progress");
                    return false;
                }
                Trace("Starting reconfiguration...");
                Trace(blame != null, "Blaming: " + Format.ToString(blame));

                LogLocked(log, Configuration);
                LogLocked(log, "");


                if (first)
                {
                    if (configuration.ResolveDns && configuration.HasDnsEndPoints())
                    {
                        var dns = configuration.ResolveEndPointsAsync(this, log).ObserveErrors();
#if NET40
                        var any = TaskEx.WhenAny(dns, TaskEx.Delay(timeoutMilliseconds));
#else
                        var any = Task.WhenAny(dns, Task.Delay(timeoutMilliseconds));
#endif
                        if ((await any.ForAwait()) != dns)
                        {
                            throw new TimeoutException("Timeout resolving endpoints");
                        }
                    }
                    int index = 0;
                    lock (this.servers)
                    {
                        serverSnapshot = new ServerEndPoint[configuration.EndPoints.Count];
                        foreach (var endpoint in configuration.EndPoints)
                        {
                            var server = new ServerEndPoint(this, endpoint);
                            serverSnapshot[index++] = server;
                            this.servers.Add(endpoint, server);
                        }
                    }
                    foreach (var server in serverSnapshot)
                    {
                        server.Activate(ConnectionType.Interactive);
                        if (this.CommandMap.IsAvailable(RedisCommand.SUBSCRIBE))
                        {
                            server.Activate(ConnectionType.Subscription);
                        }
                    }
                }
                int attemptsLeft = first ? configuration.ConnectRetry : 1;

                bool healthy = false;
                do
                {
                    if (first)
                    {
                        attemptsLeft--;
                    }
                    int standaloneCount = 0, clusterCount = 0, sentinelCount = 0;
                    var endpoints = configuration.EndPoints;
                    LogLocked(log, "{0} unique nodes specified", endpoints.Count);

                    if (endpoints.Count == 0)
                    {
                        throw new InvalidOperationException("No nodes to consider");
                    }

                    const CommandFlags flags = CommandFlags.NoRedirect | CommandFlags.HighPriority;
                    var available = new Task<bool>[endpoints.Count];
                    var servers = new ServerEndPoint[available.Length];
                    bool useTieBreakers = !string.IsNullOrWhiteSpace(configuration.TieBreaker);
                    var tieBreakers = useTieBreakers ? new Task<string>[endpoints.Count] : null;
                    RedisKey tieBreakerKey = useTieBreakers ? (RedisKey)configuration.TieBreaker : default(RedisKey);
                    for (int i = 0; i < available.Length; i++)
                    {
                        Trace("Testing: " + Format.ToString(endpoints[i]));
                        var server = GetServerEndPoint(endpoints[i]);
                        //server.ReportNextFailure();
                        servers[i] = server;
                        if (reconfigureAll && server.IsConnected)
                        {
                            LogLocked(log, "Refreshing {0}...", Format.ToString(server.EndPoint));
                            // note that these will be processed synchronously *BEFORE* the tracer is processed,
                            // so we know that the configuration will be up to date if we see the tracer
                            server.AutoConfigure(null);
                        }
                        available[i] = server.SendTracer();
                        Message msg;
                        if (useTieBreakers)
                        {
                            LogLocked(log, "Requesting tie-break from {0} > {1}...", Format.ToString(server.EndPoint), configuration.TieBreaker);
                            msg = Message.Create(0, flags, RedisCommand.GET, tieBreakerKey);
                            msg.SetInternalCall();
                            tieBreakers[i] = server.QueueDirectAsync(msg, ResultProcessor.String);
                        }
                    }

                    LogLocked(log, "Allowing endpoints {0} to respond...", TimeSpan.FromMilliseconds(configuration.ConnectTimeout));
                    Trace("Allowing endpoints " + TimeSpan.FromMilliseconds(configuration.ConnectTimeout) + " to respond...");
                    await WaitAllIgnoreErrorsAsync(available, configuration.ConnectTimeout).ForAwait();
                    List<ServerEndPoint> masters = new List<ServerEndPoint>(available.Length);

                    for (int i = 0; i < available.Length; i++)
                    {
                        var task = available[i];
                        Trace(Format.ToString(endpoints[i]) + ": " + task.Status);
                        if (task.IsFaulted)
                        {
                            servers[i].SetUnselectable(UnselectableFlags.DidNotRespond);
                            var aex = task.Exception;
                            foreach (var ex in aex.InnerExceptions)
                            {
                                LogLocked(log, "{0} faulted: {1}", Format.ToString(endpoints[i]), ex.Message);
                                failureMessage = ex.Message;
                            }
                        }
                        else if (task.IsCanceled)
                        {
                            servers[i].SetUnselectable(UnselectableFlags.DidNotRespond);
                            LogLocked(log, "{0} was canceled", Format.ToString(endpoints[i]));
                        }
                        else if (task.IsCompleted)
                        {
                            var server = servers[i];
                            if (task.Result)
                            {
                                servers[i].ClearUnselectable(UnselectableFlags.DidNotRespond);
                                LogLocked(log, "{0} returned with success", Format.ToString(endpoints[i]));

                                // count the server types
                                switch (server.ServerType) 
                                {
                                    case ServerType.Twemproxy:
                                    case ServerType.Standalone:
                                        standaloneCount++;
                                        break;
                                    case ServerType.Sentinel:
                                        sentinelCount++;
                                        break;
                                    case ServerType.Cluster:
                                        clusterCount++;
                                        break;
                                }

                                // set the server UnselectableFlags and update masters list
                                switch (server.ServerType) 
                                {
                                    case ServerType.Twemproxy:
                                    case ServerType.Sentinel:
                                    case ServerType.Standalone:
                                    case ServerType.Cluster:
                                        servers[i].ClearUnselectable(UnselectableFlags.ServerType);
                                        if (server.IsSlave) 
                                        {
                                            servers[i].ClearUnselectable(UnselectableFlags.RedundantMaster);
                                        } 
                                        else 
                                        {
                                            masters.Add(server);
                                        }
                                        break;
                                    default:
                                        servers[i].SetUnselectable(UnselectableFlags.ServerType);
                                        break;
                                }
                            }
                            else
                            {
                                servers[i].SetUnselectable(UnselectableFlags.DidNotRespond);
                                LogLocked(log, "{0} returned, but incorrectly", Format.ToString(endpoints[i]));
                            }
                        }
                        else
                        {
                            servers[i].SetUnselectable(UnselectableFlags.DidNotRespond);
                            LogLocked(log, "{0} did not respond", Format.ToString(endpoints[i]));
                        }
                    }

                    if (clusterCount == 0)
                    {
                        // set the serverSelectionStrategy
                        if (RawConfig.Proxy == Proxy.Twemproxy) 
                        {
                            this.serverSelectionStrategy.ServerType = ServerType.Twemproxy;
                        } 
                        else if (standaloneCount == 0 && sentinelCount > 0) 
                        {
                            this.serverSelectionStrategy.ServerType = ServerType.Sentinel;
                        } 
                        else 
                        {
                            this.serverSelectionStrategy.ServerType = ServerType.Standalone;
                        }
                        var preferred = await NominatePreferredMaster(log, servers, useTieBreakers, tieBreakers, masters).ObserveErrors().ForAwait();
                        foreach (var master in masters)
                        {
                            if (master == preferred)
                            {
                                master.ClearUnselectable(UnselectableFlags.RedundantMaster);
                            }
                            else
                            {
                                master.SetUnselectable(UnselectableFlags.RedundantMaster);
                            }
                        }
                    }
                    else
                    {
                        serverSelectionStrategy.ServerType = ServerType.Cluster;
                        long coveredSlots = serverSelectionStrategy.CountCoveredSlots();
                        LogLocked(log, "Cluster: {0} of {1} slots covered",
                            coveredSlots, serverSelectionStrategy.TotalSlots);

                    }
                    if (!first)
                    {
                        long subscriptionChanges = ValidateSubscriptions();
                        if (subscriptionChanges == 0)
                        {
                            LogLocked(log, "No subscription changes necessary");
                        }
                        else
                        {
                            LogLocked(log, "Subscriptions reconfigured: {0}", subscriptionChanges);
                        }
                    }
                    if (showStats)
                    {
                        GetStatus(log);
                    }

                    string stormLog = GetStormLog();
                    if (!string.IsNullOrWhiteSpace(stormLog))
                    {
                        LogLocked(log, "");
                        LogLocked(log, stormLog);
                    }
                    healthy = standaloneCount != 0 || clusterCount != 0 || sentinelCount != 0;
                    if (first && !healthy && attemptsLeft > 0)
                    {
                        LogLocked(log, "resetting failing connections to retry...");
                        ResetAllNonConnected();
                        LogLocked(log, "retrying; attempts left: " + attemptsLeft + "...");
                    }
                    //WTF("?: " + attempts);
                } while (first && !healthy && attemptsLeft > 0);

                if(first && configuration.AbortOnConnectFail && !healthy)
                {
                    return false;
                }
                if (first)
                {
                    LogLocked(log, "Starting heartbeat...");
                    pulse = new Timer(heartbeat, this, MillisecondsPerHeartbeat, MillisecondsPerHeartbeat);
                }
                if(publishReconfigure)
                {
                    try
                    {
                        LogLocked(log, "Broadcasting reconfigure...");
                        PublishReconfigureImpl(publishReconfigureFlags);
                    }
                    catch
                    { }
                }
                return true;

            } catch (Exception ex)
            {
                Trace(ex.Message);
                throw;
            }
            finally
            {
                Trace("Exiting reconfiguration...");
                OnTraceLog(log);
                if (ranThisCall) Interlocked.Exchange(ref activeConfigCause, null);
                if (!first) OnConfigurationChanged(blame);
                Trace("Reconfiguration exited");
            }
        }

        private void ResetAllNonConnected()
        {
            var snapshot = serverSnapshot;
            foreach(var server in snapshot)
            {
                server.ResetNonConnected();
            }
        }

        partial void OnTraceLog(TextWriter log, [System.Runtime.CompilerServices.CallerMemberName] string caller = null);
        private async Task<ServerEndPoint> NominatePreferredMaster(TextWriter log, ServerEndPoint[] servers, bool useTieBreakers, Task<string>[] tieBreakers, List<ServerEndPoint> masters)
        {
            Dictionary<string, int> uniques = null;
            if (useTieBreakers)
            {   // count the votes
                uniques = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase);
                await WaitAllIgnoreErrorsAsync(tieBreakers, 50).ForAwait();
                for (int i = 0; i < tieBreakers.Length; i++)
                {
                    var ep = servers[i].EndPoint;
                    var status = tieBreakers[i].Status;
                    switch (status)
                    {
                        case TaskStatus.RanToCompletion:
                            string s = tieBreakers[i].Result;
                            if (string.IsNullOrWhiteSpace(s))
                            {
                                LogLocked(log, "{0} had no tiebreaker set", Format.ToString(ep));
                            }
                            else
                            {
                                LogLocked(log, "{0} nominates: {1}", Format.ToString(ep), s);
                                int count;
                                if (!uniques.TryGetValue(s, out count)) count = 0;
                                uniques[s] = count + 1;
                            }
                            break;
                        case TaskStatus.Faulted:
                            LogLocked(log, "{0} failed to nominate ({1})", Format.ToString(ep), status);
                            foreach (var ex in tieBreakers[i].Exception.InnerExceptions)
                            {
                                if (ex.Message.StartsWith("MOVED ") || ex.Message.StartsWith("ASK ")) continue;
                                LogLocked(log, "> {0}", ex.Message);
                            }
                            break;
                        default:
                            LogLocked(log, "{0} failed to nominate ({1})", Format.ToString(ep), status);
                            break;
                    }
                }
            }


            switch (masters.Count)
            {
                case 0:
                    LogLocked(log, "No masters detected");
                    return null;
                case 1:
                    LogLocked(log, "Single master detected: " + Format.ToString(masters[0].EndPoint));
                    return masters[0];
                default:
                    LogLocked(log, "Multiple masters detected...");
                    if (useTieBreakers && uniques != null)
                    {
                        switch (uniques.Count)
                        {
                            case 0:
                                LogLocked(log, "nobody nominated a tie-breaker");
                                break;
                            case 1:
                                string unanimous = uniques.Keys.Single();
                                LogLocked(log, "tie-break is unanimous at {0}", unanimous);
                                var found = SelectServerByElection(servers, unanimous, log);
                                if (found != null)
                                {
                                    LogLocked(log, "Elected: {0}", Format.ToString(found.EndPoint));
                                    return found;
                                }
                                break;
                            default:
                                LogLocked(log, "tie-break is contested:");
                                ServerEndPoint highest = null;
                                bool arbitrary = false;
                                foreach (var pair in uniques.OrderByDescending(x => x.Value))
                                {
                                    LogLocked(log, "{0} has {1} votes", pair.Key, pair.Value);
                                    if (highest == null)
                                    {
                                        highest = SelectServerByElection(servers, pair.Key, log);
                                        if (highest != null)
                                        {
                                            // any more with this vote? if so: arbitrary
                                            arbitrary = uniques.Where(x => x.Value == pair.Value).Skip(1).Any();
                                        }
                                    }
                                }
                                if (highest != null)
                                {
                                    if (arbitrary)
                                    {
                                        LogLocked(log, "Choosing master arbitrarily: {0}", Format.ToString(highest.EndPoint));
                                    }
                                    else
                                    {
                                        LogLocked(log, "Elected: {0}", Format.ToString(highest.EndPoint));
                                    }
                                    return highest;
                                }
                                break;

                        }

                    }
                    break;
            }

            LogLocked(log, "Choosing master arbitrarily: {0}", Format.ToString(masters[0].EndPoint));
            return masters[0];

        }

        private ServerEndPoint SelectServerByElection(ServerEndPoint[] servers, string endpoint, TextWriter log)
        {
            if (servers == null || string.IsNullOrWhiteSpace(endpoint)) return null;
            for (int i = 0; i < servers.Length; i++)
            {
                if (string.Equals(Format.ToString(servers[i].EndPoint), endpoint, StringComparison.OrdinalIgnoreCase))
                    return servers[i];
            }
            LogLocked(log, "...but we couldn't find that");
            return null;
        }

        internal void UpdateClusterRange(ClusterConfiguration configuration)
        {
            if (configuration == null) return;
            foreach (var node in configuration.Nodes)
            {
                if (node.IsSlave || node.Slots.Count == 0) continue;
                foreach (var slot in node.Slots)
                {
                    var server = GetServerEndPoint(node.EndPoint);
                    if (server != null) serverSelectionStrategy.UpdateClusterRange(slot.From, slot.To, server);
                }
            }
        }

        private Timer pulse;

        private readonly ServerSelectionStrategy serverSelectionStrategy;

        internal ServerEndPoint SelectServer(Message message)
        {
            if (message == null) return null;
            return serverSelectionStrategy.Select(message);
        }

        internal ServerEndPoint SelectServer(int db, RedisCommand command, CommandFlags flags, RedisKey key)
        {
            return serverSelectionStrategy.Select(db, command, key, flags);
        }
        private bool TryPushMessageToBridge<T>(Message message, ResultProcessor<T> processor, ResultBox<T> resultBox, ref ServerEndPoint server)
        {
            message.SetSource(processor, resultBox);

            if (server == null)
            {   // infer a server automatically
                server = SelectServer(message);
            }
            else // a server was specified; do we trust their choice, though?
            {

                if (message.IsMasterOnly() && server.IsSlave)
                {
                    throw ExceptionFactory.MasterOnly(IncludeDetailInExceptions, message.Command, message, server);
                }

                switch(server.ServerType)
                {
                    case ServerType.Cluster:
                    case ServerType.Twemproxy: // strictly speaking twemproxy uses a different hashing algo, but the hash-tag behavior is
                                               // the same, so this does a pretty good job of spotting illegal commands before sending them
                        if (message.GetHashSlot(ServerSelectionStrategy) == ServerSelectionStrategy.MultipleSlots)
                        {
                            throw ExceptionFactory.MultiSlot(IncludeDetailInExceptions, message);
                        }
                        break;
                }
                if (!server.IsConnected)
                {
                    // well, that's no use!
                    server = null;
                }
            }
            if (server != null)
            {
                if (message.Db >= 0)
                {
                    int availableDatabases = server.Databases;
                    if (availableDatabases > 0 && message.Db >= availableDatabases) throw ExceptionFactory.DatabaseOutfRange(
                        IncludeDetailInExceptions, message.Db, message, server);
                }

                Trace("Queueing on server: " + message);
                if (server.TryEnqueue(message)) return true;
            }
            Trace("No server or server unavailable - aborting: " + message);
            return false;
        }


        /// <summary>
        /// See Object.ToString()
        /// </summary>
        public override string ToString()
        {
            string s = ClientName;
            if (string.IsNullOrWhiteSpace(s)) s = GetType().Name;
            return s;
        }

        internal readonly byte[] ConfigurationChangedChannel; // this gets accessed for every received event; let's make sure we can process it "raw"
        internal readonly byte[] UniqueId = Guid.NewGuid().ToByteArray(); // unique identifier used when tracing


        /// <summary>
        /// Gets or sets whether asynchronous operations should be invoked in a way that guarantees their original delivery order
        /// </summary>
        public bool PreserveAsyncOrder { get; set; }

        /// <summary>
        /// Indicates whether any servers are connected
        /// </summary>
        public bool IsConnected
        {
            get
            {
                var tmp = serverSnapshot;
                for (int i = 0; i < tmp.Length; i++)
                    if (tmp[i].IsConnected) return true;
                return false;
            }
        }

        internal ConfigurationOptions RawConfig { get { return configuration; } }

        internal ServerSelectionStrategy ServerSelectionStrategy { get { return serverSelectionStrategy; } }


        /// <summary>
        /// Close all connections and release all resources associated with this object
        /// </summary>
        public void Close(bool allowCommandsToComplete = true)
        {
            isDisposed = true;
            using (var tmp = pulse)
            {
                pulse = null;
            }

            if (allowCommandsToComplete)
            {
                var quits = QuitAllServers();
                WaitAllIgnoreErrors(quits);
            }
            DisposeAndClearServers();
            OnCloseReaderWriter();
        }
        partial void OnCloseReaderWriter();

        private void DisposeAndClearServers()
        {
            lock (servers)
            {
                var iter = servers.GetEnumerator();
                while (iter.MoveNext())
                {
                    var server = (ServerEndPoint)iter.Value;
                    server.Dispose();
                }
                servers.Clear();
            }
        }

        private Task[] QuitAllServers()
        {
            Task[] quits = new Task[servers.Count];
            lock (servers)
            {
                var iter = servers.GetEnumerator();
                int index = 0;
                while (iter.MoveNext())
                {
                    var server = (ServerEndPoint)iter.Value;
                    quits[index++] = server.Close();
                }
            }
            return quits;
        }

        /// <summary>
        /// Close all connections and release all resources associated with this object
        /// </summary>
        public async Task CloseAsync(bool allowCommandsToComplete = true)
        {
            isDisposed = true;
            using (var tmp = pulse)
            {
                pulse = null;
            }

            if (allowCommandsToComplete)
            {
                var quits = QuitAllServers();
                await WaitAllIgnoreErrorsAsync(quits, configuration.SyncTimeout).ForAwait();
            }

            DisposeAndClearServers();
        }

        /// <summary>
        /// Release all resources associated with this object
        /// </summary>
        public void Dispose()
        {
            Close(!isDisposed);
        }


        internal Task<T> ExecuteAsyncImpl<T>(Message message, ResultProcessor<T> processor, object state, ServerEndPoint server)
        {
            if (isDisposed) throw new ObjectDisposedException(ToString());

            if (message == null)
            {
                return CompletedTask<T>.Default(state);
            }
            if (message.IsFireAndForget)
            {
                TryPushMessageToBridge(message, processor, null, ref server);
                return CompletedTask<T>.Default(null); // F+F explicitly does not get async-state
            }
            else
            {
                var tcs = TaskSource.CreateDenyExecSync<T>(state);
                var source = ResultBox<T>.Get(tcs);
                if (!TryPushMessageToBridge(message, processor, source, ref server))
                {
                    ThrowFailed(tcs, ExceptionFactory.NoConnectionAvailable(IncludeDetailInExceptions, message.Command, message, server));
                }
                return tcs.Task;
            }
        }

        internal static void ThrowFailed<T>(TaskCompletionSource<T> source, Exception unthrownException)
        {
            try
            {
                throw unthrownException;
            } catch (Exception ex)
            {
                source.TrySetException(ex);
                GC.KeepAlive(source.Task.Exception);
                GC.SuppressFinalize(source.Task);
            }
        }
        internal T ExecuteSyncImpl<T>(Message message, ResultProcessor<T> processor, ServerEndPoint server)
        {
            if (isDisposed) throw new ObjectDisposedException(ToString());

            if (message == null) // fire-and forget could involve a no-op, represented by null - for example Increment by 0
            {
                return default(T);
            }

            if (message.IsFireAndForget)
            {
                TryPushMessageToBridge(message, processor, null, ref server);
                Interlocked.Increment(ref fireAndForgets);
                return default(T);
            }
            else
            {
                var source = ResultBox<T>.Get(null);

                lock (source)
                {
                    if (!TryPushMessageToBridge(message, processor, source, ref server))
                    {
                        throw ExceptionFactory.NoConnectionAvailable(IncludeDetailInExceptions, message.Command, message, server);
                    }

                    if (Monitor.Wait(source, timeoutMilliseconds))
                    {
                        Trace("Timeley response to " + message.ToString());
                    }
                    else
                    {
                        Trace("Timeout performing " + message.ToString());
                        Interlocked.Increment(ref syncTimeouts);
                        string errMessage;
                        if (server == null || !IncludeDetailInExceptions)
                        {
                            errMessage = "Timeout performing " + message.Command.ToString();
                        }
                        else
                        {
                            int inst, qu, qs, qc, wr, wq, @in, ar;
#if !__MonoCS__
                            var mgrState = socketManager.State;
#endif
                            int queue = server.GetOutstandingCount(message.Command, out inst, out qu, out qs, out qc, out wr, out wq, out @in, out ar);
                            var sb = new StringBuilder("Timeout performing ").Append(message.CommandAndKey)
                                .Append(", inst: ").Append(inst)
#if !__MonoCS__
                                .Append(", mgr: ").Append(mgrState)
#endif
                                .Append(", queue: ").Append(queue).Append(", qu=").Append(qu)
                                .Append(", qs=").Append(qs).Append(", qc=").Append(qc)
                                .Append(", wr=").Append(wr).Append("/").Append(wq)
                                .Append(", in=").Append(@in).Append("/").Append(ar);

                            errMessage = sb.ToString();
                            if (stormLogThreshold >= 0 && queue >= stormLogThreshold && Interlocked.CompareExchange(ref haveStormLog, 1, 0) == 0)
                            {
                                var log = server.GetStormLog(message.Command);
                                if (string.IsNullOrWhiteSpace(log)) Interlocked.Exchange(ref haveStormLog, 0);
                                else Interlocked.Exchange(ref stormLogSnapshot, log);
                            }
                        }
                        throw ExceptionFactory.Timeout(IncludeDetailInExceptions, errMessage, message, server);
                        // very important not to return "source" to the pool here
                    }
                }
                // snapshot these so that we can recycle the box
                Exception ex;
                T val;
                ResultBox<T>.UnwrapAndRecycle(source, out val, out ex); // now that we aren't locking it...
                if (ex != null) throw ex;
                Trace(message + " received " + val);
                return val;
            }
        }

        /// <summary>
        /// Should exceptions include identifiable details? (key names, additional .Data annotations)
        /// </summary>
        public bool IncludeDetailInExceptions { get; set; }

        int haveStormLog = 0, stormLogThreshold = 15;
        string stormLogSnapshot;
        /// <summary>
        /// Limit at which to start recording unusual busy patterns (only one log will be retained at a time;
        /// set to a negative value to disable this feature)
        /// </summary>
        public int StormLogThreshold { get { return stormLogThreshold; } set { stormLogThreshold = value; } }
        /// <summary>
        /// Obtains the log of unusual busy patterns
        /// </summary>
        public string GetStormLog()
        {
            var result = Interlocked.CompareExchange(ref stormLogSnapshot, null, null);
            return result;
        }
        /// <summary>
        /// Resets the log of unusual busy patterns
        /// </summary>
        public void ResetStormLog()
        {
            Interlocked.Exchange(ref stormLogSnapshot, null);
            Interlocked.Exchange(ref haveStormLog, 0);
        }
        private long syncTimeouts, fireAndForgets;

        /// <summary>
        /// Request all compatible clients to reconfigure or reconnect
        /// </summary>
        /// <returns>The number of instances known to have received the message (however, the actual number can be higher; returns -1 if the operation is pending)</returns>
        public long PublishReconfigure(CommandFlags flags = CommandFlags.None)
        {
            byte[] channel = ConfigurationChangedChannel;
            if (channel == null) return 0;
            if (ReconfigureIfNeeded(null, false, "PublishReconfigure", true, flags))
            {
                return -1;
            }
            else
            {
                return PublishReconfigureImpl(flags);
            }
        }
        private long PublishReconfigureImpl(CommandFlags flags)
        {
            byte[] channel = ConfigurationChangedChannel;
            if (channel == null) return 0;
            return GetSubscriber().Publish(channel, RedisLiterals.Wildcard, flags);
        }

        /// <summary>
        /// Request all compatible clients to reconfigure or reconnect
        /// </summary>
        /// <returns>The number of instances known to have received the message (however, the actual number can be higher)</returns>
        public Task<long> PublishReconfigureAsync(CommandFlags flags = CommandFlags.None)
        {
            byte[] channel = ConfigurationChangedChannel;
            if (channel == null) return CompletedTask<long>.Default(null);

            return GetSubscriber().PublishAsync(channel, RedisLiterals.Wildcard, flags);
        }        
    }   

}
