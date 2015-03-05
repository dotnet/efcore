using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;

namespace StackExchange.Redis
{
    internal enum SocketMode
    {
        Abort,
        Poll,
        Async
    }
    /// <summary>
    /// Allows callbacks from SocketManager as work is discovered
    /// </summary>
    internal partial interface ISocketCallback
    {
        /// <summary>
        /// Indicates that a socket has connected
        /// </summary>
        SocketMode Connected(Stream stream);
        /// <summary>
        /// Indicates that the socket has signalled an error condition
        /// </summary>
        void Error();

        void OnHeartbeat();

        /// <summary>
        /// Indicates that data is available on the socket, and that the consumer should read synchronously from the socket while there is data
        /// </summary>
        void Read();
        /// <summary>
        /// Indicates that we cannot know whether data is available, and that the consume should commence reading asynchronously
        /// </summary>
        void StartReading();
    }

    internal struct SocketToken
    {
        internal readonly Socket Socket;
        public SocketToken(Socket socket)
        {
            this.Socket = socket;
        }
        public int Available { get { return Socket == null ? 0 : Socket.Available; } }

        public bool HasValue { get { return Socket != null; } }
    }

    /// <summary>
    /// A SocketManager monitors multiple sockets for availability of data; this is done using
    /// the Socket.Select API and a dedicated reader-thread, which allows for fast responses
    /// even when the system is under ambient load. 
    /// </summary>
    public sealed partial class SocketManager : IDisposable
    {
        private static readonly ParameterizedThreadStart writeAllQueues = context =>
        {
            try { ((SocketManager)context).WriteAllQueues(); } catch { }
        };

        private static readonly WaitCallback writeOneQueue = context =>
        {

            try { ((SocketManager)context).WriteOneQueue(); } catch { }
        };

        private readonly string name;

        private readonly Queue<PhysicalBridge> writeQueue = new Queue<PhysicalBridge>();

        bool isDisposed;

        /// <summary>
        /// Creates a new (optionally named) SocketManager instance
        /// </summary>
        public SocketManager(string name = null)
        {
            if (string.IsNullOrWhiteSpace(name)) name = GetType().Name;
            this.name = name;

            // we need a dedicated writer, because when under heavy ambient load
            // (a busy asp.net site, for example), workers are not reliable enough
            Thread dedicatedWriter = new Thread(writeAllQueues, 32 * 1024); // don't need a huge stack;
            dedicatedWriter.Priority = ThreadPriority.AboveNormal; // time critical
            dedicatedWriter.Name = name + ":Write";
            dedicatedWriter.IsBackground = true; // should not keep process alive
            dedicatedWriter.Start(this); // will self-exit when disposed
        }

        private enum CallbackOperation
        {
            Read,
            Error
        }

        /// <summary>
        /// Gets the name of this SocketManager instance
        /// </summary>
        public string Name { get { return name; } }
        /// <summary>
        /// Releases all resources associated with this instance
        /// </summary>
        public void Dispose()
        {
            lock (writeQueue)
            {
                // make sure writer threads know to exit
                isDisposed = true;
                Monitor.PulseAll(writeQueue);
            }
            OnDispose();
        }
        internal SocketToken BeginConnect(EndPoint endpoint, ISocketCallback callback)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SetFastLoopbackOption(socket);
            socket.NoDelay = true;
            try
            {
                CompletionType connectCompletionType = CompletionType.Any;
                this.ShouldForceConnectCompletionType(ref connectCompletionType);

                CompletionTypeHelper.RunWithCompletionType(
                    (cb) => socket.BeginConnect(endpoint, cb, Tuple.Create(socket, callback)),
                    (ar) => EndConnectImpl(ar),
                    CompletionType.Sync);
            } 
            catch (NotImplementedException ex)
            {
                if (!(endpoint is IPEndPoint))
                {
                    throw new InvalidOperationException("BeginConnect failed with NotImplementedException; consider using IP endpoints, or enable ResolveDns in the configuration", ex);
                }
                throw;
            }
            var token = new SocketToken(socket);
            return token;
        }
        internal void SetFastLoopbackOption(Socket socket)
        {
            // SIO_LOOPBACK_FAST_PATH (http://msdn.microsoft.com/en-us/library/windows/desktop/jj841212%28v=vs.85%29.aspx)
            // Speeds up localhost operations significantly. OK to apply to a socket that will not be hooked up to localhost, 
            // or will be subject to WFP filtering.
            const int SIO_LOOPBACK_FAST_PATH = -1744830448;

            // windows only
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                // Win8/Server2012+ only
                var osVersion = Environment.OSVersion.Version;
                if (osVersion.Major > 6 || osVersion.Major == 6 && osVersion.Minor >= 2)
                {
                    byte[] optionInValue = BitConverter.GetBytes(1);
                    socket.IOControl(SIO_LOOPBACK_FAST_PATH, optionInValue, null);
                }
            }
        }

        internal void RequestWrite(PhysicalBridge bridge, bool forced)
        {
            if (Interlocked.CompareExchange(ref bridge.inWriteQueue, 1, 0) == 0 || forced)
            {
                lock (writeQueue)
                {
                    writeQueue.Enqueue(bridge);
                    if (writeQueue.Count == 1)
                    {
                        Monitor.PulseAll(writeQueue);
                    }
                    else if (writeQueue.Count >= 2)
                    { // struggling are we? let's have some help dealing with the backlog
                        ThreadPool.QueueUserWorkItem(writeOneQueue, this);
                    }
                }
            }
        }

        internal void Shutdown(SocketToken token)
        {
            Shutdown(token.Socket);
        }

        private void EndConnectImpl(IAsyncResult ar)
        {
            Tuple<Socket, ISocketCallback> tuple = null;
            try
            {
                tuple = (Tuple<Socket, ISocketCallback>)ar.AsyncState;
                bool ignoreConnect = false;
                ShouldIgnoreConnect(tuple.Item2, ref ignoreConnect);
                if (ignoreConnect) return;
                var socket = tuple.Item1;
                var callback = tuple.Item2;
                socket.EndConnect(ar);
                var netStream = new NetworkStream(socket, false);
                var socketMode = callback == null ? SocketMode.Abort : callback.Connected(netStream);
                switch (socketMode)
                {
                    case SocketMode.Poll:
                        ConnectionMultiplexer.TraceWithoutContext("Starting poll");
                        OnAddRead(socket, callback);
                        break;
                    case SocketMode.Async:
                        ConnectionMultiplexer.TraceWithoutContext("Starting read");
                        try
                        { callback.StartReading(); }
                        catch (Exception ex)
                        {
                            ConnectionMultiplexer.TraceWithoutContext(ex.Message);
                            Shutdown(socket);
                        }
                        break;
                    default:
                        ConnectionMultiplexer.TraceWithoutContext("Aborting socket");
                        Shutdown(socket);
                        break;
                }
            }
            catch(ObjectDisposedException)
            {
                ConnectionMultiplexer.TraceWithoutContext("(socket shutdown)");
                if (tuple != null)
                {
                    try
                    { tuple.Item2.Error(); }
                    catch (Exception inner)
                    {
                        ConnectionMultiplexer.TraceWithoutContext(inner.Message);
                    }
                }
            }
            catch(Exception outer)
            {
                ConnectionMultiplexer.TraceWithoutContext(outer.Message);
                if (tuple != null)
                {
                    try
                    { tuple.Item2.Error(); }
                    catch (Exception inner)
                    {
                        ConnectionMultiplexer.TraceWithoutContext(inner.Message);
                    }
                }
            }
        }

        partial void OnDispose();
        partial void OnShutdown(Socket socket);

        partial void ShouldIgnoreConnect(ISocketCallback callback, ref bool ignore);
        
        partial void ShouldForceConnectCompletionType(ref CompletionType completionType);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        private void Shutdown(Socket socket)
        {
            if (socket != null)
            {
                OnShutdown(socket);
                try { socket.Shutdown(SocketShutdown.Both); } catch { }
                try { socket.Close(); } catch { }
                try { socket.Dispose(); } catch { }
            }
        }

        private void WriteAllQueues()
        {
            while (true)
            {
                PhysicalBridge bridge;
                lock (writeQueue)
                {
                    if (writeQueue.Count == 0)
                    {
                        if (isDisposed) break; // <========= exit point
                        Monitor.Wait(writeQueue);
                        if (isDisposed) break; // (woken by Dispose)
                        if (writeQueue.Count == 0) continue; // still nothing...
                    }
                    bridge = writeQueue.Dequeue();
                }

                switch (bridge.WriteQueue(200))
                {
                    case WriteResult.MoreWork:
                    case WriteResult.QueueEmptyAfterWrite:
                        // back of the line!
                        lock (writeQueue)
                        {
                            writeQueue.Enqueue(bridge);
                        }
                        break;
                    case WriteResult.CompetingWriter:
                        break;
                    case WriteResult.NoConnection:
                        Interlocked.Exchange(ref bridge.inWriteQueue, 0);
                        break;
                    case WriteResult.NothingToDo:
                        if (!bridge.ConfirmRemoveFromWriteQueue())
                        { // more snuck in; back of the line!
                            lock (writeQueue)
                            {
                                writeQueue.Enqueue(bridge);
                            }
                        }
                        break;
                }
            }
        }

        private void WriteOneQueue()
        {
            PhysicalBridge bridge;
            lock (writeQueue)
            {
                bridge = writeQueue.Count == 0 ? null : writeQueue.Dequeue();
            }
            if (bridge == null) return;
            bool keepGoing;
            do
            {
                switch (bridge.WriteQueue(-1))
                {
                    case WriteResult.MoreWork:
                    case WriteResult.QueueEmptyAfterWrite:
                        keepGoing = true;
                        break;
                    case WriteResult.NothingToDo:
                        keepGoing = !bridge.ConfirmRemoveFromWriteQueue();
                        break;
                    case WriteResult.CompetingWriter:
                        keepGoing = false;
                        break;
                    case WriteResult.NoConnection:
                        Interlocked.Exchange(ref bridge.inWriteQueue, 0);
                        keepGoing = false;
                        break;
                    default:
                        keepGoing = false;
                        break;
                }
            } while (keepGoing);
        }
    }
}
