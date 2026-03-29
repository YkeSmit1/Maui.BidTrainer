namespace Engine.DotNet;

public interface ISqliteWrapper
{
    List<Dictionary<string, string>> GetInternalRulesByBid(int bidId, string currentBidding);
    List<Dictionary<string, string>> GetInternalRelativeRulesByBid(int bidId, string currentBidding);

    (int bidId, string description) GetRule(HandCharacteristic handCharacteristic, BoardCharacteristic boardCharacteristic, string previousBidding);
    (int bidId, string description) GetRelativeRule(HandCharacteristic handCharacteristic, BoardCharacteristic boardCharacteristic, string previousSlamBidding);

    string GetRulesByBid(int bidId, string previousBidding);
    string GetRelativeRulesByBid(int bidId, string previousSlamBidding);

    void SetModules(int modules);
}