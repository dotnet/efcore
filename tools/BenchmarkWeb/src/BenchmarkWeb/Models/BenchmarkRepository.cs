using Microsoft.Data.Entity;
using System.Collections.Generic;
using System.Linq;

namespace BenchmarkWeb.Models
{
    public class BenchmarkRepository
    {
        private readonly BenchmarkContext _context;
        private static readonly string _latestResultsSql =
@"SELECT Runs.*
FROM 
	dbo.Runs AS Runs
	INNER JOIN (SELECT TestClass, TestMethod, Variation, ProductReportingVersion, Framework, MAX(RunStarted) LastRun
				FROM dbo.Runs
                WHERE RunStarted > GETDATE() - 30
				GROUP BY TestClass, TestMethod, Variation, ProductReportingVersion, Framework) AS LastRuns
	ON
		Runs.TestClass = LastRuns.TestClass
		AND Runs.TestMethod = LastRuns.TestMethod
		AND Runs.Variation = LastRuns.Variation
		AND Runs.ProductReportingVersion = LastRuns.ProductReportingVersion
		AND Runs.Framework = LastRuns.Framework
		AND Runs.RunStarted = LastRuns.LastRun";

        public BenchmarkRepository(BenchmarkContext context)
        {
            _context = context;
        }

        public IEnumerable<Run> GetLatestResults()
        {
            return _context.Runs
                .FromSql(_latestResultsSql)
                .ToList();
        }

        public IEnumerable<Run> GetTestHistory(string testClass, string testMethod)
        {
            return _context.Runs
                .Where(r => r.TestClass == testClass && r.TestMethod == testMethod && r.Framework == ".NETFramework")
                .OrderByDescending(r => r.RunStarted)
                .Take(30);
        }
    }
}
