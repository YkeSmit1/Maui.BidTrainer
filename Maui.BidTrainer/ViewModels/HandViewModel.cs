using System.Collections.ObjectModel;
using Common;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui.BidTrainer.ViewModels;

public class HandViewModel : ObservableObject
{
    public ObservableCollection<Card> Cards { get; set; } = [];

    public void ShowHand(string hand, bool alternateSuits, string cardProfile, Dictionary<(string suit, string card), string> dictionary)
    {
        var settings = CardImageSettings.GetCardImageSettings(cardProfile);
        Cards.Clear();

        List<Suit> suitOrder = alternateSuits
            ? [Suit.Spades, Suit.Hearts, Suit.Clubs, Suit.Diamonds]
            : [Suit.Spades, Suit.Hearts, Suit.Diamonds, Suit.Clubs];
        var suits = hand.Split(',').Select((x, index) => (x, (Suit)(3 - index))).OrderBy(x => suitOrder.IndexOf(x.Item2));
        var index = 0;

        foreach (var suit in suits)
        {
            foreach (var card in suit.x)
            {
                Cards.Add(new Card
                {
                    Rect = new Rect(index++ * settings.CardDistance, 0, settings.CardWidth, settings.CardHeight),
                    Source = dictionary[(Util.GetSuitDescriptionASCII(suit.Item2), card.ToString())],
                });
            }
        }
    }
}