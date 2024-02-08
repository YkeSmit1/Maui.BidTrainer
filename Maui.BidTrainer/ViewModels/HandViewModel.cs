using System.Collections.ObjectModel;
using System.Reflection;
using Common;
using MvvmHelpers;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;

namespace Maui.BidTrainer.ViewModels
{
    public class HandViewModel : BaseViewModel
    {
        public ObservableCollection<Card> Cards { get; set; } = new ObservableCollection<Card>();
        private SKBitmap croppedBitmap;

        public HandViewModel()
        {
            ShowHand("AQJ4,K32,843,QT9", true, "default");
        }

        public static SKBitmap LoadBitmapResource(Type type, string resourceId)
        {
            //var assembly = type.GetTypeInfo().Assembly;
            //using var stream = assembly.GetManifestResourceStream(resourceId);
            var names = Assembly.GetExecutingAssembly().GetManifestResourceNames();

            //var source = ImageSource.FromFile(resourceId);
            
            //var source2 = ImageSource.FromFile(resourceId);
            //var loadBitmapResource = SKBitmap.Decode(resourceId);
            //var loadBitmapResource2 = SKBitmap.Decode("Resources\\Images\\" + resourceId);

            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceId);
            return SKBitmap.Decode(stream);
        }

        public void ShowHand(string hand, bool alternateSuits, string cardProfile)
        {
            var settings = CardImageSettings.GetCardImageSettings(cardProfile);
            croppedBitmap = LoadBitmapResource(GetType(), settings.CardImage);
            Cards.Clear();
            var suitOrder = alternateSuits ?
                new List<Suit> { Suit.Spades, Suit.Hearts, Suit.Clubs, Suit.Diamonds } :
                new List<Suit> { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
            var suits = hand.Split(',').Select((x, index) => (x, (Suit)(3 - index))).OrderBy(x => suitOrder.IndexOf(x.Item2));
            var index = 0;
            var width = settings.CardWidth;
            var height = settings.CardHeight;

            foreach (var suit in suits)
            {
                foreach (var card in suit.x)
                {
                    var cardwidth = index == 12 ? width : settings.CardDistance;
                    var dest = new SKRect(0, 0, cardwidth, height);

                    var face = Util.GetFaceFromDescription(card);
                    var faceInt = settings.FirstCardIsAce ? (int)face : face == Face.Ace ? 12 : (int)face - 1;
                    var topx = settings.XOffSet + (width * faceInt);
                    var topy = suit.Item2 switch
                    {
                        Suit.Clubs => settings.TopClubs,
                        Suit.Diamonds => settings.TopDiamonds,
                        Suit.Hearts => settings.TopHearts,
                        Suit.Spades => settings.TopSpades,
                        _ => throw new ArgumentException(nameof(suit)),
                    };

                    var bitmap = new SKBitmap(cardwidth, height);
                    var source = new SKRect(topx, topy, topx + cardwidth, topy + height);

                    // Copy 1/52 of the original into that bitmap
                    using (var canvas = new SKCanvas(bitmap))
                    {
                        canvas.DrawBitmap(croppedBitmap, source, dest);
                    }

                    Cards.Add(new Card
                    {
                        Width = cardwidth - settings.XCardPadding,
                        Height = height,
                        ImageSource = (SKBitmapImageSource)bitmap
                    });
                    index++;
                }
            }
        }
    }
}
