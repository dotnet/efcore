using System;
using System.Globalization;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests.Helpers
{
    public class Purchase 
    {
        public Purchase()
        {
            Purchased = DateTime.Parse("Jan 1, 1601 00:00:00 GMT", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string ETag { get; set; }
        public double Cost { get; set; }
        public string Name { get; set; }
        public DateTime Purchased { get; set; }
        public int Count { get; set; }
        public Guid GlobalGuid { get; set; }
        public bool Awesomeness { get; set; }
        // override object.Equals
        public override bool Equals(object obj)
        {
            var other = obj as Purchase;
            if (other == null)
            {
                return false;
            }
            else if (PartitionKey != other.PartitionKey)
            {
                return false;
            }
            else if (RowKey != other.RowKey)
            {
                return false;
            }
            else if (Cost != other.Cost)
            {
                return false;
            }
            else if (Name != other.Name)
            {
                return false;
            }
            else if (Count != other.Count)
            {
                return false;
            }
            else if (!GlobalGuid.Equals(other.GlobalGuid))
            {
                return false;
            }
            else if (Awesomeness != other.Awesomeness)
            {
                return false;
            }
            return Purchased.ToUniversalTime().Equals(other.Purchased.ToUniversalTime());
        }
    }
}