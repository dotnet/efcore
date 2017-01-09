using System;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests
{
    public class MongoDbConstants
    {
        public const int MongodPort = 27081;
        public static readonly string MongodExe = Environment.ExpandEnvironmentVariables(name: @"%PROGRAMFILES%\MongoDB\Server\3.2\bin\mongod.exe");
        public static readonly string MongoExe = Environment.ExpandEnvironmentVariables(name: @"%PROGRAMFILES%\MongoDB\Server\3.2\bin\mongo.exe");
        public static readonly string DataFolder = $@".data\Port-{MongodPort}";
        public static readonly string MongoUrl = $"mongodb://localhost:{MongodPort}";
    }
}