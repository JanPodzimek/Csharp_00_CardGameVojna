using System;
namespace Vojna {
    public class Card {
        private int value;
        private int suit;
        public int Value { get => value; set => this.value = value; }
        public int Suit { get => suit; set => suit = value; }
        public Card() {
            Random rnd = new Random();
            value = rnd.Next(7, 15);
            suit = rnd.Next(1, 5);
        }
        public Card(int value, int suit) {
            this.value = value;
            this.suit = suit;
        }
        
        public override string ToString() {
            return String.Format("{0} {1}", value, suit);
        }
    }
}

