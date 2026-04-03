namespace Engine.DotNet;

public sealed partial class SqliteCppWrapper
{
    private sealed class RuleRow
    {
        public int? BidId { get; set; }
        public int? BidSuitKind { get; set; }
        public string? Description { get; set; }
        public string? PreviousBidding { get; set; }
        public int? Id { get; set; }
        public int? BidKindAuction { get; set; }
        public int? IsOpponentsSuit { get; set; }
        public int? Priority { get; set; }
        public int? BidRank { get; set; }
        public int? Module { get; set; }
        public int? Position { get; set; }
        public int? IsCompetitive { get; set; }
        public int? IsReverse { get; set; }
        public int? IsSemiBalanced { get; set; }
        public int? RelevantIds { get; set; }
        public int? UseInCalculation { get; set; }

        public string? MinSpades { get; set; }
        public string? MaxSpades { get; set; }
        public string? MinHearts { get; set; }
        public string? MaxHearts { get; set; }
        public string? MinDiamonds { get; set; }
        public string? MaxDiamonds { get; set; }
        public string? MinClubs { get; set; }
        public string? MaxClubs { get; set; }
        public string? MinHcp { get; set; }
        public string? MaxHcp { get; set; }
        public string? IsBalanced { get; set; }
        public string? OpponentsSuit { get; set; }
        public string? StopInOpponentsSuit { get; set; }
        public string? MinFirstSuit { get; set; }
        public string? MaxFirstSuit { get; set; }
        public string? MinSecondSuit { get; set; }
        public string? MaxSecondSuit { get; set; }
        public string? HasFit { get; set; }
        public string? FitIsMajor { get; set; }
        public string? PreviousBiddingText { get; set; }
        public string? LastBid { get; set; }
        public string? KeyCards { get; set; }
        public string? TrumpQueen { get; set; }
        public string? FitWithPartner { get; set; }
        public string? SpadeControl { get; set; }
        public string? HeartControl { get; set; }
        public string? DiamondControl { get; set; }
        public string? ClubControl { get; set; }
        public string? AllControlsPresent { get; set; }

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