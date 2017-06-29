namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
    public class LicensedOperator : Operator
    {
        public string LicenseType { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as LicensedOperator;
            return other != null
                   && base.Equals(other)
                   && LicenseType == other.LicenseType;
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
