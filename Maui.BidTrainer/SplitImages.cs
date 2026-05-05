using System.Reflection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace Maui.BidTrainer;

public static class SplitImages
{
    public static Dictionary<(string suit ,string card), string> Split(CardImageSettings imageSettings)
    {
        var fileNames = ExtractAndSaveImages(imageSettings);
        return CreateLookup(imageSettings, fileNames.ToArray());
    }

    private static IEnumerable<string> ExtractAndSaveImages(CardImageSettings imageSettings)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"Maui.BidTrainer.Resources.Images.Embedded.{imageSettings.CardImage}");
        using var originalImage = Image.Load(stream ?? throw new InvalidOperationException("CardImage not found in resources"));
        var counter = 0;

        for (var i = 0; i < 4; i++)
            for (var j = 0; j < 13; j++)
            {
                var left = imageSettings.XOffSet + j * imageSettings.CardWidth;
                var top = imageSettings.YOffSet + i * imageSettings.CardHeight;
                var width = imageSettings.CardWidth - imageSettings.XCardPadding;
                var height = imageSettings.CardHeight - imageSettings.YCardPadding;

                var imagePath = Path.Combine(FileSystem.CacheDirectory, $"{imageSettings.CardImage}-image-{counter}.png");
                // Clone and crop the card
                using var cardImage = originalImage.Clone(ctx => ctx.Crop(new Rectangle(left, top, width, height)));
                cardImage.SaveAsPng(imagePath);
                yield return imagePath;
                counter++;
            }
    }

    private static Dictionary<(string suit ,string card), string> CreateLookup(CardImageSettings cardImageSettings, IReadOnlyList<string> fileNames)
    {
        Dictionary<(string suit ,string card), string> lookup = [];

        var counter = 0;
        foreach (var suit in cardImageSettings.SuitOrder)
        {
            foreach (var card in cardImageSettings.CardOrder)
            {
                lookup.Add((suit.ToString(), card.ToString()), fileNames[counter]);
                counter++;
            }
        }

        return lookup;
    }
}