namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
    public class PoweredVehicle : Vehicle
    {
        public Engine Engine { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as PoweredVehicle;
            return other != null
                   && base.Equals(other)
                   && Equals(Engine, other.Engine);
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
