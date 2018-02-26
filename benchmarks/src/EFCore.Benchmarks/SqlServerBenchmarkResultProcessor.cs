// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.SqlClient;

namespace Microsoft.EntityFrameworkCore.Benchmarks
{
    public class SqlServerBenchmarkResultProcessor
    {
        private const string _insertCommand =
            @"INSERT INTO [dbo].[BenchmarkDotNetRuns]
           ([MachineName]
            ,[Framework]
            ,[Architecture]
            ,[TestClassFullName]
            ,[TestClass]
            ,[TestMethodName]
            ,[Variation]
            ,[EfVersion]
            ,[CustomData]
            ,[ReportingTime]
            ,[WarmupIterations]
            ,[MainIterations]
            ,[TimeElapsedMean]
            ,[TimeElapsedPercentile90]
            ,[TimeElapsedPercentile95]
            ,[TimeElapsedStandardError]
            ,[TimeElapsedStandardDeviation]
            ,[MemoryAllocated])
     VALUES
           (@MachineName
           ,@Framework
           ,@Architecture
           ,@TestClassFullName
           ,@TestClass
           ,@TestMethodName
           ,@Variation
           ,@EfVersion
           ,@CustomData
           ,@ReportingTime
           ,@WarmupIterations
           ,@MainIterations
           ,@TimeElapsedMean
           ,@TimeElapsedPercentile90
           ,@TimeElapsedPercentile95
           ,@TimeElapsedStandardError
           ,@TimeElapsedStandardDeviation
           ,@MemoryAllocated)";

        private const string _tableCreationCommand =
            @"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='benchmarkdotnetruns' and xtype='U')
    CREATE TABLE [dbo].[BenchmarkDotNetRuns](
        [Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [MachineName] [nvarchar](max) NULL,
        [Framework] [nvarchar](max) NULL,
        [Architecture] [nvarchar](max) NULL,
        [TestClassFullName] [nvarchar](max) NULL,
        [TestClass] [nvarchar](max) NULL,
        [TestMethodName] [nvarchar](max) NULL,
        [Variation] [nvarchar](max) NULL,
        [EfVersion] [nvarchar](max) NULL,
        [CustomData] [nvarchar](max) NULL,
        [ReportingTime] [datetime2](7) NOT NULL,
        [WarmupIterations] [int] NOT NULL,
        [MainIterations] [int] NOT NULL,
        [TimeElapsedMean] [float] NOT NULL,
        [TimeElapsedPercentile90] [float] NOT NULL,
        [TimeElapsedPercentile95] [float] NOT NULL,
        [TimeElapsedStandardError] [float] NOT NULL,
        [TimeElapsedStandardDeviation] [float] NOT NULL,
        [MemoryAllocated] [float] NOT NULL)
ELSE IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'Architecture')
    ALTER TABLE [dbo].[BenchmarkDotNetRuns] ADD [Architecture] nvarchar(max) NULL";

        public virtual void SaveSummary(string connectionString, BenchmarkResult result)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                EnsureRunsTableCreated(conn);
                WriteResultRecord(result, conn);
            }
        }

        private static void EnsureRunsTableCreated(SqlConnection conn)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = _tableCreationCommand;
            cmd.ExecuteNonQuery();
        }

        private static void WriteResultRecord(BenchmarkResult summary, SqlConnection conn)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = _insertCommand;

            cmd.Parameters.AddWithValue("@MachineName", summary.MachineName);
            cmd.Parameters.AddWithValue("@Framework", summary.Framework);
            cmd.Parameters.AddWithValue("@Architecture", summary.Architecture);

            cmd.Parameters.AddWithValue("@TestClassFullName", summary.TestClassFullName);
            cmd.Parameters.AddWithValue("@TestClass", summary.TestClass);
            cmd.Parameters.AddWithValue("@TestMethodName", summary.TestMethodName);
            cmd.Parameters.AddWithValue("@Variation", summary.Variation);

            cmd.Parameters.AddWithValue("@EfVersion", summary.EfVersion);
            cmd.Parameters.Add("@CustomData", SqlDbType.NVarChar).Value = (object)summary.CustomData ?? DBNull.Value;

            cmd.Parameters.AddWithValue("@ReportingTime", summary.ReportingTime);
            cmd.Parameters.AddWithValue("@WarmupIterations", summary.WarmupIterations);
            cmd.Parameters.AddWithValue("@MainIterations", summary.MainIterations);

            cmd.Parameters.AddWithValue("@TimeElapsedMean", summary.TimeElapsedMean);
            cmd.Parameters.AddWithValue("@TimeElapsedPercentile90", summary.TimeElapsedPercentile90);
            cmd.Parameters.AddWithValue("@TimeElapsedPercentile95", summary.TimeElapsedPercentile95);
            cmd.Parameters.AddWithValue("@TimeElapsedStandardError", summary.TimeElapsedStandardError);
            cmd.Parameters.AddWithValue("@TimeElapsedStandardDeviation", summary.TimeElapsedStandardDeviation);
            cmd.Parameters.AddWithValue("@MemoryAllocated", summary.MemoryAllocated);

            cmd.ExecuteNonQuery();
        }
    }
}
