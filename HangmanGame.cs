using System;
using System.IO;
using System.Collections.Generic;
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

        private const string FileNameCountriesAndCapitals = ".\\countries_and_capitals.txt";

        private readonly Random random = new Random();

        private List<(string, string)> CountriesAndCapitals;

        private string CurrentCapital;
        private string CurrentCountry;
        private bool[] CityMask;
        private List<char> NotInWordList;
        private int Lives;
        private bool PlayerWon;

        private int GuessCount;

        private List<(string, string)> LoadCountriesAndCapitals()
        {
            string line;
            string corruptLineMessage = "Found a corrupt line in the input file, skipping.";
            List<(string, string)> outputList = new List<(string, string)>();

            try
            {
                using (StreamReader inputStream = new StreamReader(FileNameCountriesAndCapitals))
                {
                    while ((line = inputStream.ReadLine()) != null)
                    {
                        string[] items = line.Split('|');
                        if (items.Length != 2)
                        {
                            Console.WriteLine(corruptLineMessage);
                        }
                        else
                        {
                            string country = items[0].Trim();
                            string capital = items[1].Trim();
                            if (country.Length == 0 || capital.Length == 0)
                            {
                                Console.WriteLine(corruptLineMessage);
                            }
                            else
                            {
                                outputList.Add((country, capital));
                            }
                        }
                    }

                }
                return outputList;

            }
            catch (IOException)
            {
                Console.WriteLine("Failed to open the file with countries and capitals.");
                return new List<(string, string)>();
            }

        }

        public void Play()
        {
            // Early exit if there are no cities to choose from - might need another action
            CountriesAndCapitals = LoadCountriesAndCapitals();
            if (CountriesAndCapitals.Count == 0)
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
                    }
                    else
                    {
                        JudgeLetterInput(GetLetterInput());
                    }
                    GuessCount++;
                }
                if (PlayerWon)
                {
                    DisplaySuccess();
                }
                else
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
            Console.WriteLine($"Congratulations! You won! The capital was {CurrentCapital}.");
            Console.WriteLine($"It took you {GuessCount} guesses.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }
        private void DisplayFailure()
        {
            Console.Clear();
            Console.WriteLine($"You lost! The capital was {CurrentCapital}.");
            Console.WriteLine("Press any key to continue...");
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

            if (CurrentCapital.ToUpper().Contains(chosenUpperLetter))
            {
                for (int i=0; i<CurrentCapital.Length; i++)
                {
                    if (char.ToUpper(CurrentCapital[i]) == chosenUpperLetter)
                    {
                        CityMask[i] = false;
                    }
                }
                if (AllLettersDiscovered())
                {
                    PlayerWon = true;
                }

            }
            else
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
            if (chosenWord.ToUpper() == CurrentCapital.ToUpper())
            {
                CityMask = MakeMaskFromString(CurrentCapital);
                PlayerWon = true;
            }
            else
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
                }
                else
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
                }
                else if (char.ToUpper(pressed) == 'W')
                {
                    return true;
                }
                else
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
            }
            else
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
            StringBuilder hiddenCityName = new StringBuilder(CurrentCapital.Length * 2);
            for (int i=0; i<CurrentCapital.Length; i++)
            {
                if (CityMask[i])
                {
                    hiddenCityName.Append(MaskCharacter);
                }
                else
                {
                    hiddenCityName.Append(CurrentCapital[i]);
                }
                hiddenCityName.Append(" ");
            }
            Console.WriteLine(hiddenCityName.ToString());
        }

        private void ResetTheGame()
        {
            int cityIndex = random.Next(CountriesAndCapitals.Count);
            (CurrentCountry, CurrentCapital) = CountriesAndCapitals[cityIndex];

            Lives = MaxLives;
            PlayerWon = false;
            CityMask = MakeMaskFromString(CurrentCapital);
            NotInWordList = new List<char>();
            GuessCount = 0;
        }

        private bool[] MakeMaskFromString(string capital)
        {
            bool[] mask = new bool[capital.Length];
            for (int i = 0; i < capital.Length; i++)
            {
                mask[i] = (capital[i] != ' ');
            }
            return mask;
        }

        private bool AllLettersDiscovered()
        {
            foreach (bool place in CityMask)
            {
                if (place)
                {
                    return false;
                }
            }
            return true;
        }
    }
}