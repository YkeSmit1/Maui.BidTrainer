namespace Maui.BidTrainer.Services;

public class CardService
{
    public Dictionary<(string suit, string card), string> Dictionary { get; private set; }

    public void GenerateCardImages()
    {
        var cardProfile = Preferences.Get("CardImageSettings", "default");
        var settings = CardImageSettings.GetCardImageSettings(cardProfile);
        Dictionary = SplitImages.Split(settings);
    }
}