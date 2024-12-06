using SkiaSharp;
using System.Reflection;

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
            var bitmap = SKBitmap.Decode(stream);
            var counter = 0;

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 13; j++)
                {
                    var newBitmap = new SKBitmap(imageSettings.CardWidth, imageSettings.CardHeight);
                    var left = imageSettings.XOffSet + j * imageSettings.CardWidth;
                    var top = imageSettings.YOffSet + i * imageSettings.CardHeight;
                    var source = new SKRect(left, top, left + imageSettings.CardWidth - imageSettings.XCardPadding,
                        top + imageSettings.CardHeight - imageSettings.YCardPadding);
                    var dest = new SKRect(0, 0, imageSettings.CardWidth, imageSettings.CardHeight);
                    // Copy 1/52 of the original into that bitmap
                    using (var canvas = new SKCanvas(newBitmap))
                    {
                        canvas.DrawBitmap(bitmap, source, dest);
                    }

                    yield return SaveBitmapToFile(newBitmap, counter, imageSettings);
                    counter++;
                }

            static string SaveBitmapToFile(SKBitmap bitmap, int counter, CardImageSettings imageSettings)
            {
                SKImage image = SKImage.FromBitmap(bitmap);
                using SKData encodedData = image.Encode(SKEncodedImageFormat.Png, 100);
                string imagePath = Path.Combine(FileSystem.CacheDirectory, $"{imageSettings.CardImage}-image-{counter}.png");
                using var bitmapImageStream = File.Open(imagePath, FileMode.Create, FileAccess.Write, FileShare.None);
                encodedData.SaveTo(bitmapImageStream);

                return imagePath;
            }
        }


    private static Dictionary<(string suit ,string card), string> CreateLookup(CardImageSettings cardImageSettings, IReadOnlyList<string> fileNames)
    {
            Dictionary<(string suit ,string card), string> lookup = [];

            int counter = 0;
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