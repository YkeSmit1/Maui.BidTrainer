namespace Maui.BidTrainer
{
    public class CardImageSettings
    {
        public string CardImage { get; set; }
        public int XOffSet { get; set; }
        public int YOffSet { get; set; }
        public int CardWidth { get; set; }
        public int CardHeight { get; set; }
        public int XCardPadding { get; set; }
        public int YCardPadding { get; set; }
        public int CardDistance { get; set; }
        public string CardOrder { get; set; }
        public string SuitOrder { get; set; }

        private static readonly CardImageSettings DefaultCardImageSettings = new CardImageSettings
        {
            CardImage = "cardfaces.png",
            XOffSet = 0,
            YOffSet = 0,
            CardWidth = 73,
            CardHeight = 98,
            XCardPadding = 0,
            YCardPadding = 0,
            CardDistance = 20,
            CardOrder = "A23456789TJQK",
            SuitOrder = "CSHD"
        };

        private static readonly CardImageSettings BboCardImageSettings = new CardImageSettings
        {
            CardImage = "cardfaces2.jpg",
            XOffSet = 14,
            YOffSet = 12,
            CardWidth = 38,
            CardHeight = 62,
            XCardPadding = 4,
            YCardPadding = 14,
            CardDistance = 32,
            CardOrder = "23456789TJQKA",
            SuitOrder = "DHCS"
        };

        public static CardImageSettings GetCardImageSettings(string settings)
        {
            return settings switch
            {
                "default" => DefaultCardImageSettings,
                "bbo" => BboCardImageSettings,
                _ => throw new NotImplementedException(),
            };
        }

    }
}
