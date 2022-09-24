using System;
namespace Vojna {
    public class Player {
        public static int playersNumber = 0;
        private string name;
        private List<Card> hand;
        private Dice dice;
        private ConsoleKeyInfo gameButton;
        public const int MAX_CARDS = 32;
        private int number;
        private int currentCards;
        private int playedCardValue;
        private int playedCardSuit;
        private List<Card> cardsOnTheTable;
        public string Name { get => name; set => name = value; }
        public List<Card> Hand { get => hand; set => hand = value; }
        public int CurrentCards { get => currentCards; set => currentCards = value; }
        public int PlayedCardValue { get => playedCardValue; set => playedCardValue = value; }
        public int PlayedCardSuit { get => playedCardSuit; set => playedCardSuit = value; }
        public List<Card> CardsOnTheTable { get => cardsOnTheTable; set => cardsOnTheTable = value; }
        public ConsoleKeyInfo GameButton { get => gameButton; set => gameButton = value; }
        public int Number { get => number; set => number = value; }

        public Player() {
            playersNumber++;
            number = playersNumber;
            this.name = "";
            currentCards = 16;
            hand = new List<Card>(currentCards);
            dice = new Dice();
            cardsOnTheTable = new List<Card>();
        }
        public Player(string name) {
            playersNumber++;
            number = playersNumber;
            this.name = name;
            currentCards = 16;
            hand = new List<Card>(currentCards);
            dice = new Dice();
            cardsOnTheTable = new List<Card>();
        }
        public int ChooseRandomCardFromPack() {
            return dice.ThrowDice(currentCards);
        }
        public string GetReadableValueOfCard(int value, int suit) {
            string stringValue = "", stringSuit = "";
            switch (value) {
                case 7: stringValue = "\"7\""; break;
                case 8: stringValue = "\"8\""; break;
                case 9: stringValue = "\"9\""; break;
                case 10: stringValue = "\"10\""; break;
                case 11: stringValue = "\"Jack\""; break;
                case 12: stringValue = "\"Queen\""; break;
                case 13: stringValue = "\"King\""; break;
                case 14: stringValue = "\"Ace\""; break;
                default: break;
            }
            switch (suit) {
                case 1: stringSuit = "Clubs"; break;
                case 2: stringSuit = "Diamonds"; break;
                case 3: stringSuit = "Hearts"; break;
                case 4: stringSuit = "Spades"; break;
                default: break;
            }
            return $"{stringValue} {stringSuit}";
        }
        public string GetReadableValueOfCard() {
            return GetReadableValueOfCard(playedCardValue, playedCardSuit);
        }
        public override string ToString() {
            return $"Player: {name}, Number of cards: {currentCards}";
        }
    }
}

