using System.Collections.ObjectModel;
using Common;
using Microsoft.Maui.Controls.Shapes;
using MvvmHelpers;

namespace Maui.BidTrainer.ViewModels
{
    public class HandViewModel : BaseViewModel
    {
        public ObservableCollection<Card> Cards { get; set; } = new ObservableCollection<Card>();

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
            var width = settings.CardWidth;
            var height = settings.CardHeight;

            foreach (var suit in suits)
            {
                foreach (var card in suit.x)
                {
                    var cardwidth = index == 12 ? width : settings.CardDistance;
                    var valueTuple = (Util.GetSuitDescriptionASCII(suit.Item2), card.ToString());
                    Cards.Add(new Card
                    {
                        Width = cardwidth - settings.XCardPadding,
                        Height = height,
                        Source = dictionary[valueTuple],
                    });
                    index++;
                }
            }
        }
    }
}
