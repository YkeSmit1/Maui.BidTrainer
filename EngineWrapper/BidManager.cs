using System;
using Common;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace EngineWrapper
{
    public static class BidManager
    {
        public static Bid GetBid(Auction auction, string handsString)
        {
            var description = new StringBuilder(128);
            var bidId = Pinvoke.GetBidFromRule(handsString, auction.GetBidsAsStringASCII(), description);

            if (bidId == 0)
            {
                var calculatedBid = GetCalculatedBid();
                if (calculatedBid > auction.currentContract)
                    return calculatedBid;
            }
            var bid = Bid.GetBid(bidId);
            bid.description = description.ToString();
            return bid;

            Bid GetCalculatedBid()
            {
                var info = GetInformationFromAuction();
                if ((long)info["minHcpPartner"] == 0)
                    return Bid.PassBid;
                var suits = handsString.Split(',');
                var minSuitLengthsPartner = info["minSuitLengthsPartner"]?.AsArray();
                if (minSuitLengthsPartner == null)
                    throw new InvalidOperationException(nameof(minSuitLengthsPartner));
                var majorFits = minSuitLengthsPartner
                    .Zip(suits, (x, y) => (int)x + y.Length)
                    .Take(2)
                    .Select((x, index) => (x, (Suit)(3 - index)))
                    .Where(z => z.x >= 8).ToList();
                var playingSuit = !majorFits.Any() ? Suit.NoTrump : majorFits.MaxBy(z => z.x).Item2;
                var hcpPartnership = Util.GetHcpCount(handsString) + (long)info["minHcpPartner"];

                if (hcpPartnership < 23)
                    return Util.IsSameTeam(auction.CurrentPlayer, auction.GetDeclarer()) && majorFits.Any()
                        ? Bid.CheapestContract(auction.currentContract, majorFits.First().Item2)
                        : Bid.PassBid;

                var rank = playingSuit == Suit.NoTrump ? 2 : 3;
                if (hcpPartnership >= 25)
                    if (hcpPartnership < 29)
                        rank++;
                    else
                        rank = SlamIsPossible() ? 6 : Bid.GetGameContract(playingSuit, auction.currentContract, true).Rank;

                return new Bid(rank, playingSuit);

                JsonNode GetInformationFromAuction()
                {
                    var stringBuilder = new StringBuilder(8129);
                    Pinvoke.GetInformationFromAuction(auction.GetBidsAsStringASCII(), stringBuilder);
                    return JsonNode.Parse(stringBuilder.ToString());
                }

                bool SlamIsPossible()
                {
                    if (playingSuit == Suit.NoTrump)
                        return hcpPartnership >= 33;
                    var totalKeyCards = Util.GetKeyCards(handsString, playingSuit) + (long)info["keyCardsPartner"];
                    var totalTrumpQueen = Util.GetHasTrumpQueen(handsString, playingSuit) || (bool)info["trumpQueenPartner"];

                    var controlsPartner = info["controls"]?.AsArray();
                    if (controlsPartner == null)
                        throw new InvalidOperationException(nameof(controlsPartner));

                    var controls = handsString.Split(",").Select((x, index) => (HasControl(x), index));
                    var controlsPartnership = controls.Select(x => x.Item1 || (bool)controlsPartner[x.index]);

                    var slamIsPossible = ((totalKeyCards == 4 && totalTrumpQueen) || totalKeyCards == 5) && controlsPartnership.All(x => x);
                    return slamIsPossible;

                    bool HasControl(string x) => x.Contains("A") || x.Contains("K") || x.Length <= 1;
                }
            }
        }

        public static string GetInformation(Bid bid, Auction auction)
        {
            var informationJson = new StringBuilder(8192);
            Pinvoke.GetRulesByBid(Bid.GetBidId(bid), auction.GetBidsAsStringASCII(), informationJson);

            var records = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(informationJson.ToString());
            var bidInformation = new BidInformation(records);
            return bidInformation.GetInformation();
        }
        
        public static Auction GetAuction(Dictionary<Player, string> deal, Player dealer)
        {
            var auction = new Auction();
            auction.Clear(dealer);
            while (!auction.IsEndOfBidding())
            {
                var bid = GetBid(auction, deal[auction.CurrentPlayer]);
                auction.AddBid(bid);
            }
            return auction;
        }
        
    }
}