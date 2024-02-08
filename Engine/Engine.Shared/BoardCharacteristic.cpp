// ReSharper disable CppCStyleCast
// ReSharper disable CppLocalVariableMayBeConst
// ReSharper disable CppParameterMayBeConst
#include "BoardCharacteristic.h"
#include <vector>
#include <algorithm>
#include "Rule.h"
#include "Utils.h"
#include "InformationFromAuction.h"

BoardCharacteristic::BoardCharacteristic(HandCharacteristic hand, const std::string& previousBidding, const InformationFromAuction& informationFromAuction)
{
    auto bidIds = Utils::SplitAuction(previousBidding);
    position = (int)bidIds.size() + 1;

    partnersSuits = informationFromAuction.partnersSuits;
    opponentsSuit = GetLongestSuit(informationFromAuction.openersSuits);
    stopInOpponentsSuit = GetHasStopInOpponentsSuit(hand.hand, opponentsSuit);

    std::vector<int> suitLengthCombined;
    std::transform(hand.suitLengths.begin(), hand.suitLengths.end(), partnersSuits.begin(), std::back_inserter(suitLengthCombined),
        [](const auto& x, const auto& y) {return x + y; });
    auto firstSuitWithFitIter = std::find_if(suitLengthCombined.begin(), suitLengthCombined.end(), 
        [](const auto& x) {return x >= 8; });
    fitWithPartnerSuit = firstSuitWithFitIter == suitLengthCombined.end() ? -1 : (int)std::distance(suitLengthCombined.begin(), firstSuitWithFitIter);
    hasFit = firstSuitWithFitIter != suitLengthCombined.end();
    fitIsMajor = suitLengthCombined.at(0) >= 8 || suitLengthCombined.at(1) >= 8;

    auto suits = Utils::Split<char>(hand.hand, ',');
    auto trumpSuit = fitWithPartnerSuit == -1 ? "" : suits.at(fitWithPartnerSuit);
    keyCards = Utils::NumberOfCards(hand.hand, 'A') + Utils::NumberOfCards(trumpSuit, 'K');
    trumpQueen = Utils::NumberOfCards(trumpSuit, 'Q');
    lastBidId = Utils::GetLastBidIdFromAuction(previousBidding);

    isCompetitive = Utils::GetIsCompetitive(previousBidding);
    minHcpPartner = informationFromAuction.minHcpPartner;
    allControlsPresent = GetAllControlsPresent(hand, informationFromAuction, fitWithPartnerSuit);
}

int BoardCharacteristic::GetLongestSuit(const std::vector<int>& suitLengths)
{
    return !std::any_of(suitLengths.begin(), suitLengths.end(), [](const auto& x) {return x > 0; }) ? -1 :
        (int)std::distance(suitLengths.begin(), std::max_element(suitLengths.begin(), suitLengths.end()));
}

bool BoardCharacteristic::GetHasStopInOpponentsSuit(const std::string& hand, int opponentsSuit)
{
    if (opponentsSuit == -1)
        return false;
    auto cardsInOpponentSuit = Utils::Split<char>(hand, ',')[opponentsSuit];
    if (cardsInOpponentSuit.length() == 0)
        return false;

    switch (cardsInOpponentSuit[0])
    {
    case 'A': return true;
    case 'K': return cardsInOpponentSuit.length() >= 2;
    case 'Q': return cardsInOpponentSuit.length() >= 3;
    case 'J': return cardsInOpponentSuit.length() >= 4;
    default:
        return false;
    }
}

bool BoardCharacteristic::GetAllControlsPresent(const HandCharacteristic& handCharacteristic, const InformationFromAuction& informationFromAuction, int fitWithPartnerSuit)
{
    std::vector<bool> controlsCombined;
    std::copy(informationFromAuction.controls.begin(), informationFromAuction.controls.end(), std::back_inserter(controlsCombined));
    std::transform(controlsCombined.begin(), controlsCombined.end(), handCharacteristic.controls.begin(), controlsCombined.begin(), [](auto a, auto b) {return a || b;  });
    for (int i = 0; i < 3; i++)
    {
        if (!controlsCombined.at(i) && i != fitWithPartnerSuit)
            return false;
    }
    return true;
}