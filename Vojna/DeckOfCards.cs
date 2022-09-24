using System;
namespace Vojna {
    public class DeckOfCards {
        public const int AMOUNT_OF_CARDS = 32;
        private Card[] package;
        public Card[] Package { get => package; set => package = value; }
        public DeckOfCards() {
            package = getPackage();
        }
        public Card[] getPackage() {
            List<Card> package = new List<Card>(AMOUNT_OF_CARDS);
            bool unique;
            for (int i = 0; i < AMOUNT_OF_CARDS; i++) {
                Card card;
                do {
                    unique = true;
                    card = new Card();
                    int value = card.Value;
                    int suit = card.Suit;
                    foreach (Card c in package) {
                        if (c.Suit == suit && c.Value == value)
                            unique = false;
                    }
                } while (!unique);
                package.Add(card);
            }
            return package.ToArray();
        }
    }
}

