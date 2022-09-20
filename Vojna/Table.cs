using System;
using System.Numerics;

namespace Vojna {
    public class Table {
        private Dice dice;
        private Player player1;
        private Player player2;
        private DeckOfCards deckOfCards;
        private string message;
        public const int SIZE_OF_PACKAGE = 32;
        public string Message { get => message; set => message = value; }
        public Table(Dice dice, Player player1, Player player2) {
            this.dice = dice;
            this.player1 = player1;
            this.player2 = player2;
            deckOfCards = new DeckOfCards();
            message = "";
        }
//====================================================================================================================
//                                                  THE GAME
//====================================================================================================================
        public void Game() {
            SetPlayersName(player1);
            SetPlayersGameButton(player1);
            SetPlayersName(player2);
            SetPlayersGameButton(player2);
            DealTheCards(deckOfCards);
            StartTheGame();
            Console.ReadKey(true);
            do {
                Round();
                Console.ReadKey(true);
                Console.Clear();
            } while (player1.CurrentCards > 0 && player2.CurrentCards > 0);
        }
        public void Round() {
            bool enoughCardsInHand;
            PlayCard(player1); // each player pick one random card from their hand
            PlayCard(player2);
            RoundResultAnnoucement();
            if (GetRoundWinner() != null) { // if cards are not equal, round ends and points are divided
                AddLoosersCardToWinnersHand(); // +card to winners hand
                RemoveLoosersCardFromLoosersHand(); // -card from loosers hand,
                CheckWinner();
                AnnoucCurrentScore(); // actual score is showed in console
            } else {
                enoughCardsInHand = CheckScore(); //checks whether each player has at least one more card
                if (enoughCardsInHand) {
                    SetCardsOnTheTable(player1); // add already played card to special list with played cards, for later addition to winners hand after the round will end
                    SetCardsOnTheTable(player2); // same for player2
                    ExtraRound(player1, player2, GetAvaliableNumberOfRounds()); // extra round after a draw in the previous round, number of showed cards in extra round can be set from 1 to 3, depends on how many cards do have the players left
                    ExtraRoundResultAnnoucementWith1ExtraRound();
                    if (GetRoundWinner() != null) { // if extra round have its winner, round ends and points and cards are divided between players
                        AddAllLoosersCardsOnTheTableToWinnersHand(); // all cards played in round and extra round are added to winners hand
                        RemoveAllLoosersCardOnTheTableFromLoosersHand(); // opposite for the looser
                        CheckWinner();
                        AnnoucCurrentScore();
                    } else {
                        enoughCardsInHand = CheckScore();
                        if (enoughCardsInHand) {
                            PlayCard(player1);
                            PlayCard(player2);
                            SetCardsOnTheTable(player1);
                            SetCardsOnTheTable(player2);
                            ExtraRoundResultAnnoucementWithDice();
                            if (GetRoundWinner() != null) {
                                AddAllLoosersCardsOnTheTableToWinnersHand();
                                RemoveAllLoosersCardOnTheTableFromLoosersHand();
                                CheckWinner();
                                AnnoucCurrentScore();
                            } else {
                                DiceDecision();
                                AddAllLoosersCardsOnTheTableToWinnersHand();
                                RemoveAllLoosersCardOnTheTableFromLoosersHand();
                                CheckWinner();
                                AnnoucCurrentScore();
                            }
                        } else {
                            CheckWinner();
                        }
                    }
                } else {
                    CheckWinner();
                }
            }
        }
//====================================================================================================================
//                                              BEFORE THE GAME
//====================================================================================================================
        public void DealTheCards(DeckOfCards deck) {
            List<Card> handP1 = new List<Card>(player1.CurrentCards);
            List<Card> handP2 = new List<Card>(player2.CurrentCards);
            for (int i = 0; i < SIZE_OF_PACKAGE; i++) {
                if (i % 2 == 0) {
                    handP1.Add(deckOfCards.Package[i]);
                } else {
                    handP2.Add(deckOfCards.Package[i]);
                }
            }
            player1.Hand = handP1;
            player2.Hand = handP2;
        }
        public void SetPlayersName(Player player) {
            bool OK;
            do {
                OK = true;
                message = $"Enter the name of player{player.Number}: ";
                Console.Write(message);
                try {
                    string? playerName = Console.ReadLine();
                    if (playerName is null || playerName == "")
                        throw new ArgumentNullException("The NAME field cannot be empty.");
                    player.Name = playerName;
                } catch (ArgumentNullException e) {
                    OK = false;
                    Console.WriteLine(e.Message);
                } catch (Exception e) {
                    OK = false;
                    Console.WriteLine(e.Message);
                }
            } while (!OK);
        }
        public void SetPlayersGameButton(Player player) {
            bool OK;
            do {
                OK = true;
                message = $"Press the key you would like to use to control player {player.Name}: ";
                Console.Write(message);
                try {
                    ConsoleKeyInfo gameButton = Console.ReadKey(true);
                    //if (player.Number == 2 && gameButton == player1.GameButton)
                    //    throw new FormatException($"\nPlayer {player.Name} cannot have the " +
                    //        $"SAME game button as {player1.Name}.\n");
                    player.GameButton = gameButton;
                    message = $"\nGame button for player {player.Name} is set to " +
                        $"\"{gameButton.KeyChar.ToString().ToUpper()}\"\n" +
                        $"Don't forget it!\n";
                    Console.WriteLine(message);
                } catch (InvalidOperationException e) {
                    OK = false;
                    Console.WriteLine(e.Message);
                } catch (FormatException e) {
                    OK = false;
                    Console.WriteLine(e.Message);
                } catch (Exception e) {
                    OK = false;
                    Console.WriteLine(e.Message);
                }
            } while (!OK);
        }
//====================================================================================================================
//                                                BASIC ROUND
//====================================================================================================================
        public void PlayCard(Player player) {
            Card card = player.Hand[player.ChooseRandomCardFromPack()];
            player.PlayedCardValue = card.Value;
            player.PlayedCardSuit = card.Suit;
            AnnoucPlayedCards(player);
        }
        public Player GetRoundWinner() {
            if (player1.PlayedCardValue > player2.PlayedCardValue) {
                return player1;
            } else if (player1.PlayedCardValue < player2.PlayedCardValue) {
                return player2;
            } else {
                return null; //NEMUZE VRACET NULL, OPRAVIT
            }
        }
        public Player GetRoundLooser() {
            if (player1.PlayedCardValue < player2.PlayedCardValue) {
                return player1;
            } else {
                return player2;
            }
        }
        public bool CheckScore() {
            if (player1.CurrentCards >= 1 && player2.CurrentCards >= 1)
                return true;
            else
                return false;
        }
        
        public int GetAvaliableNumberOfRounds() {
            if (player1.CurrentCards >= 3 && player2.CurrentCards >= 3)
                return 3;
            else if (player1.CurrentCards >= 2 && player2.CurrentCards >= 2)
                return 2;
            else if (player1.CurrentCards >= 1 && player2.CurrentCards >= 1)
                return 1;
            else
                return 0;
        }
//====================================================================================================================
//                                                EXTRA ROUND
//====================================================================================================================
        public void PlayCardInExtraRound(Player player) {
            Card card;
            bool OK;
            do {
                OK = true;
                card = player.Hand[player.ChooseRandomCardFromPack()];
                for (int i = 0; i < player.CardsOnTheTable.Count; i++) {
                    if (card.Value == player.Hand[i].Value && card.Suit == player.Hand[i].Suit)
                        OK = false;
                }
            } while (!OK);
            player.PlayedCardValue = card.Value;
            player.PlayedCardSuit = card.Suit;
            message = $"{player.GetReadableValueOfCard()}";
            Console.WriteLine(message);
        }
        public void ExtraRound(Player player1, Player player2, int rounds) {
            message = $"\n{player1.Name} plays...";
            Console.WriteLine(message);
            while (Console.ReadKey(true).Key != player1.GameButton.Key) {
            }
            for (int i = 0; i < rounds; i++) {
                PlayCardInExtraRound(player1);
                AnnoucPlayedCardsInExtraRound(player1);
                player1.CardsOnTheTable.Add(new Card(player1.PlayedCardValue, player1.PlayedCardSuit));
                while (Console.ReadKey(true).Key != player1.GameButton.Key) {
                }
            }
            message = $"\n{player2.Name} plays...";
            Console.WriteLine(message);
            while (Console.ReadKey(true).Key != player2.GameButton.Key) {
            }
            for (int i = 0; i < rounds; i++) {
                PlayCardInExtraRound(player2);
                AnnoucPlayedCardsInExtraRound(player2);
                player2.CardsOnTheTable.Add(new Card(player2.PlayedCardValue, player2.PlayedCardSuit));
                while (Console.ReadKey(true).Key != player2.GameButton.Key) {
                }
            }
        }
        public int ThrowTheDice(Player player) {
            return dice.ThrowDice();
        }
        public void DiceDecision() {
            int throw1; 
            int throw2;
            do {
                throw1 = ThrowTheDice(player1);
                AnnoucDiceThrow(player1, throw1);
                throw2 = ThrowTheDice(player2);
                AnnoucDiceThrow(player2, throw2);
                if (throw1 == throw2) {
                    Console.WriteLine("\nImpossible.\nBut real!\nOnce more.\n");
                } else {
                    if (throw1 > throw2)
                        Console.WriteLine($"\n{player1.Name} won the \"dice decision\" and he's a winner of this crazy round!");
                    else
                        Console.WriteLine($"\n{player2.Name} won the \"dice decision\" and he's a winner of this crazy round!");
                }
            } while (throw1 == throw2);
        }
//====================================================================================================================
//                                          ADDING / REMOVING CARDS
//====================================================================================================================
        public void AddLoosersCardToWinnersHand() {
            Player winner = GetRoundWinner();
            Player looser = GetRoundLooser();
            winner.Hand.Add(new Card(looser.PlayedCardValue, looser.PlayedCardSuit));
            winner.CurrentCards++;
        }
        public void AddAllLoosersCardsOnTheTableToWinnersHand() {
            Player winner = GetRoundWinner();
            Player looser = GetRoundLooser();
            //Player winner = player2;
            //Player looser = player1;
            for (int i = 0; i < looser.CardsOnTheTable.Count; i++) {
                winner.Hand.Add(new Card(looser.CardsOnTheTable[i].Value, looser.CardsOnTheTable[i].Suit));
            }
            winner.CurrentCards += looser.CardsOnTheTable.Count;
            winner.CardsOnTheTable.Clear();
        }
        public void RemoveLoosersCardFromLoosersHand() {
            Player looser = GetRoundLooser();
            foreach (Card c in looser.Hand) {
                if (looser.PlayedCardValue == c.Value && looser.PlayedCardSuit == c.Suit) {
                    looser.Hand.Remove(c);
                    looser.CurrentCards--;
                    break;
                }
            }
        }
        public void RemoveAllLoosersCardOnTheTableFromLoosersHand() {
            Player looser = GetRoundLooser();
            //Player looser = player1;
            for (int i = 0; i < looser.CardsOnTheTable.Count; i++) {
                for (int j = 0; j < looser.Hand.Count; j++) {
                    if (looser.CardsOnTheTable[i].Value == looser.Hand[j].Value
                        && looser.CardsOnTheTable[i].Suit == looser.Hand[j].Suit) {
                        looser.Hand.RemoveAt(j);
                    }
                }
            }
            looser.CurrentCards -= looser.CardsOnTheTable.Count;
            looser.CardsOnTheTable.Clear();
        }
        public void SetCardsOnTheTable(Player player) {
            player.CardsOnTheTable.Add(new Card(player.PlayedCardValue, player.PlayedCardSuit));
        }
//====================================================================================================================
//                                                 ANNOUNCEMENTS
//====================================================================================================================
        public void RoundResultAnnoucement() {
            if (GetRoundWinner() == player1)
                RoundWinner(player1);
            else if (GetRoundWinner() == player2)
                RoundWinner(player2);
            else
                RoundDraw();
        }
        public void AnnoucPlayedCards(Player player) {
            message = $"{player.Name} plays...";
            Console.WriteLine(message);
            while (Console.ReadKey(true).Key != player.GameButton.Key) {
            }
            message = $"{player.GetReadableValueOfCard()}\n";
            Console.WriteLine(message);
        }
        public void AnnoucPlayedCardsInExtraRound(Player player) {
            message = $"{player.GetReadableValueOfCard()}";
        }
        public void AnnoucCurrentScore() {
            message = $"\n<= Score =>\n{player1.Name} {player1.CurrentCards} : {player2.CurrentCards} {player2.Name}";
            Console.WriteLine(message);
        }
        public void RoundWinner(Player player) {
            message = $"{player.Name} won the round.";
            Console.WriteLine(message);
        }
        public void RoundDraw() {
            message = $"It's a draw!\nNow, each player will show " +
                $"{GetAvaliableNumberOfRounds()} more cards." +
                $"\nThe last one is the most important.";
            Console.WriteLine(message);
        }
        public void LastCard() {
            message = $"\nBut it seems that someone has no more cards.";
            Console.WriteLine(message);
        }
        public void CheckWinner() {
            if (player1.CurrentCards <= 0) {
                message = $"\nAnd it seems that {player2.Name} is also winner of the whole game.\n" +
                    $"CONGRATULATION!";
                Console.WriteLine(message);
            } else if (player2.CurrentCards <= 0) {
                message = $"\nAnd it seems that {player1.Name} is also winner of the whole game.\n" +
                    $"CONGRATULATION!";
                Console.WriteLine(message);
            }
        }
        public void AnnoucDiceDecision() {
            message = $"\nAre you kidding?!\nIs this even possible?\nOk, let the dice decide!\n";
            Console.WriteLine(message);
        }
        public void AnnoucDiceThrow(Player player, int number) {
            message = $"{player.Name} throws the dice and it's...";
            Console.Write(message);
            while (Console.ReadKey(true).Key != player.GameButton.Key) {
            }
            message = $"{number}!";
            Console.WriteLine(message);
        }
        public void ExtraRoundWinner(Player player) {
            message = $"\n{player.Name} won the EXTRA round and takes all played cards.";
            Console.WriteLine(message);
        }

        public void ExtraRoundDrawWithUpTo3Cards() {
            message = $"\nUnbelievable, there is another draw!" +
                $"\nEach player will show {GetAvaliableNumberOfRounds()} more card.\n";
            Console.WriteLine(message);
        }
        public void ExtraRoundDrawWith1Card() {
            message = $"\nUnbelievable, there is another draw!" +
                $"\nEach player will show 1 more final card.\n";
            Console.WriteLine(message);
        }
        public void ExtraRoundResultAnnoucementWith3ExtraRounds() {
            if (GetRoundWinner() == player1)
                ExtraRoundWinner(player1);
            else if (GetRoundWinner() == player2)
                ExtraRoundWinner(player2);
            else
                ExtraRoundDrawWithUpTo3Cards();
        }
        public void ExtraRoundResultAnnoucementWith1ExtraRound() {
            if (GetRoundWinner().Name == player1.Name)
                ExtraRoundWinner(player1);
            else if (GetRoundWinner().Name == player2.Name)
                ExtraRoundWinner(player2);
            else
                ExtraRoundDrawWith1Card();
        }
        public void ExtraRoundResultAnnoucementWithDice() {
            if (GetRoundWinner() == player1)
                ExtraRoundWinner(player1);
            else if (GetRoundWinner() == player2)
                ExtraRoundWinner(player2);
            else
                AnnoucDiceDecision();
        }
        public void StartTheGame() {
            message = $"Now, press ANY key and let's start playing VOJNA.\n";
            Console.WriteLine(message);
        }
        
    }
}

