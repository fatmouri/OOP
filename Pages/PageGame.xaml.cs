﻿using OOP4200_Tarneeb.Cards;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace OOP4200_Tarneeb
{
    /// <summary>
    /// Interaction logic for PageGame.xaml
    /// </summary>
    public partial class PageGame : Page
    {

        #region READ ME:

        /*
        
        TODO: List of things still needed:

         - Log & Stats (tough)

         - How To Play (easy)

         - Betting (tough)
            - Disable Cards that are clickable
            - Line 275, Add AI functionality (The three functions are made, but are empty)

         - Multiple Round Tarneeb to 31 Points(tough): right now it's single round, also idk if this is needed

         - Better AI (I [Kellen] plan on doing at least this)

         - Win determination not working. Playing a card that is not the played suit results in win even if it's not tarneeb.
            See: https://i.imgur.com/nDYQ7tu.png
            Player 4 started the hand with 9 of Diamonds. Tarneeb was Hearts. I played 5 of Spades and won.
            Another one (p4 started with 9 of Diamonds again): https://i.imgur.com/OInmmLN.png

        */
        

        #endregion

        #region Fields & Properties

        public Enums.Suit tarneeb;          // Tarneeb (trump card)
        public bool tarneebPlayed = false;  // Tarneeb played bool
        public Card firstCard;              // The first card played in the round
        public Card cardToBeat;             // The best card played in the round
        public int cardsDone = 0;           // # of remaining cards in the hand
        public Random rand = new Random();  // Random class object instantiation

        public bool playerDone = false;
        public bool roundDone = false;

        // The winner of the betting or the round. Winner places the first card of a new turn.
        // winner = 0 means new round (betting)
        public static int winner = 1;

        // Team round scores
        public int team1Score = 0;
        public int team2Score = 0;

        // Team total scores (game is to 31)
        public int team1Total = 0;
        public int team2Total = 0;

        // List of String that holds Tarneeb suit betting (1. Tarneeb Suit, 2. Bet number, 3. Player who betted the most) global variable
        public static List<string> tarneebSuit = new List<string>() { };

        // Team Colours + Misc Colours
        public SolidColorBrush team1Color = new SolidColorBrush(Color.FromRgb(51, 188, 255));
        public SolidColorBrush team2Color = new SolidColorBrush(Color.FromRgb(255, 90, 90));
        public SolidColorBrush scoreColor = new SolidColorBrush(Color.FromRgb(232, 193, 51));
        public SolidColorBrush greenColor = new SolidColorBrush(Color.FromRgb(61, 184, 93));
        public SolidColorBrush blackColor = new SolidColorBrush(Color.FromRgb(0, 0, 0));

        // Create a list of the player's cards Image controls from the PageGame.xaml form
        public List<Image> playerCardImages = new List<Image>();

        // List of all player's cards
        public List<Card> playerHand = new List<Card>();
        public List<Card> hand2 = new List<Card>();
        public List<Card> hand3 = new List<Card>();
        public List<Card> hand4 = new List<Card>();

        // Played Cards each turn
        public Card player1Card = new Card();
        public Card player2Card = new Card();
        public Card player3Card = new Card();
        public Card player4Card = new Card();

        // AI Difficulty setting (1 = easy, 2 = hard)
        // Not currently implemented
        public int computerDifficulty = 2;

        // Betting
        public int bettingPlayer = 0;
        public int player1Betting = 1;
        public int player2Betting = 2;
        public int player3Betting = 3;
        public int player4Betting = 4;
        public int bet = 7;
        public int minimumBet = 7;
        public int maximumBet = 13;
        public int startingPlayerBetting = 1;

        #endregion

        #region Game Initialization

        public PageGame()
        {
            InitializeComponent();
            CreateImageList();
            NewRound();
        }

        /// <summary>
        /// Initiates a new round of Tarneeb, including shuffling deck, dealing cards, resetting variables, etc..
        /// </summary>
        public void NewRound()
        {
            // Create a deck
            Deck deck = new Deck();

            // Shuffle the deck
            deck.Shuffle();

            // Pass out 13 cards to each
            playerHand = deck.Sort(deck.TakeCards(13));
            hand2 = deck.Sort(deck.TakeCards(13));
            hand3 = deck.Sort(deck.TakeCards(13));
            hand4 = deck.Sort(deck.TakeCards(13));

            // Create 4 Players each with their hand of 13 shuffled cards
            Player player1 = new Player(playerHand);
            Player player2 = new Player(hand2);
            Player player3 = new Player(hand3);
            Player player4 = new Player(hand4);

            // Display the player's cards
            DisplayCards(playerHand);

            // Reset round progression variables
            winner = 1;
            playerDone = false;
            roundDone = false;
            cardsDone = 0;

            // Hide the Next Round button
            btnNextRound.Visibility = Visibility.Hidden;
            btnNextRound.IsEnabled = false;

            // Hide the played cards
            playedCard1.Source = null;
            playedCard2.Source = null;
            playedCard3.Source = null;
            playedCard4.Source = null;

            // Reset the Tarneeb
            tarneebPlayed = false;
            tarneebImage.Source = null;

            // Reset the bet label
            lblBet1.Visibility = Visibility.Hidden;
            lblBet2.Visibility = Visibility.Hidden;
            lblBet3.Visibility = Visibility.Hidden;
            lblBet4.Visibility = Visibility.Hidden;
            lblBet5.Visibility = Visibility.Hidden;

            // Reset the team single round scores
            team1Score = 0;
            team2Score = 0;
            UpdateTeam1Score();
            UpdateTeam2Score();

            // Remove winner text
            lblWinner.Content = "";

            // Reset first card of round and the card to beat (AI logic)
            firstCard = null;
            cardToBeat = null;

            ////Assign Players to Teams (defunct?)
            //Team team1 = new Team(player1, player2);
            //Team team2 = new Team(player3, player4);

            //// Create a List of Players (defunct?)
            //List<Player> playerList = new List<Player> { player1, player2, player3, player4 };

            // Reset btnNextRound text
            btnNextRound.Content = "Next Round";

            // Starting Player bet
            if(startingPlayerBetting == player2Betting)
            {
                Player2Bet();
                Player3Bet();
                Player4Bet();
            }
            else if (startingPlayerBetting == player3Betting)
            {
                Player3Bet();
                Player4Bet();
            }
            else if (startingPlayerBetting == player4Betting)
            {
                Player4Bet();
            }
        }

        #endregion

        #region Card Display

        /// <summary>
        /// Create a list of the player's cards Image controls from the PageGame.xaml form
        /// </summary>
        public void CreateImageList()
        {
            // Add all Image controls to the list
            playerCardImages.Add(p01);
            playerCardImages.Add(p02);
            playerCardImages.Add(p03);
            playerCardImages.Add(p04);
            playerCardImages.Add(p05);
            playerCardImages.Add(p06);
            playerCardImages.Add(p07);
            playerCardImages.Add(p08);
            playerCardImages.Add(p09);
            playerCardImages.Add(p10);
            playerCardImages.Add(p11);
            playerCardImages.Add(p12);
            playerCardImages.Add(p13);
        }

        /// <summary>
        /// Displays the list of cards given
        /// </summary>
        /// <param name="hand">List of Cards to display</param>
        public void DisplayCards(List<Card> hand)
        {
            // Display player 1's card images in the Image controls
            for (int i = 0; i < playerHand.Count; i++)
            {
                playerCardImages[i].Source = Card.ToImage(hand[i]);
            }

            // If player's hand is less than 13, set remaining card image controls to null
            for (int i = playerHand.Count; i < 13; i++)
            {
                playerCardImages[i].Source = null;
            }
        }

        #endregion

        #region Betting

        private void BtnBetAddClick(object sender, RoutedEventArgs e)
        {
            if (bet < maximumBet)
            {
                bet += 1;
                lblBetting1.Content = bet.ToString();
                lblBetting2.Content = bet.ToString();
                lblBetting3.Content = bet.ToString();
                lblBetting4.Content = bet.ToString();
                lblBetting5.Content = bet.ToString();
            }
        }
        private void BtnBetSubClick(object sender, RoutedEventArgs e)
        {
            if (bet > minimumBet)
            {
                bet -= 1;
                lblBetting1.Content = bet.ToString();
                lblBetting2.Content = bet.ToString();
                lblBetting3.Content = bet.ToString();
                lblBetting4.Content = bet.ToString();
                lblBetting5.Content = bet.ToString();
            }
        }
        private void BtnPassClick(object sender, RoutedEventArgs e)
        {
            // AI betting functionality
            Player2Bet();
            Player3Bet();
            Player4Bet();

            // if one player made a bet
            if (bettingPlayer != 0)
            {
                HideBettingButtons();
            }
            // No bets were made, re-shuffle
            else
            {

            }

        }
        private void BtnBetClick(object sender, RoutedEventArgs e)
        {
            int originalBet = bet;
            bettingPlayer = player1Betting;

            // AI betting functionality
            Player2Bet();
            Player3Bet();
            Player4Bet();


            // Player1 won the bet. 
            if (originalBet == bet)
            {
                HideBettingButtons();
            }
        }
        
        // Betting AI functionality
        public void Player2Bet()
        {

        }
        public void Player3Bet()
        {

        }
        public void Player4Bet()
        {

        }


        public void HideBettingButtons()
        {
            // Hide the buttons
            btnBetAdd.Visibility = Visibility.Hidden;
            btnBetAdd.IsEnabled = false;
            btnBetSub.Visibility = Visibility.Hidden;
            btnBetSub.IsEnabled = false;
            btnBet.Visibility = Visibility.Hidden;
            btnBet.IsEnabled = false;
            btnPass.Visibility = Visibility.Hidden;
            btnPass.IsEnabled = false;
            lblBetting1.Visibility = Visibility.Hidden;
            lblBetting2.Visibility = Visibility.Hidden;
            lblBetting3.Visibility = Visibility.Hidden;
            lblBetting4.Visibility = Visibility.Hidden;
            lblBetting5.Visibility = Visibility.Hidden;

            lblBet1.Content = bet.ToString();
            lblBet2.Content = bet.ToString();
            lblBet3.Content = bet.ToString();
            lblBet4.Content = bet.ToString();
            lblBet5.Content = bet.ToString();
            lblBet2.Visibility = Visibility.Visible;
            lblBet3.Visibility = Visibility.Visible;
            lblBet4.Visibility = Visibility.Visible;
            lblBet5.Visibility = Visibility.Visible;
            // Change label colour based on team that won bet
            if (bettingPlayer == 1 || bettingPlayer == 3)
            {
                lblBet1.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#33BCFF"));
            }
            else if (bettingPlayer == 2 || bettingPlayer == 4)
            {
                lblBet1.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5A5A"));
            }
            lblBet1.Visibility = Visibility.Visible;
        }

        #endregion

        #region Tarneeb Selection

        /// <summary>
        /// Sets the tarneeb suit
        /// </summary>
        /// <param name="suit">The tarneeb suit</param>
        public void SetTarneeb(Enums.Suit suit)
        {
            // Set tarneeb if it hasn't already been set
            if (!tarneebPlayed)
            {
                tarneeb = suit;
                tarneebImage.Source = (ImageSource)new ImageSourceConverter().ConvertFrom(@"../../../Images/_" + suit + ".png");
                tarneebPlayed = true;
            }
        }

        #endregion

        #region AI Logic

        /// <summary>
        /// Completes the turns of the computer players 2-4 and starts the next round
        /// </summary>
        /// <returns></returns>
        public void ComputerTurnLogic()
        {
            // If the round isn't over, execute the computer turns
            if (!roundDone)
            {
                DoComputerTurns();
            }

            // If the round is completed, start the next round
            if (roundDone)
            {
                NextRound();
            }
        }

        /// <summary>
        /// Executes the computer player 2-4's turns
        /// </summary>
        public void DoComputerTurns()
        {
            // If player has made their turn already...
            if (playerDone)
            {
                // ...switch depending on winner (player who places first)
                switch (winner)
                {
                    case 1:
                        // Set first card and card to beat to the player's card played
                        firstCard = player1Card;
                        cardToBeat = player1Card;
                        // Play the AI turns and set the tarneeb
                        Player2Turn();
                        Player3Turn();
                        Player4Turn();
                        roundDone = true;
                        break;
                    case 2:
                        roundDone = true;
                        break;
                    case 3:
                        // Play the remaining AI turn
                        Player2Turn();
                        roundDone = true;
                        break;
                    case 4:
                        // Play the remaining AI turns
                        Player2Turn();
                        Player3Turn();
                        roundDone = true;
                        break;
                    default:
                        break;
                }
            }
            // If player has NOT made their turn already...
            else if (!playerDone)
            {
                // ...switch depending on winner (player who places first)
                switch (winner)
                {
                    case 2:
                        // Play the turns in order from player 2 and set Tarneeb / firstCard
                        Player2Turn();
                        firstCard = player2Card;
                        Player3Turn();
                        Player4Turn();
                        break;
                    case 3:
                        // Play the turns in order from player 3 and set Tarneeb / firstCard
                        Player3Turn();
                        firstCard = player3Card;
                        Player4Turn();
                        break;
                    case 4:
                        // Play the first turn and set Tarneeb / firstCard
                        Player4Turn();
                        firstCard = player4Card;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Returns the AI's choice of card to play with given hand
        /// </summary>
        /// <param name="hand">The computer's current hand</param>
        /// <returns></returns>
        public Card AIChooseCard(List<Card> hand)
        {
            // The card chosen by the AI to play
            Card chosenCard = new Card();

            // Create new list of cards that match the suit played, and a list of tarneebs
            List<Card> matchingList = new List<Card>();
            List<Card> tarneebList = new List<Card>();

            // Properties of card to beat
            int playedSuit;
            int playedNumber;

            // If a card has already been played this round
            if (cardToBeat != null)
            {
                // Set properties of card to beat
                playedSuit = (int)cardToBeat.Suit;
                playedNumber = (int)cardToBeat.CardNumber;
            }
            // If a card has not been played
            else
            {
                // Loop through the remaining cards and pick out the lowest value card
                for (int i = 0; i < hand.Count; i++)
                {
                    // If a card hasn't been chosen OR the current card's number is lower than
                    // the chosen card's number...
                    if (i == 0 || (int)hand[i].CardNumber < (int)chosenCard.CardNumber)
                    {
                        // ... choose the current card to play
                        chosenCard = hand[i];

                        // Set this card to the card to beat and the first card
                        cardToBeat = hand[i];
                    }
                }

                // Set properties of card to beat
                playedSuit = (int)cardToBeat.Suit;
                playedNumber = (int)cardToBeat.CardNumber;
            }

            // If the tarneeb has been selected...
            if (tarneebPlayed)
            {
                // ... make a list of tarneebs in the AI's hand
                for (int i = 0; i < hand.Count; i++)
                {
                    if (hand[i].Suit == tarneeb)
                    {
                        tarneebList.Add(hand[i]);
                    }
                }
            }

            // Make a list of cards that match the played suit in the AI's hand
            for (int i = 0; i < hand.Count; i++)
            {
                if ((int)hand[i].Suit == playedSuit)
                {
                    matchingList.Add(hand[i]);
                }
            }

            // Loop through the new list of matching cards...
            for (int i = 0; i < matchingList.Count; i++)
            {
                // If a card hasn't been chosen...
                if (i == 0)
                {
                    // ...choose the current card to play
                    chosenCard = matchingList[i];
                }

                // If the current card beats the card played AND the current card does NOT
                // beat the card played (so as to not waste a better card)...
                if (playedNumber < (int)matchingList[i].CardNumber
                    && (int)matchingList[i].CardNumber < (int)chosenCard.CardNumber)
                {
                    // ...choose the current card to play
                    chosenCard = matchingList[i];

                    // Since this card is better, set it to the new cardToBeat
                    cardToBeat = chosenCard;
                }
                // If the lower card doesn't beat the cardToBeat but the higher one does,
                // play the higher one
                else if (playedNumber < (int)matchingList[i].CardNumber)
                {
                    // ...choose the current card to play
                    chosenCard = matchingList[i];

                    // Since this card is better, set it to the new cardToBeat
                    cardToBeat = chosenCard;
                }
            }

            // If there are no cards with a matching suit...
            if (matchingList.Count == 0)
            {
                // ...loop through the remaining cards and pick out the lowest value card
                for (int i = 0; i < hand.Count; i++)
                {
                    // If a card hasn't been chosen OR the current card's number is lower than
                    // the chosen card's number...
                    if (i == 0 || (int)hand[i].CardNumber < (int)chosenCard.CardNumber)
                    {
                        // ... choose the current card to play
                        chosenCard = hand[i];
                    }
                }

                // If the card to beat is a KING or ACE and AI has at least one tarneeb in hand,
                // play the lowest value tarneeb
                if ((int)cardToBeat.CardNumber > 11 && tarneebList.Count > 0)
                {
                    // ...loop through the remaining cards and pick out the lowest value tarneeb
                    for (int i = 0; i < tarneebList.Count; i++)
                    {
                        // If a card hasn't been chosen OR the current card's number is lower than
                        // the chosen card's number...
                        if (i == 0 || (int)tarneebList[i].CardNumber < (int)chosenCard.CardNumber)
                        {
                            // ... choose the current card to play
                            chosenCard = tarneebList[i];

                            // The tarneeb played beats the non-tarneeb cardToBeat
                            cardToBeat = chosenCard;
                        }
                    }
                }
            }

            // Return the AI's card choice
            return chosenCard;
        }

        /// <summary>
        /// Turn logic for Player 2 AI
        /// </summary>
        public void Player2Turn()
        {
            Card chosenCard;
            chosenCard = AIChooseCard(hand2);
            player2Card = chosenCard;
            playedCard2.Source = Card.ToImage(chosenCard);
            hand2.RemoveAll(card => card.CardNumber == chosenCard.CardNumber && card.Suit == chosenCard.Suit);
        }

        /// <summary>
        /// Turn logic for Player 3 AI
        /// </summary>
        public void Player3Turn()
        {
            Card chosenCard;
            chosenCard = AIChooseCard(hand3);
            player3Card = chosenCard;
            playedCard3.Source = Card.ToImage(chosenCard);
            hand3.RemoveAll(card => card.CardNumber == chosenCard.CardNumber && card.Suit == chosenCard.Suit);
        }

        /// <summary>
        /// Turn logic for Player 4 AI
        /// </summary>
        public void Player4Turn()
        {
            Card chosenCard;
            chosenCard = AIChooseCard(hand4);
            player4Card = chosenCard;
            playedCard4.Source = Card.ToImage(chosenCard);
            hand4.RemoveAll(card => card.CardNumber == chosenCard.CardNumber && card.Suit == chosenCard.Suit);
        }

        /// <summary>
        /// Starts the next round
        /// </summary>
        public void NextRound()
        {
            // Determine winner of round
            EndOfRoundCleanup(tarneeb, player1Card, player2Card, player3Card, player4Card);

            // Increment number of cards done
            cardsDone += 1;

            // If there are more cards to play, continue the game
            if (cardsDone < 13)
            {
                // Show the Next Round button which starts the next round
                btnNextRound.Background = greenColor;
                btnNextRound.Foreground = blackColor;
                btnNextRound.Visibility = Visibility.Visible;
                btnNextRound.IsEnabled = true;
            }
            // If the cards are finished, prompt for new game
            else
            {
                // Show the New Game button which creates a new fresh PageGame page
                btnNextRound.Background = scoreColor;
                btnNextRound.Foreground = blackColor;
                btnNextRound.Content = "New Game?";
                btnNextRound.Visibility = Visibility.Visible;
                btnNextRound.IsEnabled = true;
            }
        }


        #endregion

        #region Button Functionality

        /// <summary>
        /// Sends user back to main menu (exits current game)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnGameBackClick(object sender, RoutedEventArgs e)
        {
            PageMenu menuPage = new PageMenu();
            NavigationService.Navigate(menuPage);
        }

        /// <summary>
        /// Starts the next round of Tarneeb
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnNextRoundClick(object sender, RoutedEventArgs e)
        {
            if (cardsDone < 13 && roundDone)
            {
                // Clear cards played
                player1Card = null;
                player2Card = null;
                player3Card = null;
                player4Card = null;
                firstCard = null;
                cardToBeat = null;

                // Clear respective card's images
                playedCard1.Source = null;
                playedCard2.Source = null;
                playedCard3.Source = null;
                playedCard4.Source = null;

                // Refresh card display with remaining cards in hand
                DisplayCards(playerHand);

                // Reset round completion bools to false
                playerDone = false;
                roundDone = false;

                // If a computer won, loop this function to complete the computer turns again
                if (winner > 1)
                {
                    ComputerTurnLogic();
                }

                // Remove winner text
                lblWinner.Content = "";

                // Hide the button again
                btnNextRound.Visibility = Visibility.Hidden;
                btnNextRound.IsEnabled = false;
            }
            // All 4 players are out of cards. Create new game
            else if (roundDone)
            {
                // Call NewRound function and pray it works
                NewRound();

                //// Create a new game from scratch (defunct?)
                //PageGame gamePage = new PageGame();
                //NavigationService.Navigate(gamePage);
            }


            // Next player starts betting
            startingPlayerBetting += 1;
            if (startingPlayerBetting > 4)
            {
                startingPlayerBetting = 1;
            }
        }


        #endregion

        #region Card Click Functionality

        /// <summary>
        /// Determines if the card clicked is playable based on first card played on the current round
        /// </summary>
        /// <param name="card">The card that was selected by the player</param>
        /// <returns></returns>
        private bool IsPlayable(Card card)
        {
            // If the variable "winner" is set to 0, don't allow cards to be clicked
            // In the betting phase, setting winner to 0 will prevent the player from playing
            // Set winner equal to the player # of the winner of the betting
            if (winner == 0)
            {
                return false;
            }
            
            // If the first card has already been placed...
            if (firstCard != null)
            {
                // ... check player's remaining hand for matching suits
                bool hasSuit = false;
                for (int i = 0; i < playerHand.Count; i++)
                {
                    // If there is a matching suit, set bool to true
                    if (playerHand[i].Suit == firstCard.Suit)
                    {
                        hasSuit = true;
                    }
                }

                // If the player doesn't have a matching suit, card is playable
                if (!hasSuit)
                {
                    return true;
                }
                // If the card the player is trying to play matches, card is playable
                else
                {
                    return card.Suit == firstCard.Suit;
                }
            }
            // If the card hasn't been played, card is playable
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Plays the card that is clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Card01MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If this slot has a card in it and there's no currently played card...
            if (p01.Source != null && playedCard1.Source == null && IsPlayable(playerHand[0]))
            {
                // ...play the card.
                playedCard1.Source = p01.Source;
                p01.Source = null;

                // Remove card from hand
                player1Card = playerHand[0];
                playerHand.RemoveAt(0);
                playerDone = true;
                SetTarneeb(player1Card.Suit);

                // Complete computer turns (async)
                ComputerTurnLogic();
            }
        }

        /// <summary>
        /// Plays the card that is clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Card02MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If this slot has a card in it and there's no currently played card...
            if (p02.Source != null && playedCard1.Source == null && IsPlayable(playerHand[1]))
            {
                // ...play the card.
                playedCard1.Source = p02.Source;
                p02.Source = null;

                // Remove card from hand
                player1Card = playerHand[1];
                playerHand.RemoveAt(1);
                playerDone = true;
                SetTarneeb(player1Card.Suit);

                // Complete computer turns (async)
                ComputerTurnLogic();
            }
        }

        /// <summary>
        /// Plays the card that is clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Card03MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If this slot has a card in it and there's no currently played card...
            if (p03.Source != null && playedCard1.Source == null && IsPlayable(playerHand[2]))
            {
                // ...play the card.
                playedCard1.Source = p03.Source;
                p03.Source = null;

                // Remove card from hand
                player1Card = playerHand[2];
                playerHand.RemoveAt(2);
                playerDone = true;
                SetTarneeb(player1Card.Suit);

                // Complete computer turns (async)
                ComputerTurnLogic();
            }
        }

        /// <summary>
        /// Plays the card that is clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Card04MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If this slot has a card in it and there's no currently played card...
            if (p04.Source != null && playedCard1.Source == null && IsPlayable(playerHand[3]))
            {
                // ...play the card.
                playedCard1.Source = p04.Source;
                p04.Source = null;


                // Remove card from hand
                player1Card = playerHand[3];
                playerHand.RemoveAt(3);
                playerDone = true;
                SetTarneeb(player1Card.Suit);

                // Complete computer turns (async)
                ComputerTurnLogic();
            }
        }

        /// <summary>
        /// Plays the card that is clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Card05MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If this slot has a card in it and there's no currently played card...
            if (p05.Source != null && playedCard1.Source == null && IsPlayable(playerHand[4]))
            {
                // ...play the card.
                playedCard1.Source = p05.Source;
                p05.Source = null;

                // Remove card from hand
                player1Card = playerHand[4];
                playerHand.RemoveAt(4);
                playerDone = true;
                SetTarneeb(player1Card.Suit);

                // Complete computer turns (async)
                ComputerTurnLogic();
            }
        }

        /// <summary>
        /// Plays the card that is clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Card06MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If this slot has a card in it and there's no currently played card...
            if (p06.Source != null && playedCard1.Source == null && IsPlayable(playerHand[5]))
            {
                // ...play the card.
                playedCard1.Source = p06.Source;
                p06.Source = null;

                // Remove card from hand
                player1Card = playerHand[5];
                playerHand.RemoveAt(5);
                playerDone = true;
                SetTarneeb(player1Card.Suit);

                // Complete computer turns (async)
                ComputerTurnLogic();
            }
        }

        /// <summary>
        /// Plays the card that is clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Card07MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If this slot has a card in it and there's no currently played card...
            if (p07.Source != null && playedCard1.Source == null && IsPlayable(playerHand[6]))
            {
                // ...play the card.
                playedCard1.Source = p07.Source;
                p07.Source = null;

                // Remove card from hand
                player1Card = playerHand[6];
                playerHand.RemoveAt(6);
                playerDone = true;
                SetTarneeb(player1Card.Suit);

                // Complete computer turns (async)
                ComputerTurnLogic();
            }
        }

        /// <summary>
        /// Plays the card that is clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Card08MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If this slot has a card in it and there's no currently played card...
            if (p08.Source != null && playedCard1.Source == null && IsPlayable(playerHand[7]))
            {
                // ...play the card.
                playedCard1.Source = p08.Source;
                p08.Source = null;

                // Remove card from hand
                player1Card = playerHand[7];
                playerHand.RemoveAt(7);
                playerDone = true;
                SetTarneeb(player1Card.Suit);

                // Complete computer turns (async)
                ComputerTurnLogic();
            }
        }

        /// <summary>
        /// Plays the card that is clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Card09MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If this slot has a card in it and there's no currently played card...
            if (p09.Source != null && playedCard1.Source == null && IsPlayable(playerHand[8]))
            {
                // ...play the card.
                playedCard1.Source = p09.Source;
                p09.Source = null;

                // Remove card from hand
                player1Card = playerHand[8];
                playerHand.RemoveAt(8);
                playerDone = true;
                SetTarneeb(player1Card.Suit);

                // Complete computer turns (async)
                ComputerTurnLogic();
            }
        }

        /// <summary>
        /// Plays the card that is clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Card10MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If this slot has a card in it and there's no currently played card...
            if (p10.Source != null && playedCard1.Source == null && IsPlayable(playerHand[9]))
            {
                // ...play the card.
                playedCard1.Source = p10.Source;
                p10.Source = null;

                // Remove card from hand
                player1Card = playerHand[9];
                playerHand.RemoveAt(9);
                playerDone = true;
                SetTarneeb(player1Card.Suit);

                // Complete computer turns (async)
                ComputerTurnLogic();
            }
        }

        /// <summary>
        /// Plays the card that is clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Card11MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If this slot has a card in it and there's no currently played card...
            if (p11.Source != null && playedCard1.Source == null && IsPlayable(playerHand[10]))
            {
                // ...play the card.
                playedCard1.Source = p11.Source;
                p11.Source = null;

                // Remove card from hand
                player1Card = playerHand[10];
                playerHand.RemoveAt(10);
                playerDone = true;
                SetTarneeb(player1Card.Suit);

                // Complete computer turns (async)
                ComputerTurnLogic();
            }
        }

        /// <summary>
        /// Plays the card that is clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Card12MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If this slot has a card in it and there's no currently played card...
            if (p12.Source != null && playedCard1.Source == null && IsPlayable(playerHand[11]))
            {
                // ...play the card.
                playedCard1.Source = p12.Source;
                p12.Source = null;

                // Remove card from hand
                player1Card = playerHand[11];
                playerHand.RemoveAt(11);
                playerDone = true;
                SetTarneeb(player1Card.Suit);

                // Complete computer turns (async)
                ComputerTurnLogic();
            }
        }

        /// <summary>
        /// Plays the card that is clicked on
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Card13MouseDown(object sender, MouseButtonEventArgs e)
        {
            // If this slot has a card in it and there's no currently played card...
            if (p13.Source != null && playedCard1.Source == null && IsPlayable(playerHand[12]))
            {
                // ...play the card.
                playedCard1.Source = p13.Source;
                p13.Source = null;

                // Remove card from hand
                player1Card = playerHand[12];
                playerHand.RemoveAt(12);
                playerDone = true;
                SetTarneeb(player1Card.Suit);

                // Complete computer turns (async)
                ComputerTurnLogic();
            }
        }

        #endregion

        #region Scoring & Win Determination

        /// <summary>
        /// Main Scoring & Win Determination function that performs the end of round functions
        /// </summary>
        /// <param name="tarneeb">The current Tarneeb</param>
        /// <param name="card1">Player 1's card played</param>
        /// <param name="card2">Player 2's card played</param>
        /// <param name="card3">Player 3's card played</param>
        /// <param name="card4">Player 4's card played</param>
        private void EndOfRoundCleanup(Cards.Enums.Suit tarneeb, Card card1, Card card2, Card card3, Card card4)
        {
            // Determines the winner of the round when passed parent function's parameters
            DetermineWinner(tarneeb, card1, card2, card3, card4);

            // Displays the winner of the round
            DisplayWinner();

            // Adds points to the winning team and updates scores accordingly
            HandleScores();
        }

        /// <summary>
        /// Determines the winner of the round
        /// </summary>
        /// <param name="tarneeb">The current Tarneeb</param>
        /// <param name="card1">Player 1's card played</param>
        /// <param name="card2">Player 2's card played</param>
        /// <param name="card3">Player 3's card played</param>
        /// <param name="card4">Player 4's card played</param>
        public void DetermineWinner(Enums.Suit tarneeb, Card card1, Card card2, Card card3, Card card4)
        {

            Enums.Suit suit;
            Card winningCard = null;

            // Set winningCard equal to the card that was played first in the round
            switch (winner)
            {
                case 1:
                    winningCard = card1;
                    break;
                case 2:
                    winningCard = card2;
                    break;
                case 3:
                    winningCard = card3;
                    break;
                case 4:
                    winningCard = card4;
                    break;
                default:
                    break;
            }



            // If any tarneebs were played, set the winning suit to the tarneeb suit
            if (card1.Suit == tarneeb)
            {
                suit = tarneeb;
            }
            else if (card2.Suit == tarneeb)
            {
                suit = tarneeb;
            }
            else if (card3.Suit == tarneeb)
            {
                suit = tarneeb;
            }
            else if (card4.Suit == tarneeb)
            {
                suit = tarneeb;
            }
            // Otherwise, the suit is the round suit (suit played by first player of round)
            else
            {
                suit = winningCard.Suit;
            }

            // Set winner to the player who played the highest card number of the winning suit
            if (card1.Suit == suit)
            {
                if (card1.CardNumber > winningCard.CardNumber)
                {
                    winningCard = card1;
                    winner = 1;
                }
            }

            if (card2.Suit == suit)
            {
                if (card2.CardNumber > winningCard.CardNumber)
                {
                    winningCard = card2;
                    winner = 2;
                }
            }

            if (card3.Suit == suit)
            {
                if (card3.CardNumber > winningCard.CardNumber)
                {
                    winningCard = card3;
                    winner = 3;
                }
            }

            if (card4.Suit == suit)
            {
                if (card4.CardNumber > winningCard.CardNumber)
                {
                    winner = 4;
                }
            }
        }

        /// <summary>
        /// Updates and displays winner labels based on the winner of the round
        /// </summary>
        public void DisplayWinner()
        {
            // Show the round winner label if the game isn't over
            if (cardsDone < 12)
            {
                switch (winner)
                {
                    case 1:
                        lblWinner.Content = "Player 1 Wins!";
                        lblWinner.Foreground = team1Color;
                        break;
                    case 2:
                        lblWinner.Content = "Player 2 Wins!";
                        lblWinner.Foreground = team2Color;
                        break;
                    case 3:
                        lblWinner.Content = "Player 3 Wins!";
                        lblWinner.Foreground = team1Color;
                        break;
                    case 4:
                        lblWinner.Content = "Player 4 Wins!";
                        lblWinner.Foreground = team2Color;
                        break;
                    default:
                        break;
                }
            }
            // If the game is over...
            else
            {
                // ...determine the winning team
                if (team1Score > team2Score)
                {
                    lblWinner.Content = "Team 1 Wins!";
                    lblWinner.Foreground = team1Color;
                }
                else
                {
                    lblWinner.Content = "Team 2 Wins!";
                    lblWinner.Foreground = team2Color;
                }
            }
        }

        /// <summary>
        /// Adds and updates scores based on the winner of the round
        /// </summary>
        public void HandleScores()
        {
            // Add score to whichever team won the round
            switch (winner)
            {
                case 1:
                    team1Score++;
                    break;
                case 2:
                    team2Score++;
                    break;
                case 3:
                    team1Score++;
                    break;
                case 4:
                    team2Score++;
                    break;
                default:
                    break;
            }

            // Update both scores
            UpdateTeam1Score();
            UpdateTeam2Score();
        }

        /// <summary>
        /// Updates the score labels for Team 1's score
        /// </summary>
        public void UpdateTeam1Score()
        {
            lblTeam1Score1.Content = team1Score;
            lblTeam1Score2.Content = team1Score;
            lblTeam1Score3.Content = team1Score;
            lblTeam1Score4.Content = team1Score;
            lblTeam1Score5.Content = team1Score;
        }

        /// <summary>
        /// Updates the score labels for Team 2's score
        /// </summary>
        public void UpdateTeam2Score()
        {
            lblTeam2Score1.Content = team2Score;
            lblTeam2Score2.Content = team2Score;
            lblTeam2Score3.Content = team2Score;
            lblTeam2Score4.Content = team2Score;
            lblTeam2Score5.Content = team2Score;
        }

        #endregion

    }
}
