namespace WebApplication1.Models
{
    public class MemberStatisticsViewModel
    {
        public int TotalMembers { get; set; }
        public double AverageYearsOfExperience { get; set; }
        public int OrganicCount { get; set; }
        public int NonOrganicCount { get; set; }
        public int MembersWithPlots { get; set; }
        public int MembersWithoutPlots { get; set; }
        public Dictionary<string, int> MembersByTier { get; set; } = new();
        public string MostCommonTier { get; set; } = "N/A";
        public int NewestMemberYear { get; set; }
    }
}
