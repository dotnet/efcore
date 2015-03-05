#if __MonoCS__

namespace StackExchange.Redis
{
    partial class SocketManager
    {
        internal const SocketMode DefaultSocketMode = SocketMode.Async;

        private void OnAddRead(System.Net.Sockets.Socket socket, ISocketCallback callback)
        {
            throw new System.NotSupportedException();
        }
    }
}

#endif