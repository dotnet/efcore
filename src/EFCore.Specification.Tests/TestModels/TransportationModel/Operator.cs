namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
    public class Operator
    {
        public string VehicleName { get; set; }
        public string Name { get; set; }
        public Vehicle Vehicle { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Operator;
            return other != null
                   && VehicleName == other.VehicleName
                   && Name == other.Name;
        }

        public override int GetHashCode() => VehicleName.GetHashCode();
    }
}
