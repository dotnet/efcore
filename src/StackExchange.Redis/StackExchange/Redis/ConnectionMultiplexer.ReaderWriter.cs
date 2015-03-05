using System;
using System.Collections.Generic;
using System.Threading;

namespace StackExchange.Redis
{
    partial class ConnectionMultiplexer
    {
        internal SocketManager SocketManager {  get {  return socketManager; } }

        private SocketManager socketManager;
        private bool ownsSocketManager;

        partial void OnCreateReaderWriter(ConfigurationOptions configuration)
        {
            this.ownsSocketManager = configuration.SocketManager == null;
            this.socketManager = configuration.SocketManager ?? new SocketManager(ClientName);
        }

        partial void OnCloseReaderWriter()
        {
            if (ownsSocketManager && socketManager != null) socketManager.Dispose();
            socketManager = null;
        }

        internal void RequestWrite(PhysicalBridge bridge, bool forced)
        {
            if (bridge == null) return;
            var tmp = SocketManager;
            if (tmp != null)
            {
                Trace("Requesting write: " + bridge.Name);
                tmp.RequestWrite(bridge, forced);
            }
        }
        partial void OnWriterCreated();

        
    }
}
