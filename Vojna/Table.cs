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
            WelcomeScreen();
            SetPlayersName(player1);
            SetPlayersGameButton(player1);
            SetPlayersName(player2);
            SetPlayersGameButton(player2);
            DealTheCards(deckOfCards);
            StartTheGame();
            Console.ReadKey(true);
            Console.Clear();
            do {
                Round();
                Console.ReadKey(true);
                Console.Clear();
            } while (player1.CurrentCards > 0 && player2.CurrentCards > 0);
        }
        public void Round() {
            PlayCard(player1); // each player pick one random card from their hand
            PlayCard(player2);
            RoundResultAnnoucement();
            if (!CheckDraw()) { // if cards are not equal, round ends and points are divided
                AddLoosersCardToWinnersHand(); // +card to winners hand
                RemoveLoosersCardFromLoosersHand(); // -card from loosers hand,
                CheckWinner();
                AnnoucCurrentScore(); // actual score is showed in console
            } else {
                if (CheckScore()) {  //checks whether each player has at least one more card
                    SetCardsOnTheTable(player1); // add already played card to special list with played cards, for later addition to winners hand after the round will end
                    SetCardsOnTheTable(player2); // same for player2
                    ExtraRound(player1, player2, GetAvaliableNumberOfRounds()); // extra round after a draw in the previous round, number of showed cards in extra round can be set from 1 to 3, depends on how many cards do have the players left
                    ExtraRoundResultAnnoucementWith1ExtraRound();
                    if (!CheckDraw()) { // if extra round have its winner, round ends and points and cards are divided between players
                        AddAllLoosersCardsOnTheTableToWinnersHand(); // all cards played in round and extra round are added to winners hand
                        RemoveAllLoosersCardOnTheTableFromLoosersHand(); // opposite for the looser
                        CheckWinner(); // if score of anyone is 0, winner is announced
                        AnnoucCurrentScore();
                    } else {
                        if (CheckScore()) { //checks whether each player has at least one more card
                            PlayCard(player1); // each player pick one random card from their hand
                            PlayCard(player2);
                            SetCardsOnTheTable(player1); // add already played card to special list with played cards, for later addition to winners hand after the round will end
                            SetCardsOnTheTable(player2);
                            ExtraRoundResultAnnoucementWithDice();
                            if (!CheckDraw()) {
                                AddAllLoosersCardsOnTheTableToWinnersHand(); // all cards played in round and extra round are added to winners hand
                                RemoveAllLoosersCardOnTheTableFromLoosersHand();
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
        /// <summary>
        /// Divides deck of 32 cards between two players and gives them both of 16 unique cards
        /// </summary>
        /// <param name="deck"></param>
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
        /// <summary>
        /// Sets players name. Name cannot be an empty string.
        /// </summary>
        /// <param name="player"></param>
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
        /// <summary>
        /// Sets players game button which is used to move one step forward when its players turn.
        /// Button has to be different for both players.
        /// </summary>
        /// <param name="player"></param>
        public void SetPlayersGameButton(Player player) {
            bool OK;
            do {
                OK = true;
                message = $"Press the key you would like to use to control player {player.Name}: ";
                Console.Write(message);
                try {
                    ConsoleKeyInfo gameButton = Console.ReadKey(true);
                    if (player.Number == 2 && gameButton == player1.GameButton)
                        throw new FormatException($"\nPlayer {player.Name} cannot have the " +
                            $"SAME game button as {player1.Name}.\n");
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
        /// <summary>
        /// Picks one random card from current players pack. Cards value and suit are stored.
        /// </summary>
        /// <param name="player"></param>
        public void PlayCard(Player player) {
            Card card = player.Hand[player.ChooseRandomCardFromPack()];
            player.PlayedCardValue = card.Value;
            player.PlayedCardSuit = card.Suit;
            AnnoucPlayedCards(player);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>returns the winner of the round</returns>
        public Player GetRoundWinner() {
            if (player1.PlayedCardValue > player2.PlayedCardValue) {
                return player1;
            } else {
                return player2;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>returns the looser of the round</returns>
        public Player GetRoundLooser() {
            if (player1.PlayedCardValue < player2.PlayedCardValue) {
                return player1;
            } else {
                return player2;
            }
        }
        /// <summary>
        /// checks round result
        /// </summary>
        /// <returns>if round ends in draw, returns true, else returns false</returns>
        public bool CheckDraw() {
            return player1.PlayedCardValue == player2.PlayedCardValue ? true : false;
        }
        public bool CheckScore() {
            if (player1.CurrentCards >= 1 && player2.CurrentCards >= 1)
                return true;
            else
                return false;
        }
        /// <summary>
        /// supportive method for decision how many cards should be showed in extra round when its draw
        /// </summary>
        /// <returns>returns 3 if both players have at least 3 cards, 2 if both players have at least 2 cards and so on</returns>
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
        /// <summary>
        /// chooses unique cards from players hand that can be played including played cards in current round  
        /// </summary>
        /// <param name="player"></param>
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
        /// <summary>
        /// in case of draw extra round runs and players show number of cards depending on round parameter
        /// </summary>
        /// <param name="player1"></param>
        /// <param name="player2"></param>
        /// <param name="rounds"></param>
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <returns>return int in scope of dice</returns>
        public int ThrowTheDice(Player player) {
            return dice.ThrowDice();
        }
        /// <summary>
        /// both players throw dice as long as one of them is winner or do not have any card
        /// </summary>
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
        /// <summary>
        /// card that played player who lost the round is moved into winners pack
        /// </summary>
        public void AddLoosersCardToWinnersHand() {
            Player winner = GetRoundWinner();
            Player looser = GetRoundLooser();
            winner.Hand.Add(new Card(looser.PlayedCardValue, looser.PlayedCardSuit));
            winner.CurrentCards++;
        }
        /// <summary>
        /// cards that played player who lost the round, includes the extra round, is moved into winners pack
        /// </summary>
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
        /// <summary>
        /// card that played player who lost the round is removed from his pack
        /// </summary>
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
        /// <summary>
        /// cards that played player who lost the round is removed from his pack, includes cards in extra round
        /// </summary>
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
        /// <summary>
        /// in case of extra round, card from first round is add into the list of cards that are played for further use
        /// </summary>
        /// <param name="player"></param>
        public void SetCardsOnTheTable(Player player) {
            player.CardsOnTheTable.Add(new Card(player.PlayedCardValue, player.PlayedCardSuit));
        }
//====================================================================================================================
//                                                 ANNOUNCEMENTS
//====================================================================================================================
        public void RoundResultAnnoucement() {
            if (CheckDraw())
                RoundDraw();
            else {
                if (GetRoundWinner() == player1)
                    RoundWinner(player1);
                else if (GetRoundWinner() == player2)
                    RoundWinner(player2);
            }
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
            if (CheckDraw())
                ExtraRoundDrawWithUpTo3Cards();
            else {
                if (GetRoundWinner() == player1)
                    RoundWinner(player1);
                else if (GetRoundWinner() == player2)
                    RoundWinner(player2);
            }
        }
        public void ExtraRoundResultAnnoucementWith1ExtraRound() {
            if (CheckDraw())
                ExtraRoundDrawWith1Card();
            else {
                if (GetRoundWinner() == player1)
                    RoundWinner(player1);
                else if (GetRoundWinner() == player2)
                    RoundWinner(player2);
            }
        }
        public void ExtraRoundResultAnnoucementWithDice() {
            if (CheckDraw())
                AnnoucDiceDecision();
            else {
                if (GetRoundWinner() == player1)
                    RoundWinner(player1);
                else if (GetRoundWinner() == player2)
                    RoundWinner(player2);
            }
        }
        public void StartTheGame() {
            message = $"Now, press ANY key and let's start playing VOJNA.\n";
            Console.WriteLine(message);
        }
        public void WelcomeScreen() {
            message = @"
_    _ _____ _     _____ ________  ________    ___ _____   _____ _   _ _____ _____   _____ ___ ______ _     _____ 
| |  | |  ___| |   /  __ |  _  |  \/  |  ___|  / _ |_   _| |_   _| | | |_   _/  ___| |_   _/ _ \| ___ | |   |  ___|
| |  | | |__ | |   | /  \| | | | .  . | |__   / /_\ \| |     | | | |_| | | | \ `--.    | |/ /_\ | |_/ | |   | |__  
| |/\| |  __|| |   | |   | | | | |\/| |  __|  |  _  || |     | | |  _  | | |  `--. \   | ||  _  | ___ | |   |  __| 
\  /\  | |___| |___| \__/\ \_/ | |  | | |___  | | | || |     | | | | | |_| |_/\__/ /   | || | | | |_/ | |___| |___ 
 \/  \/\____/\_____/\____/\___/\_|  |_\____/  \_| |_/\_/     \_/ \_| |_/\___/\____/    \_/\_| |_\____/\_____\____/ 
                                                                                                                   
                                                                                                                   
  ___ _____   _____ _   _ _____ _____   _____ ___ ______ _     _____   _    _ _____  ______ _      _____   __      
 / _| _   _| |_   _| | | |_   _/  ___| |_   _/ _ \| ___ | |   |  ___| | |  | |  ___| | ___ | |    / _ \ \ / /      
/ /_\ \| |     | | | |_| | | | \ `--.    | |/ /_\ | |_/ | |   | |__   | |  | | |__   | |_/ | |   / /_\ \ V /       
|  _  || |     | | |  _  | | |  `--. \   | ||  _  | ___ | |   |  __|  | |/\| |  __|  |  __/| |   |  _  |\ /        
| | | || |     | | | | | |_| |_/\__/ /   | || | | | |_/ | |___| |___  \  /\  | |___  | |   | |___| | | || |        
\_| |_/\_/     \_/ \_| |_/\___/\____/    \_/\_| |_\____/\_____\____/   \/  \/\____/  \_|   \_____\_| |_/\_/                                                                                                                 
            ";
            Console.WriteLine(message);
            message = @"
.----------------.   .----------------.   .----------------.   .-----------------.  .----------------. 
| .--------------. | | .--------------. | | .--------------. | | .--------------. | | .--------------. |
| | ____   ____  | | | |     ____     | | | |     _____    | | | | ____  _____  | | | |      __      | |
| ||_  _| |_  _| | | | |   .'    `.   | | | |    |_   _|   | | | ||_   \|_   _| | | | |     /  \     | |
| |  \ \   / /   | | | |  /  .--.  \  | | | |      | |     | | | |  |   \ | |   | | | |    / /\ \    | |
| |   \ \ / /    | | | |  | |    | |  | | | |   _  | |     | | | |  | |\ \| |   | | | |   / ____ \   | |
| |    \ ' /     | | | |  \  `--'  /  | | | |  | |_' |     | | | | _| |_\   |_  | | | | _/ /    \ \_ | |
| |     \_/      | | | |   `.____.'   | | | |  `.___.'     | | | ||_____|\____| | | | ||____|  |____|| |
| |              | | | |              | | | |              | | | |              | | | |              | |
| '--------------' | | '--------------' | | '--------------' | | '--------------' | | '--------------' |
'----------------'   '----------------'   '----------------'   '----------------'   '----------------' 
            ";
            Console.WriteLine(message);
            message = @"                                    (press any key to continue...)";
            Console.WriteLine(message);
            Console.ReadKey();
            Console.Clear();
        }
    }
}

