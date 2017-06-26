namespace Microsoft.EntityFrameworkCore.TestModels.TransportationModel
{
    public class CombustionEngine : Engine
    {
        public FuelTank FuelTank { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as CombustionEngine;
            return other != null
                   && base.Equals(other)
                   && Equals(FuelTank, other.FuelTank);
        }

        public override int GetHashCode() => base.GetHashCode();
    }
}
