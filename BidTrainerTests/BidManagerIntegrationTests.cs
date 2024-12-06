using Xunit;
using System;
using EngineWrapper;
using System.IO;
using Common;
using Xunit.Abstractions;

namespace BidTrainerTests;

public class BidManagerIntegrationTests
{
    private const string PbnPath = "..\\..\\..\\..\\Maui.BidTrainer\\Resources\\Raw";
    private readonly ITestOutputHelper testOutputHelper;

    public BidManagerIntegrationTests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
        var _ = Pinvoke.Setup("four_card_majors.db3");
    }

    [Fact]
    public void BidTest()
    {
        var resultsPath = Path.Combine(PbnPath, "results");
        Directory.CreateDirectory(resultsPath);
        foreach (var pbnFile in Directory.GetFiles(PbnPath, "*.pbn"))
        {
            var pbn = BidPbnFile(pbnFile);
            SaveAndCheckPbn(pbn, pbnFile, resultsPath);
        }
    }

    private Pbn BidPbnFile(string pbnFile)
    {
        testOutputHelper.WriteLine($"Executing file {pbnFile}");
        var pbn = LoadPbnFile(pbnFile);
        foreach (var board in pbn.Boards)
        {
            BidBoard(board);
        }
        return pbn;
    }

    private static void SaveAndCheckPbn(Pbn pbn, string pbnFile, string resultsPath)
    {
        var filePath = $"{Path.Combine(resultsPath, Path.GetFileName(pbnFile))}.{DateTime.Now:d MMM yyyy}";
        pbn.Save(filePath);
        Assert.Equal(File.ReadAllText($"{pbnFile}.etalon"), File.ReadAllText(filePath));
    }

    private static Pbn LoadPbnFile(string pbnFile)
    {
        var pbn = new Pbn();
        var modules = Path.GetFileName(pbnFile) switch
        {
            "lesson2.pbn" => 1,
            "lesson3.pbn" => 1,
            "lesson4.pbn" => 5,
            "CursusSlotdrive.pbn" => 1,
            "lesson5.pbn" => 6,
            "lesson6.pbn" => 30,
            "lesson7.pbn" => 126,
            _ => 127
        };
        Pinvoke.SetModules(modules);
        pbn.Load(pbnFile);
        return pbn;
    }

    private void BidBoard(BoardDto board)
    {
        var auction = BidManager.GetAuction(board.Deal, board.Dealer);
        board.Auction = auction;
        board.Declarer = auction.GetDeclarer();
        testOutputHelper.WriteLine($"Board:{board.BoardNumber}");
        testOutputHelper.WriteLine(auction.GetAuctionAll("|"));
    }

    [Fact]
    public void BidSpecificBoard()
    {
        var pbn = LoadPbnFile(Path.Combine(PbnPath, "lesson7.pbn"));
        BidBoard(pbn.Boards[0]);
    }

    [Fact]
    public void BidSpecificPbnFile()
    {
        var pbnFile = Path.Combine(PbnPath, "lesson7.pbn");
        var pbn = BidPbnFile(pbnFile);
        var resultsPath = Path.Combine(Path.GetDirectoryName(pbnFile)!, "results");
        Directory.CreateDirectory(resultsPath);
        SaveAndCheckPbn(pbn, pbnFile, resultsPath);
    }
}