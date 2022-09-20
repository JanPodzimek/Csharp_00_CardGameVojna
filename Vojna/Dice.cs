using System;
namespace Vojna {
    public class Dice {
        private int sides;
        private Random rnd;
        public int Sides { get => sides; set => sides = value; }
        public Dice() {
            sides = 6;
            rnd = new Random();
        }
        public Dice(int sides) {
            this.sides = sides;
            rnd = new Random();
        }
        public int ThrowDice() {
            return rnd.Next(1, sides);
        }
        public int ThrowDice(int numberOfCardsInHand) {
            return rnd.Next(1, numberOfCardsInHand);
        }
    }
}

