#include "pch.h"
#include "../Engine/Engine.Shared/Rule.h"
#include "../Engine/Engine.Shared/BoardCharacteristic.h"
#include "../Engine/Engine.Shared/SQLiteCppWrapper.h"
#include "../Engine/Engine.Shared/InformationFromAuction.h"
#include "gtest/gtest.h"
#include "sqlite3.h"
#include <regex>

TEST(TestHandCharacteristic, TestName)
{
    HandCharacteristic handCharacteristic{};
    handCharacteristic.Initialize("A432,K5432,Q,J43");

    EXPECT_EQ(handCharacteristic.isBalanced, false);
    EXPECT_EQ(handCharacteristic.isSemiBalanced, true);
    EXPECT_EQ(handCharacteristic.suitLengths[0], 4);
    EXPECT_EQ(handCharacteristic.suitLengths[1], 5);
    EXPECT_EQ(handCharacteristic.suitLengths[2], 1);
    EXPECT_EQ(handCharacteristic.suitLengths[3], 3);
    EXPECT_EQ(handCharacteristic.lengthFirstSuit, 5);
    EXPECT_EQ(handCharacteristic.lengthSecondSuit, 4);
    EXPECT_EQ(handCharacteristic.firstSuit, 1);
    EXPECT_EQ(handCharacteristic.secondSuit, 0);
    EXPECT_EQ(handCharacteristic.lowestSuit, -1);
    EXPECT_EQ(handCharacteristic.highestSuit, -1);
    EXPECT_EQ(handCharacteristic.hcp, 10);

    handCharacteristic.Initialize("A32,K432,K432,J3");

    EXPECT_EQ(handCharacteristic.isBalanced, true);
    EXPECT_EQ(handCharacteristic.isSemiBalanced, true);
    EXPECT_EQ(handCharacteristic.suitLengths[0], 3);
    EXPECT_EQ(handCharacteristic.suitLengths[1], 4);
    EXPECT_EQ(handCharacteristic.suitLengths[2], 4);
    EXPECT_EQ(handCharacteristic.suitLengths[3], 2);
    EXPECT_EQ(handCharacteristic.lengthFirstSuit, 4);
    EXPECT_EQ(handCharacteristic.lengthSecondSuit, 4);
    EXPECT_EQ(handCharacteristic.firstSuit, -1);
    EXPECT_EQ(handCharacteristic.secondSuit, -1);
    EXPECT_EQ(handCharacteristic.lowestSuit, 2);
    EXPECT_EQ(handCharacteristic.highestSuit, 1);
    EXPECT_EQ(handCharacteristic.hcp, 11);

    handCharacteristic.Initialize("A,K432,J432,K432");
    EXPECT_EQ(handCharacteristic.lowestSuit, 3);
    EXPECT_EQ(handCharacteristic.highestSuit, 1);

    handCharacteristic.Initialize("K432,A,J432,K432");
    EXPECT_EQ(handCharacteristic.lowestSuit, 3);
    EXPECT_EQ(handCharacteristic.highestSuit, 0);

    handCharacteristic.Initialize("K432,J432,K432,A");
    EXPECT_EQ(handCharacteristic.lowestSuit, 2);
    EXPECT_EQ(handCharacteristic.highestSuit, 0);

    handCharacteristic.Initialize("A32,K432,K432,J3");
    EXPECT_EQ(handCharacteristic.isReverse, false);
    handCharacteristic.Initialize("A2,K5432,K432,J3");
    EXPECT_EQ(handCharacteristic.isReverse, false);
    handCharacteristic.Initialize("A2,K432,K5432,J3");
    EXPECT_EQ(handCharacteristic.isReverse, true);
    handCharacteristic.Initialize("A,K5432,K65432,J");
    EXPECT_EQ(handCharacteristic.isReverse, true);
    EXPECT_EQ(handCharacteristic.isSemiBalanced, false);

    handCharacteristic.Initialize("432,VB2,A,AK5432");
    EXPECT_EQ(handCharacteristic.controls[0], false);
    EXPECT_EQ(handCharacteristic.controls[1], false);
    EXPECT_EQ(handCharacteristic.controls[2], true);
    EXPECT_EQ(handCharacteristic.controls[3], true);

    handCharacteristic.Initialize("A65432,K65432,2,");
    EXPECT_EQ(handCharacteristic.controls[0], true);
    EXPECT_EQ(handCharacteristic.controls[1], true);
    EXPECT_EQ(handCharacteristic.controls[2], true);
    EXPECT_EQ(handCharacteristic.controls[3], true);
}

TEST(TestBoardCharacteristic, TestName)
{
    HandCharacteristic handCharacteristic{};
    handCharacteristic.Initialize("A432,K5432,Q,J43");
    InformationFromAuction informationFromAuction{};

    BoardCharacteristic boardCharacteristic{ handCharacteristic, "", informationFromAuction };
    EXPECT_EQ(boardCharacteristic.hasFit, false);
    EXPECT_EQ(boardCharacteristic.fitIsMajor, false);
    EXPECT_EQ(boardCharacteristic.opponentsSuit, -1);
    EXPECT_EQ(boardCharacteristic.fitWithPartnerSuit, -1);
    EXPECT_EQ(boardCharacteristic.stopInOpponentsSuit, false);

    informationFromAuction.openersSuits = { 0, 0, 4, 0 };
    informationFromAuction.partnersSuits = { 0, 0, 0, 4 };

    boardCharacteristic = BoardCharacteristic{ handCharacteristic, "1C", informationFromAuction };
    EXPECT_EQ(boardCharacteristic.hasFit, false);
    EXPECT_EQ(boardCharacteristic.fitIsMajor, false);
    EXPECT_EQ(boardCharacteristic.opponentsSuit, 2);
    EXPECT_EQ(boardCharacteristic.fitWithPartnerSuit, -1);
    EXPECT_EQ(boardCharacteristic.stopInOpponentsSuit, false);

    informationFromAuction.openersSuits = { 4, 0, 0, 0 };
    informationFromAuction.partnersSuits = { 0, 4, 0, 0 };

    boardCharacteristic = BoardCharacteristic{ handCharacteristic, "1H1SPass", informationFromAuction };
    EXPECT_EQ(boardCharacteristic.hasFit, true);
    EXPECT_EQ(boardCharacteristic.fitIsMajor, true);
    EXPECT_EQ(boardCharacteristic.opponentsSuit, 0);
    EXPECT_EQ(boardCharacteristic.fitWithPartnerSuit, 1);
    EXPECT_EQ(boardCharacteristic.stopInOpponentsSuit, true);
    EXPECT_EQ(boardCharacteristic.keyCards, 2);
    EXPECT_EQ(boardCharacteristic.trumpQueen, false);

    HandCharacteristic handCharacteristicSlam{};
    handCharacteristicSlam.Initialize("A432,Q5432,A,AJ4");
    boardCharacteristic = BoardCharacteristic{ handCharacteristicSlam, "1H1SPass", informationFromAuction };
    EXPECT_EQ(boardCharacteristic.hasFit, true);
    EXPECT_EQ(boardCharacteristic.fitIsMajor, true);
    EXPECT_EQ(boardCharacteristic.opponentsSuit, 0);
    EXPECT_EQ(boardCharacteristic.fitWithPartnerSuit, 1);
    EXPECT_EQ(boardCharacteristic.stopInOpponentsSuit, true);
    EXPECT_EQ(boardCharacteristic.keyCards, 3);
    EXPECT_EQ(boardCharacteristic.trumpQueen, true);
}

TEST(TestGetBidKindFromAuction, TestName)
{
    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("", 4), (int)BidKindAuction::UnknownSuit);

    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1H", 4), (int)BidKindAuction::UnknownSuit);

    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1HPass", 4), (int)BidKindAuction::UnknownSuit);
    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1HX", 5), (int)BidKindAuction::UnknownSuit);
    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1HPass", 8), (int)BidKindAuction::PartnersSuit);

    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1H1SPass", 5), (int)BidKindAuction::UnknownSuit);
    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1H1SX", 6), (int)BidKindAuction::UnknownSuit);
    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1H1SPass", 9), (int)BidKindAuction::PartnersSuit);

    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1DPass1SPass", 5), (int)BidKindAuction::UnknownSuit);
    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1DPass1SPass", 6), (int)BidKindAuction::NonReverse);
    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1DPass1SPass", 7), (int)BidKindAuction::OwnSuit);
    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1DPass1SPass", 8), (int)BidKindAuction::Reverse);
    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1DPass1SPass", 9), (int)BidKindAuction::PartnersSuit);

    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1DPass1HPass1NTPass", 6), (int)BidKindAuction::NonReverse);
    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1DPass1HPass1NTPass", 7), (int)BidKindAuction::PartnersSuit);
    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1DPass1HPass1NTPass", 8), (int)BidKindAuction::OwnSuit);
    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1DPass1HPass1NTPass", 9), (int)BidKindAuction::Reverse);
    EXPECT_EQ((int)SqliteCppWrapper::GetBidKindFromAuction("1DPass1HPass2SPass", 14), (int)BidKindAuction::PartnersSuit);
}

static void firstchar(sqlite3_context* context, int argc, sqlite3_value** argv)
{
    if (argc == 1)
    {
        auto* text = sqlite3_value_text(argv[0]);
        if (text && text[0])
        {
            char result[2];
            result[0] = text[0];
            result[1] = '\0';
            sqlite3_result_text(context, result, -1, SQLITE_TRANSIENT);
            return;
        }
    }
    sqlite3_result_null(context);
}

TEST(Database, createFunction_firstchar)
{
    SQLite::Database db(":memory:", SQLite::OPEN_READWRITE);
    db.exec("CREATE TABLE test (id INTEGER PRIMARY KEY, value TEXT)");

    EXPECT_EQ(1, db.exec("INSERT INTO test VALUES (NULL, \"first\")"));
    EXPECT_EQ(1, db.exec("INSERT INTO test VALUES (NULL, \"second\")"));

    // exception with SQL error: "no such function: firstchar"
    EXPECT_THROW(db.exec("SELECT firstchar(value) FROM test WHERE id=1"), SQLite::Exception);

    db.createFunction("firstchar", 1, true, nullptr, &firstchar, nullptr, nullptr, nullptr);

    EXPECT_EQ(1, db.exec("SELECT firstchar(value) FROM test WHERE id=1"));
}

static void regex_match(sqlite3_context* context, int argc, sqlite3_value** argv)
{
    if (argc == 2)
    {
        auto* regexp = (const char*)sqlite3_value_text(argv[0]);
        auto* text = (const char*)sqlite3_value_text(argv[1]);
        if (regexp && regexp[0] && text && text[0])
        {
            std::regex regex(regexp);
            auto match = std::regex_search(text, regex);
            sqlite3_result_int(context, match);
            return;
        }
    }
    sqlite3_result_null(context);
}

TEST(Database, createFunction_regex_match)
{
    SQLite::Database db(":memory:", SQLite::OPEN_READWRITE);
    db.exec("CREATE TABLE test (id INTEGER PRIMARY KEY, value TEXT)");

    EXPECT_EQ(1, db.exec("INSERT INTO test VALUES (NULL, \"first\")"));
    EXPECT_EQ(1, db.exec("INSERT INTO test VALUES (NULL, \"second\")"));

    // exception with SQL error: "no such function: regex_match"
    auto sql = "SELECT value FROM test WHERE regex_match('frst', value) = 1";
    EXPECT_THROW(SQLite::Statement query(db, sql), SQLite::Exception);

    db.createFunction("regex_match", 2, true, nullptr, &regex_match, nullptr, nullptr, nullptr);
    SQLite::Statement query(db, sql);
    query.executeStep();
    EXPECT_FALSE(query.hasRow()); // frst does not match first

    auto sql3 = "SELECT value FROM test WHERE regex_match('first', value) = 1";
    SQLite::Statement query2(db, sql3);
    query2.executeStep();
    EXPECT_TRUE(query2.hasRow()); // first does not match first
}