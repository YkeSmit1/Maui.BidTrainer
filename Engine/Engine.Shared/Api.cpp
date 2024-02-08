// ReSharper disable CppLocalVariableMayBeConst
// ReSharper disable CppParameterMayBeConst
#include "Api.h"

#include <string>
#include "Rule.h"
#include "SqliteCppWrapper.h"
#include "BoardCharacteristic.h"
#include "InformationFromAuction.h"

std::unique_ptr<ISqliteWrapper> sqliteWrapper = nullptr;


HandCharacteristic GetHandCharacteristic(const std::string& hand)
{
    static HandCharacteristic handCharacteristic{};
    if (hand != handCharacteristic.hand)
    {
        handCharacteristic.Initialize(hand);
    }
    return handCharacteristic;
}

ISqliteWrapper* GetSqliteWrapper()
{
    if (sqliteWrapper == nullptr)
        throw std::logic_error("Setup was not called to initialize sqlite database");
    return sqliteWrapper.get();
}

int GetBidFromRule(const char* hand, const char* previousBidding, char* description)
{    
    auto handCharacteristic = GetHandCharacteristic(hand);
    InformationFromAuction informationFromAuction{ GetSqliteWrapper(), previousBidding};
    BoardCharacteristic boardCharacteristic{ handCharacteristic, previousBidding, informationFromAuction };

    auto isSlamBidding = informationFromAuction.isSlamBidding || ((handCharacteristic.hcp + boardCharacteristic.minHcpPartner >= 29 && boardCharacteristic.hasFit));

    auto [bidId, lDescription] = !isSlamBidding ?
        GetSqliteWrapper()->GetRule(handCharacteristic, boardCharacteristic, previousBidding) :
        GetSqliteWrapper()->GetRelativeRule(handCharacteristic, boardCharacteristic, informationFromAuction.previousSlamBidding);
    assert(lDescription.size() < 128);
    strcpy(description, lDescription.c_str());
    return bidId;
}

int Setup(const char* database)
{
    sqliteWrapper = std::make_unique<SqliteCppWrapper>(database);
    return 0;
}

void GetRulesByBid(int bidId, const char* previousBidding, char* information)
{
    InformationFromAuction informationFromAuction{ GetSqliteWrapper(), previousBidding };
    std::string lInformation;
    if (informationFromAuction.isSlamBidding)
        lInformation = GetSqliteWrapper()->GetRelativeRulesByBid(bidId, informationFromAuction.previousSlamBidding);
    else
        lInformation = GetSqliteWrapper()->GetRulesByBid(bidId, previousBidding);
    assert(lInformation.size() < 8192);
    strcpy(information, lInformation.c_str());
}

void SetModules(int modules)
{
    GetSqliteWrapper()->SetModules(modules);
}

void GetInformationFromAuction(const char* previousBidding, char* informationFromAuctionJson)
{
    InformationFromAuction informationFromAuction{ GetSqliteWrapper(), previousBidding };
    auto json = informationFromAuction.AsJson();
    assert(json.size() < 8192);
    strcpy(informationFromAuctionJson, json.c_str());
}
