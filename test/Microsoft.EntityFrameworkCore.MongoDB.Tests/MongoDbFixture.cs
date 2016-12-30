using System;
using System.Diagnostics;
using System.IO;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.MongoDB.Tests.TestDomain;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.MongoDB.Tests
{
    public class MongoDbFixture : IDisposable
    {
        private const int MongodPort = 27081;
        private static readonly string _mongodExe = Environment.ExpandEnvironmentVariables(name: @"%PROGRAMFILES%\MongoDB\Server\3.2\bin\mongod.exe");
        private static readonly string _mongoExe = Environment.ExpandEnvironmentVariables(name: @"%PROGRAMFILES%\MongoDB\Server\3.2\bin\mongo.exe");
        private static readonly string _dataFolder = $@".data\Port-{MongodPort}";
        private static readonly string _mongoUrl = $"mongodb://localhost:{MongodPort}";

        private Process _mongodProcess;

        public MongoDbFixture()
        {
            if (!File.Exists(_mongodExe))
            {
                throw new Exception(message: "MongoDB is not installed on the local system. Please install version 3.2 to run these tests.");
            }
            Directory.CreateDirectory(_dataFolder);
            _mongodProcess = Process.Start(
                new ProcessStartInfo
                {
                    FileName = Environment.ExpandEnvironmentVariables(name: _mongodExe),
                    Arguments = $@"-vvvvv --port {MongodPort} --logpath "".data\{MongodPort}.log"" --dbpath ""{_dataFolder}""",
                    CreateNoWindow = true,
                    UseShellExecute = false
                });
        }

        public TestMongoDbContext TestMongoDbContext => new ServiceCollection()
                .AddDbContext<TestMongoDbContext>(options => options.UseMongoDb(connectionString: _mongoUrl))
                .BuildServiceProvider()
                .GetService<TestMongoDbContext>();

        public void Dispose()
        {
            if (_mongodProcess != null && !_mongodProcess.HasExited)
            {
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = _mongoExe,
                        Arguments = $@"""{_mongoUrl}/admin"" --eval ""db.shutdownServer();""",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                _mongodProcess.WaitForExit(milliseconds: 5000);
                _mongodProcess.Dispose();
                _mongodProcess = null;
            }
        }
    }
}
