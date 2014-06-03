using System;
using System.Globalization;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests.Helpers
{
    public class Purchase 
    {
        protected bool Equals(Purchase other)
        {
            // intentionally leaves out Timestamp (changed by server)
            return string.Equals(PartitionKey, other.PartitionKey) && string.Equals(RowKey, other.RowKey) && string.Equals(ETag, other.ETag) && Cost.Equals(other.Cost) && string.Equals(Name, other.Name) && Purchased.Equals(other.Purchased) && Count == other.Count && GlobalGuid.Equals(other.GlobalGuid) && Awesomeness.Equals(other.Awesomeness);
        }

        public static bool operator ==(Purchase left, Purchase right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Purchase left, Purchase right)
        {
            return !Equals(left, right);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (PartitionKey != null ? PartitionKey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RowKey != null ? RowKey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Timestamp.GetHashCode();
                hashCode = (hashCode * 397) ^ (ETag != null ? ETag.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Cost.GetHashCode();
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Purchased.GetHashCode();
                hashCode = (hashCode * 397) ^ Count;
                hashCode = (hashCode * 397) ^ GlobalGuid.GetHashCode();
                hashCode = (hashCode * 397) ^ Awesomeness.GetHashCode();
                return hashCode;
            }
        }

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
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((Purchase)obj);
        }
    }
}