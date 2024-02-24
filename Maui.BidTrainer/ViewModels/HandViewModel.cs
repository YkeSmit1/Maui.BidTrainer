using System.Collections.ObjectModel;
using Common;
using MvvmHelpers;

namespace Maui.BidTrainer.ViewModels
{
    public class HandViewModel : BaseViewModel
    {
        public ObservableCollection<Card> Cards { get; set; } = [];

        public HandViewModel()
        {
            ShowHand("AQJ4,K32,843,QT9", true, "default");
        }

        public void ShowHand(string hand, bool alternateSuits, string cardProfile)
        {
            var settings = CardImageSettings.GetCardImageSettings(cardProfile);
            Cards.Clear();
            var dictionary = SplitImages.Split(settings);

            var suitOrder = alternateSuits ?
                new List<Suit> { Suit.Spades, Suit.Hearts, Suit.Clubs, Suit.Diamonds } :
                new List<Suit> { Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs };
            var suits = hand.Split(',').Select((x, index) => (x, (Suit)(3 - index))).OrderBy(x => suitOrder.IndexOf(x.Item2));
            var index = 0;

            foreach (var suit in suits)
            {
                foreach (var card in suit.x)
                {
                    var valueTuple = (Util.GetSuitDescriptionASCII(suit.Item2), card.ToString());
                    Cards.Add(new Card
                    {
                        Rect = new Rect(index * settings.CardDistance, 0, settings.CardWidth, settings.CardHeight),
                        Source = dictionary[valueTuple],
                    });
                    index++;
                }
            }
        }
    }
}
