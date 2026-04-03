namespace Engine.DotNet;

public sealed partial class SqliteCppWrapper
{
    private sealed class RelativeRuleRow
    {
        public int? BidId { get; set; }
        public string? Description { get; set; }
        public string? PreviousBidding { get; set; }
        public int? LastBid { get; set; }
        public int? KeyCards { get; set; }
        public int? TrumpQueen { get; set; }
        public string? TrumpSuits { get; set; }
        public int? SpadeControl { get; set; }
        public int? HeartControl { get; set; }
        public int? DiamondControl { get; set; }
        public int? ClubControl { get; set; }
        public int? AllControlsPresent { get; set; }
        public string? FitWithPartner { get; set; }
        public int? Module { get; set; }
        public int? Priority { get; set; }

        public Dictionary<string, string> ToDictionary()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var prop in GetType().GetProperties())
            {
                var value = prop.GetValue(this);
                dict[prop.Name] = value?.ToString() ?? string.Empty;
            }

            return dict;
        }
    }
}