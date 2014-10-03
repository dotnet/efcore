using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbContextPerfTests.Model;

namespace DbContextPerfTests
{
    public class DbContextAssociationPerfTests : DbContextPerfTestsBase
    {
        public void DbContextRelationshipFixup()
        {
            using (var context = new AdvWorksDbContext(ConnectionString, ServiceProvider, Options))
            {
                var x1 = context.ProductModels.ToList();
                var x2 = context.ProductSubcategories.ToList();

                //Materialize all dependents
                var x3 = context.Products.ToList();
            }
        }
    }
}
