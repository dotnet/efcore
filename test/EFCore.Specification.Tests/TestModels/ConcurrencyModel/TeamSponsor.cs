namespace Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel
{
    public class TeamSponsor
    {
        public int TeamId { get; set; }
        public string SponsorId { get; set; }

        public virtual Team Team { get; set; }
        public virtual Sponsor Sponsor { get; set; }
    }
}
