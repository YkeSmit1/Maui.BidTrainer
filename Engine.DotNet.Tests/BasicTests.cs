using Microsoft.Data.Sqlite;

namespace Engine.DotNet.Tests;

public class BasicTests
{
    [Fact]
    public void TestHandCharacteristic()
    {
        var handCharacteristic = new HandCharacteristic("A432,K5432,Q,J43");

        Assert.False(handCharacteristic.IsBalanced);
        Assert.True(handCharacteristic.IsSemiBalanced);
        Assert.Equal(4, handCharacteristic.SuitLengths[0]);
        Assert.Equal(5, handCharacteristic.SuitLengths[1]);
        Assert.Equal(1, handCharacteristic.SuitLengths[2]);
        Assert.Equal(3, handCharacteristic.SuitLengths[3]);
        Assert.Equal(5, handCharacteristic.LengthFirstSuit);
        Assert.Equal(4, handCharacteristic.LengthSecondSuit);
        Assert.Equal(1, handCharacteristic.FirstSuit);
        Assert.Equal(0, handCharacteristic.SecondSuit);
        Assert.Equal(-1, handCharacteristic.LowestSuit);
        Assert.Equal(-1, handCharacteristic.HighestSuit);
        Assert.Equal(10, handCharacteristic.Hcp);

        handCharacteristic.Initialize("A32,K432,K432,J3");

        Assert.True(handCharacteristic.IsBalanced);
        Assert.True(handCharacteristic.IsSemiBalanced);
        Assert.Equal(3, handCharacteristic.SuitLengths[0]);
        Assert.Equal(4, handCharacteristic.SuitLengths[1]);
        Assert.Equal(4, handCharacteristic.SuitLengths[2]);
        Assert.Equal(2, handCharacteristic.SuitLengths[3]);
        Assert.Equal(4, handCharacteristic.LengthFirstSuit);
        Assert.Equal(4, handCharacteristic.LengthSecondSuit);
        Assert.Equal(-1, handCharacteristic.FirstSuit);
        Assert.Equal(-1, handCharacteristic.SecondSuit);
        Assert.Equal(2, handCharacteristic.LowestSuit);
        Assert.Equal(1, handCharacteristic.HighestSuit);
        Assert.Equal(11, handCharacteristic.Hcp);

        handCharacteristic.Initialize("A,K432,J432,K432");
        Assert.Equal(3, handCharacteristic.LowestSuit);
        Assert.Equal(1, handCharacteristic.HighestSuit);

        handCharacteristic.Initialize("K432,A,J432,K432");
        Assert.Equal(3, handCharacteristic.LowestSuit);
        Assert.Equal(0, handCharacteristic.HighestSuit);

        handCharacteristic.Initialize("K432,J432,K432,A");
        Assert.Equal(2, handCharacteristic.LowestSuit);
        Assert.Equal(0, handCharacteristic.HighestSuit);

        handCharacteristic.Initialize("A32,K432,K432,J3");
        Assert.False(handCharacteristic.IsReverse);
        handCharacteristic.Initialize("A2,K5432,K432,J3");
        Assert.False(handCharacteristic.IsReverse);
        handCharacteristic.Initialize("A2,K432,K5432,J3");
        Assert.True(handCharacteristic.IsReverse);
        handCharacteristic.Initialize("A,K5432,K65432,J");
        Assert.True(handCharacteristic.IsReverse);
        Assert.False(handCharacteristic.IsSemiBalanced);

        handCharacteristic.Initialize("432,VB2,A,AK5432");
        Assert.False(handCharacteristic.Controls[0]);
        Assert.False(handCharacteristic.Controls[1]);
        Assert.True(handCharacteristic.Controls[2]);
        Assert.True(handCharacteristic.Controls[3]);

        handCharacteristic.Initialize("A65432,K65432,2,");
        Assert.True(handCharacteristic.Controls[0]);
        Assert.True(handCharacteristic.Controls[1]);
        Assert.True(handCharacteristic.Controls[2]);
        Assert.True(handCharacteristic.Controls[3]);
    }

    [Fact]
    public void TestBoardCharacteristic()
    {
        var handCharacteristic = new HandCharacteristic("A432,K5432,Q,J43");
        var informationFromAuction = new InformationFromAuction();

        var boardCharacteristic = new BoardCharacteristic(handCharacteristic, "", informationFromAuction);
        Assert.False(boardCharacteristic.HasFit);
        Assert.False(boardCharacteristic.FitIsMajor);
        Assert.Equal(-1, boardCharacteristic.OpponentsSuit);
        Assert.Equal(-1, boardCharacteristic.FitWithPartnerSuit);
        Assert.False(boardCharacteristic.StopInOpponentsSuit);

        informationFromAuction = new InformationFromAuction { OpenersSuits = [0, 0, 4, 0], PartnersSuits = [0, 0, 0, 4] };

        boardCharacteristic = new BoardCharacteristic(handCharacteristic, "1C", informationFromAuction);
        Assert.False(boardCharacteristic.HasFit);
        Assert.False(boardCharacteristic.FitIsMajor);
        Assert.Equal(2, boardCharacteristic.OpponentsSuit);
        Assert.Equal(-1, boardCharacteristic.FitWithPartnerSuit);
        Assert.False(boardCharacteristic.StopInOpponentsSuit);

        informationFromAuction = new InformationFromAuction { OpenersSuits = [4, 0, 0, 0], PartnersSuits = [0, 4, 0, 0] };

        boardCharacteristic = new BoardCharacteristic(handCharacteristic, "1H1SPass", informationFromAuction);
        Assert.True(boardCharacteristic.HasFit);
        Assert.True(boardCharacteristic.FitIsMajor);
        Assert.Equal(0, boardCharacteristic.OpponentsSuit);
        Assert.Equal(1, boardCharacteristic.FitWithPartnerSuit);
        Assert.True(boardCharacteristic.StopInOpponentsSuit);
        Assert.Equal(2, boardCharacteristic.KeyCards);
        Assert.False(boardCharacteristic.TrumpQueen);

        var handCharacteristicSlam = new HandCharacteristic("A432,Q5432,A,AJ4");
        boardCharacteristic = new BoardCharacteristic(handCharacteristicSlam, "1H1SPass", informationFromAuction);
        Assert.True(boardCharacteristic.HasFit);
        Assert.True(boardCharacteristic.FitIsMajor);
        Assert.Equal(0, boardCharacteristic.OpponentsSuit);
        Assert.Equal(1, boardCharacteristic.FitWithPartnerSuit);
        Assert.True(boardCharacteristic.StopInOpponentsSuit);
        Assert.Equal(3, boardCharacteristic.KeyCards);
        Assert.True(boardCharacteristic.TrumpQueen);
    }

    [Fact]
    public void TestGetBidKindFromAuction()
    {
        Assert.Equal((int)BidKindAuction.UnknownSuit, (int)SqliteCppWrapper.GetBidKindFromAuction("", 4));

        Assert.Equal((int)BidKindAuction.UnknownSuit, (int)SqliteCppWrapper.GetBidKindFromAuction("1H", 4));

        Assert.Equal((int)BidKindAuction.UnknownSuit, (int)SqliteCppWrapper.GetBidKindFromAuction("1HPass", 4));
        Assert.Equal((int)BidKindAuction.UnknownSuit, (int)SqliteCppWrapper.GetBidKindFromAuction("1HX", 5));
        Assert.Equal((int)BidKindAuction.PartnersSuit, (int)SqliteCppWrapper.GetBidKindFromAuction("1HPass", 8));

        Assert.Equal((int)BidKindAuction.UnknownSuit, (int)SqliteCppWrapper.GetBidKindFromAuction("1H1SPass", 5));
        Assert.Equal((int)BidKindAuction.UnknownSuit, (int)SqliteCppWrapper.GetBidKindFromAuction("1H1SX", 6));
        Assert.Equal((int)BidKindAuction.PartnersSuit, (int)SqliteCppWrapper.GetBidKindFromAuction("1H1SPass", 9));

        Assert.Equal((int)BidKindAuction.UnknownSuit, (int)SqliteCppWrapper.GetBidKindFromAuction("1DPass1SPass", 5));
        Assert.Equal((int)BidKindAuction.NonReverse, (int)SqliteCppWrapper.GetBidKindFromAuction("1DPass1SPass", 6));
        Assert.Equal((int)BidKindAuction.OwnSuit, (int)SqliteCppWrapper.GetBidKindFromAuction("1DPass1SPass", 7));
        Assert.Equal((int)BidKindAuction.Reverse, (int)SqliteCppWrapper.GetBidKindFromAuction("1DPass1SPass", 8));
        Assert.Equal((int)BidKindAuction.PartnersSuit, (int)SqliteCppWrapper.GetBidKindFromAuction("1DPass1SPass", 9));

        Assert.Equal((int)BidKindAuction.NonReverse, (int)SqliteCppWrapper.GetBidKindFromAuction("1DPass1HPass1NTPass", 6));
        Assert.Equal((int)BidKindAuction.PartnersSuit, (int)SqliteCppWrapper.GetBidKindFromAuction("1DPass1HPass1NTPass", 7));
        Assert.Equal((int)BidKindAuction.OwnSuit, (int)SqliteCppWrapper.GetBidKindFromAuction("1DPass1HPass1NTPass", 8));
        Assert.Equal((int)BidKindAuction.Reverse, (int)SqliteCppWrapper.GetBidKindFromAuction("1DPass1HPass1NTPass", 9));
        Assert.Equal((int)BidKindAuction.PartnersSuit, (int)SqliteCppWrapper.GetBidKindFromAuction("1DPass1HPass2SPass", 14));
    }

    private static void RegisterScalarFunction(SqliteConnection db, string name, int argc, Func<object?[], object?> implementation)
    {
        db.CreateFunction(name, args => implementation(args), isDeterministic: true);
    }

    [Fact]
    public void CreateFunction_FirstChar()
    {
        using var db = new SqliteConnection("Data Source=:memory:");
        db.Open();

        using (var cmd = db.CreateCommand())
        {
            cmd.CommandText = "CREATE TABLE test (id INTEGER PRIMARY KEY, value TEXT);";
            cmd.ExecuteNonQuery();
        }

        using (var cmd = db.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO test VALUES (NULL, 'first');";
            Assert.Equal(1, cmd.ExecuteNonQuery());

            cmd.CommandText = "INSERT INTO test VALUES (NULL, 'second');";
            Assert.Equal(1, cmd.ExecuteNonQuery());
        }

        Assert.Throws<SqliteException>(() =>
        {
            using var cmd = db.CreateCommand();
            cmd.CommandText = "SELECT firstchar(value) FROM test WHERE id=1;";
            cmd.ExecuteNonQuery();
        });

        db.CreateFunction<string?, string?>("firstchar", value =>
            string.IsNullOrEmpty(value) ? null : value[..1]);

        using (var cmd = db.CreateCommand())
        {
            cmd.CommandText = "SELECT firstchar(value) FROM test WHERE id=1;";
            Assert.Equal(-1, cmd.ExecuteNonQuery());
        }
    }

    [Fact]
    public void CreateFunction_RegexMatch()
    {
        using var db = new SqliteConnection("Data Source=:memory:");
        db.Open();

        using (var cmd = db.CreateCommand())
        {
            cmd.CommandText = "CREATE TABLE test (id INTEGER PRIMARY KEY, value TEXT);";
            cmd.ExecuteNonQuery();
        }

        using (var cmd = db.CreateCommand())
        {
            cmd.CommandText = "INSERT INTO test VALUES (NULL, 'first');";
            Assert.Equal(1, cmd.ExecuteNonQuery());

            cmd.CommandText = "INSERT INTO test VALUES (NULL, 'second');";
            Assert.Equal(1, cmd.ExecuteNonQuery());
        }

        Assert.Throws<SqliteException>(() =>
        {
            using var cmd = db.CreateCommand();
            cmd.CommandText = "SELECT value FROM test WHERE regex_match('frst', value) = 1;";
            cmd.ExecuteReader();
        });

        db.CreateFunction<string?, string?, int?>("regex_match", (regexp, text) =>
        {
            if (string.IsNullOrWhiteSpace(regexp) || string.IsNullOrWhiteSpace(text))
                return null;

            return System.Text.RegularExpressions.Regex.IsMatch(text, regexp) ? 1 : 0;
        });

        using (var cmd = db.CreateCommand())
        {
            cmd.CommandText = "SELECT value FROM test WHERE regex_match('frst', value) = 1;";
            using var reader = cmd.ExecuteReader();
            Assert.False(reader.Read());
        }

        using (var cmd = db.CreateCommand())
        {
            cmd.CommandText = "SELECT value FROM test WHERE regex_match('first', value) = 1;";
            using var reader = cmd.ExecuteReader();
            Assert.True(reader.Read());
        }
    }
}
