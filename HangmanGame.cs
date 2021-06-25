﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hangman
{
    public class HangmanGame
    {
        private const int MaxLives = 5;
        private const int WordPenalty = 2;
        private const int LetterPenalty = 1;
        private const int ShowHintWhenBelowThisManyLives = 2;
        private const char MaskCharacter = '_';
        private const char LivesCharacter = '♥';

        private Random random = new Random();

        private List<(string, string)> CountriesAndCities;

        private string CurrentCity;
        private string CurrentCountry;
        private List<bool> CityMask;
        private List<char> NotInWordList;
        private int Lives;
        private bool PlayerWon;

        private List<(string, string)> LoadCountriesAndCities()
        {
            return new List<(string, string)> { ("Poland", "Warsaw") };
            // TODO: actually load data from the file
        }

        public void Play()
        {
            // Early exit if there are no cities to choose from - might need another action
            CountriesAndCities = LoadCountriesAndCities();
            if (CountriesAndCities.Count == 0)
            {
                Console.WriteLine("Sorry, there are no city names to guess.");
                return;
            }

            // Play the game until the player decides to quit
            while (true)
            {
                ResetTheGame();
                while (Lives > 0 && !PlayerWon)
                {
                    bool guessTheWholeWord = GetInputType();
                    if (guessTheWholeWord)
                    {
                        JudgeWordInput(GetWordInput());
                    } else
                    {
                        JudgeLetterInput(GetLetterInput());
                    }
                }
                if (PlayerWon)
                {
                    DisplaySuccess();
                } else
                {
                    DisplayFailure();
                }
                if (PlayerWantsToExit())
                {
                    break;
                }
            }
        }

        private void DisplaySuccess()
        {
            Console.Clear();
            Console.WriteLine("Congratulations! You won!");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey(true);
        }
        private void DisplayFailure()
        {
            Console.Clear();
            Console.WriteLine($"You lost! The city was {CurrentCity}");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey(true);
        }

        private bool PlayerWantsToExit()
        {
            string clarification = "Please press 'Y' or 'N'.";
            bool displayClarification = false;
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Play again? (Y/N)");
                if (displayClarification)
                {
                    Console.WriteLine(clarification);
                }

                char pressed = Console.ReadKey().KeyChar;
                if (char.ToUpper(pressed) == 'Y')
                {
                    return false;
                }
                else if (char.ToUpper(pressed) == 'N')
                {
                    return true;
                }
                else
                {
                    displayClarification = true;
                }
            }
        }

        private void JudgeLetterInput(char chosenLetter)
        {
            char chosenUpperLetter = char.ToUpper(chosenLetter);

            if (CurrentCity.ToUpper().Contains(chosenUpperLetter))
            {
                for (int i=0; i<CurrentCity.Length; i++)
                {
                    if (char.ToUpper(CurrentCity[i]) == chosenUpperLetter)
                    {
                        CityMask[i] = false;
                    }
                }

            } else
            {
                if (NotInWordList.Contains(chosenUpperLetter) == false)
                {
                    NotInWordList.Add(chosenUpperLetter);
                }
                Lives -= LetterPenalty;
            }
        }

        private void JudgeWordInput(string chosenWord)
        {
            if (chosenWord.ToUpper() == CurrentCity.ToUpper())
            {
                CityMask = Enumerable.Repeat(false, CurrentCity.Length).ToList();
                PlayerWon = true;
            } else
            {
                Lives -= WordPenalty;
            }
        }

        private string GetWordInput()
        {
            string input;
            string clarification = "Please write at least one letter.";
            bool displayClarification = false;
            while (true)
            {
                Display();
                Console.WriteLine("Guess the whole word:");
                if (displayClarification)
                {
                    Console.WriteLine(clarification);
                }
                input = Console.ReadLine();
                if (input.Length == 0)
                {
                    displayClarification = true;
                }
                else
                {
                    return input;
                }
            }
        }

        private char GetLetterInput()
        {
            string input;
            string clarification = "Please write one letter.";
            bool displayClarification = false;
            while (true)
            {
                Display();
                Console.WriteLine("Choose a letter:");
                if (displayClarification)
                {
                    Console.WriteLine(clarification);
                }
                input = Console.ReadLine();
                if (input.Length != 1)
                {
                    displayClarification = true;
                } else
                {
                    return input[0];
                }
            }
        }

        private bool GetInputType()
        {
            string clarification = "Please press 'L' or 'W'.";
            bool displayClarification = false;
            while (true)
            {
                Display();
                Console.WriteLine("Do you want to guess a [L]etter or the whole [W]ord?");
                if (displayClarification)
                {
                    Console.WriteLine(clarification);
                }

                char pressed = Console.ReadKey().KeyChar;
                if (char.ToUpper(pressed) == 'L')
                {
                    return false;
                } else if (char.ToUpper(pressed) == 'W')
                {
                    return true;
                } else
                {
                    displayClarification = true;
                }
            }
        }

        private void Display()
        {
            Console.Clear();
            DisplayMaskedCity();
            DisplayHealth();
            DisplayNotInWordList();
            DisplayHint();
        }

        private void DisplayHint()
        {
            if (Lives < ShowHintWhenBelowThisManyLives)
            {
                Console.WriteLine($"Hint: it's the capital of {CurrentCountry}");
            } else
            {
                Console.WriteLine();
            }
        }

        private void DisplayNotInWordList()
        {
            StringBuilder notInWordListBuilder = new StringBuilder("Letters not in the word:");
            for (int i=0; i<NotInWordList.Count; i++)
            {
                notInWordListBuilder.Append($" {NotInWordList[i]}");
            }
            Console.WriteLine(notInWordListBuilder.ToString());
        }

        private void DisplayHealth()
        {
            StringBuilder livesStringBuilder = new StringBuilder("Lives:");
            for (int i=0; i<Lives; i++)
            {
                livesStringBuilder.Append($" {LivesCharacter}");
            }
            Console.WriteLine(livesStringBuilder.ToString());
        }

        private void DisplayMaskedCity()
        {
            Console.WriteLine("Guess the capital city:");
            StringBuilder hiddenCityName = new StringBuilder(CurrentCity.Length * 2);
            for (int i=0; i<CurrentCity.Length; i++)
            {
                if (CityMask[i])
                {
                    hiddenCityName.Append(MaskCharacter);
                }
                else
                {
                    hiddenCityName.Append(CurrentCity[i]);
                }
                hiddenCityName.Append(" ");
            }
            Console.WriteLine(hiddenCityName.ToString());
        }

        private void ResetTheGame()
        {
            int cityIndex = random.Next(CountriesAndCities.Count);
            (CurrentCountry, CurrentCity) = CountriesAndCities[cityIndex];

            Lives = MaxLives;
            PlayerWon = false;
            CityMask = Enumerable.Repeat(true, CurrentCity.Length).ToList();
            NotInWordList = new List<char>();
        }
    }
}