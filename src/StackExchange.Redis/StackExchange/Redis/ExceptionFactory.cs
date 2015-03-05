using System;

namespace StackExchange.Redis
{
    internal static class ExceptionFactory
    {
        const string DataCommandKey = "redis-command",
            DataServerKey = "redis-server";

        internal static Exception AdminModeNotEnabled(bool includeDetail, RedisCommand command, Message message, ServerEndPoint server)
        {
            string s = GetLabel(includeDetail, command, message);
            var ex = new RedisCommandException("This operation is not available unless admin mode is enabled: " + s);
            if (includeDetail) AddDetail(ex, message, server, s);
            return ex;
        }
        internal static Exception CommandDisabled(bool includeDetail, RedisCommand command, Message message, ServerEndPoint server)
        {
            string s = GetLabel(includeDetail, command, message);
            var ex = new RedisCommandException("This operation has been disabled in the command-map and cannot be used: " + s);
            if (includeDetail) AddDetail(ex, message, server, s);
            return ex;
        }

        internal static Exception ConnectionFailure(bool includeDetail, ConnectionFailureType failureType, string message, ServerEndPoint server)
        {
            var ex = new RedisConnectionException(failureType, message);
            if (includeDetail) AddDetail(ex, null, server, null);
            return ex;
        }

        internal static Exception DatabaseNotRequired(bool includeDetail, RedisCommand command)
        {
            string s = command.ToString();
            var ex = new RedisCommandException("A target database is not required for " + s);
            if (includeDetail) AddDetail(ex, null, null, s);
            return ex;
        }

        internal static Exception DatabaseOutfRange(bool includeDetail, int targetDatabase, Message message, ServerEndPoint server)
        {
            var ex = new RedisCommandException("The database does not exist on the server: " + targetDatabase);
            if (includeDetail) AddDetail(ex, message, server, null);
            return ex;
        }

        internal static Exception DatabaseRequired(bool includeDetail, RedisCommand command)
        {
            string s = command.ToString();
            var ex = new RedisCommandException("A target database is required for " + s);
            if (includeDetail) AddDetail(ex, null, null, s);
            return ex;
        }

        internal static Exception MasterOnly(bool includeDetail, RedisCommand command, Message message, ServerEndPoint server)
        {
            string s = GetLabel(includeDetail, command, message);
            var ex = new RedisCommandException("Command cannot be issued to a slave: " + s);
            if (includeDetail) AddDetail(ex, message, server, s);
            return ex;
        }

        internal static Exception MultiSlot(bool includeDetail, Message message)
        {
            var ex = new RedisCommandException("Multi-key operations must involve a single slot; keys can use 'hash tags' to help this, i.e. '{/users/12345}/account' and '{/users/12345}/contacts' will always be in the same slot");
            if (includeDetail) AddDetail(ex, message, null, null);
            return ex;
        }

        internal static Exception NoConnectionAvailable(bool includeDetail, RedisCommand command, Message message, ServerEndPoint server)
        {
            string s = GetLabel(includeDetail, command, message);
            var ex = new RedisConnectionException(ConnectionFailureType.UnableToResolvePhysicalConnection, "No connection is available to service this operation: " + s);
            if (includeDetail) AddDetail(ex, message, server, s);
            return ex;
        }

        internal static Exception NotSupported(bool includeDetail, RedisCommand command)
        {
            string s = GetLabel(includeDetail, command, null);
            var ex = new RedisCommandException("Command is not available on your server: " + s);
            if (includeDetail) AddDetail(ex, null, null, s);
            return ex;
        }
        internal static Exception NoCursor(RedisCommand command)
        {
            string s = GetLabel(false, command, null);
            var ex = new RedisCommandException("Command cannot be used with a cursor: " + s);
            return ex;
        }

        internal static Exception Timeout(bool includeDetail, string errorMessage, Message message, ServerEndPoint server)
        {
            var ex = new TimeoutException(errorMessage);
            if (includeDetail) AddDetail(ex, message, server, null);
            return ex;
        }

        private static void AddDetail(Exception exception, Message message, ServerEndPoint server, string label)
        {
            if (exception != null)
            {
                if (message != null) exception.Data.Add(DataCommandKey, message.CommandAndKey);
                else if (label != null) exception.Data.Add(DataCommandKey, label);

                if (server != null) exception.Data.Add(DataServerKey, Format.ToString(server.EndPoint));
            }
        }

        static string GetLabel(bool includeDetail, RedisCommand command, Message message)
        {
            return message == null ? command.ToString() : (includeDetail ? message.CommandAndKey : message.Command.ToString());
        }

        internal static Exception UnableToConnect(string failureMessage=null)
        {
            return new RedisConnectionException(ConnectionFailureType.UnableToConnect,
                "It was not possible to connect to the redis server(s); to create a disconnected multiplexer, disable AbortOnConnectFail. " + failureMessage);
        }
    }
}
