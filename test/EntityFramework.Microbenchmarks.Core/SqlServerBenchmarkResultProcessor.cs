// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data.SqlClient;
using System.Data;

namespace EntityFramework.Microbenchmarks.Core
{
    public class SqlServerBenchmarkResultProcessor
    {
        private readonly string _connectionString;
        private static readonly string _insertCommand =
            @"INSERT INTO [dbo].[Runs]
           ([TestClassFullName]
            ,[TestClass]
            ,[TestMethod]
            ,[Variation]
            ,[MachineName]
            ,[ProductReportingVersion]
            ,[Framework]
            ,[CustomData]
            ,[RunStarted]
            ,[WarmupIterations]
            ,[Iterations]
            ,[TimeElapsedAverage]
            ,[TimeElapsedPercentile99]
            ,[TimeElapsedPercentile95]
            ,[TimeElapsedPercentile90]
            ,[TimeElapsedStandardDeviation]
            ,[MemoryDeltaAverage]
            ,[MemoryDeltaPercentile99]
            ,[MemoryDeltaPercentile95]
            ,[MemoryDeltaPercentile90]
            ,[MemoryDeltaStandardDeviation])
     VALUES
           (@TestClassFullName
           ,@TestClass
           ,@TestMethod
           ,@Variation
           ,@MachineName
           ,@ProductReportingVersion
           ,@Framework
           ,@CustomData
           ,@RunStarted
           ,@WarmupIterations
           ,@Iterations
           ,@TimeElapsedAverage
           ,@TimeElapsedPercentile99
           ,@TimeElapsedPercentile95
           ,@TimeElapsedPercentile90
           ,@TimeElapsedStandardDeviation
           ,@MemoryDeltaAverage
           ,@MemoryDeltaPercentile99
           ,@MemoryDeltaPercentile95
           ,@MemoryDeltaPercentile90
           ,@MemoryDeltaStandardDeviation)";

        private static readonly string _tableCreationCommand =
@"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='runs' and xtype='U')
	CREATE TABLE [dbo].[Runs](
		[Id] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
		[TestClassFullName] [nvarchar](max) NULL,
		[TestClass] [nvarchar](max) NULL,
		[TestMethod] [nvarchar](max) NULL,
		[Variation] [nvarchar](max) NULL,
		[MachineName] [nvarchar](max) NULL,
		[ProductReportingVersion] [nvarchar](max) NULL,
		[Framework] [nvarchar](max) NULL,
		[CustomData] [nvarchar](max) NULL,
		[RunStarted] [datetime2](7) NOT NULL,
		[WarmupIterations] [int] NOT NULL,
		[Iterations] [int] NOT NULL,
		[TimeElapsedAverage] [bigint] NOT NULL,
		[TimeElapsedPercentile90] [bigint] NOT NULL,
		[TimeElapsedPercentile95] [bigint] NOT NULL,
		[TimeElapsedPercentile99] [bigint] NOT NULL,
		[TimeElapsedStandardDeviation] [float] NOT NULL,
		[MemoryDeltaAverage] [bigint] NOT NULL,
		[MemoryDeltaPercentile90] [bigint] NOT NULL,
		[MemoryDeltaPercentile95] [bigint] NOT NULL,
		[MemoryDeltaPercentile99] [bigint] NOT NULL,
		[MemoryDeltaStandardDeviation] [float] NOT NULL)";

        public SqlServerBenchmarkResultProcessor(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void SaveSummary(BenchmarkRunSummary summary)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                EnsureRunsTableCreated(conn);
                WriteSummaryRecord(summary, conn);
            }
        }

        private void EnsureRunsTableCreated(SqlConnection conn)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = _tableCreationCommand;
            cmd.ExecuteNonQuery();
        }

        private static void WriteSummaryRecord(BenchmarkRunSummary summary, SqlConnection conn)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = _insertCommand;
            cmd.Parameters.AddWithValue("@TestClassFullName", summary.TestClassFullName);
            cmd.Parameters.AddWithValue("@TestClass", summary.TestClass);
            cmd.Parameters.AddWithValue("@TestMethod", summary.TestMethod);
            cmd.Parameters.AddWithValue("@Variation", summary.Variation);
            cmd.Parameters.AddWithValue("@MachineName", summary.MachineName);
            cmd.Parameters.AddWithValue("@ProductReportingVersion", summary.ProductReportingVersion);
            cmd.Parameters.AddWithValue("@Framework", summary.Framework);
            cmd.Parameters.Add("@CustomData", SqlDbType.NVarChar).Value = (object)summary.CustomData ?? DBNull.Value;
            cmd.Parameters.AddWithValue("@RunStarted", summary.RunStarted);
            cmd.Parameters.AddWithValue("@WarmupIterations", summary.WarmupIterations);
            cmd.Parameters.AddWithValue("@Iterations", summary.Iterations);
            cmd.Parameters.AddWithValue("@TimeElapsedAverage", summary.TimeElapsedAverage);
            cmd.Parameters.AddWithValue("@TimeElapsedPercentile99", summary.TimeElapsedPercentile99);
            cmd.Parameters.AddWithValue("@TimeElapsedPercentile95", summary.TimeElapsedPercentile95);
            cmd.Parameters.AddWithValue("@TimeElapsedPercentile90", summary.TimeElapsedPercentile90);
            cmd.Parameters.AddWithValue("@TimeElapsedStandardDeviation", summary.TimeElapsedStandardDeviation);
            cmd.Parameters.AddWithValue("@MemoryDeltaAverage", summary.MemoryDeltaAverage);
            cmd.Parameters.AddWithValue("@MemoryDeltaPercentile99", summary.MemoryDeltaPercentile99);
            cmd.Parameters.AddWithValue("@MemoryDeltaPercentile95", summary.MemoryDeltaPercentile95);
            cmd.Parameters.AddWithValue("@MemoryDeltaPercentile90", summary.MemoryDeltaPercentile90);
            cmd.Parameters.AddWithValue("@MemoryDeltaStandardDeviation", summary.MemoryDeltaStandardDeviation);

            cmd.ExecuteNonQuery();
        }
    }
}
