namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
    public class Engine
    {
        public string VehicleName { get; set; }
        public string Description { get; set; }
        public PoweredVehicle Vehicle { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Engine;
            return other != null
                   && VehicleName == other.VehicleName
                   && Description == other.Description;
        }

        public override int GetHashCode() => VehicleName.GetHashCode();
    }
}
