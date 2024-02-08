#pragma once

#include <vector>
#include <string>

struct HandCharacteristic;
struct InformationFromAuction;


struct BoardCharacteristic
{
    bool hasFit;
    bool fitIsMajor;
    std::vector<int> partnersSuits{0,0,0,0};
    int fitWithPartnerSuit;
    int opponentsSuit;
    bool stopInOpponentsSuit;
    int keyCards;
    bool trumpQueen;
    int position;
    int lastBidId;
    bool isCompetitive;
    int minHcpPartner;
    bool allControlsPresent;
    BoardCharacteristic(HandCharacteristic hand, const std::string& previousBidding, const InformationFromAuction& informationFromAuction);
private:
    static int GetLongestSuit(const std::vector<int>& suitLengths);
    static bool GetHasStopInOpponentsSuit(const std::string& hand, int opponentsSuit);
    static bool GetAllControlsPresent(const HandCharacteristic& handCharacteristic, const InformationFromAuction& informationFromAuction, int fitWithPartnerSuit);
};

