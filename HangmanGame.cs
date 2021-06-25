using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Linq;
using System.IO;

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
        private const string FileNameHighscores = ".\\highscores.txt";

        private readonly Random random = new Random();

        private List<(string, string)> CountriesAndCapitals;
        private List<HighscoreRecord> Highscores;

        private string CurrentCapital;
        private string CurrentCountry;
        private bool[] CityMask;
        private List<char> NotInWordList;
        private int Lives;
        private bool PlayerWon;

        private int GuessCount;
        private readonly Stopwatch Timer = new Stopwatch();

        public void Play()
        {
            // Early exit if there are no cities to choose from - might need another action
            LoadCountriesAndCapitals();
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
                Timer.Stop();

                if (PlayerWon)
                {
                    DisplaySuccess();
                    UpdateHighscores();
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

        private void LoadCountriesAndCapitals()
        {
            int corruptRecords = 0;
            CountriesAndCapitals = new List<(string, string)>();

            List<string[]> records = new FileReader().Read(FileNameCountriesAndCapitals);    
            
            foreach (string[] line in records)
            {
                if (line.Length != 2)
                {
                    corruptRecords++;
                }
                else
                {
                    string country = line[0].Trim();
                    string capital = line[1].Trim();
                    if (country.Length == 0 || capital.Length == 0)
                    {
                        corruptRecords++;
                    }
                    else
                    {
                        CountriesAndCapitals.Add((country, capital));
                    }
                }
            }
            if (corruptRecords > 0)
            {
                Console.WriteLine($"Found {corruptRecords} corrupt record(s) in the input file.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }
        }

        private void LoadHighscores()
        {
            int corruptRecords = 0;
            Highscores = new List<HighscoreRecord>();

            List<string[]> records = new FileReader().Read(FileNameHighscores);

            foreach (string[] line in records)
            {
                if (line.Length != 5)
                {
                    corruptRecords++;
                }
                else
                {
                    int tries;
                    if (int.TryParse(line[3].Trim(), out tries) == false)
                    {
                        corruptRecords++;
                        break;
                    }
                    HighscoreRecord highscore = new HighscoreRecord
                    {
                        Name = line[0].Trim(),
                        Date = line[1].Trim(),
                        GuessingTime = line[2].Trim(),
                        GuessingTries = tries,
                        GuessedWord = line[4].Trim()
                    };
                    Highscores.Add(highscore);
                }
            }
            if (corruptRecords > 0)
            {
                Console.WriteLine($"Found {corruptRecords} corrupt record(s) in the highscores file.");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey(true);
            }

        }

        private void SaveHighscores()
        {
            try
            {
                using (StreamWriter outputStream = new StreamWriter(FileNameHighscores))
                {
                    for (int i=0; i<Math.Min(10, Highscores.Count); i++)
                    {
                        outputStream.WriteLine(Highscores[i].ToString());
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine($"Failed to write highscores to a file. Reason: {e.Message}");
            }
        }

        private void UpdateHighscores()
        {
            string playerName = GetPlayerName();
            HighscoreRecord currentRecord = new HighscoreRecord
            {
                Name = playerName,
                Date = DateTime.Today.ToString("d"),
                GuessedWord = CurrentCapital,
                GuessingTime = Timer.Elapsed.TotalSeconds.ToString("0.00"),
                GuessingTries = GuessCount
            };
            LoadHighscores();
            Highscores.Add(currentRecord);
            Highscores = Highscores.OrderBy(x => x.GuessingTries)
                                    .ThenBy(x => x.GuessingTime)
                                    .ThenBy(x => x.Date)
                                    .ToList();
            DisplayHighscores();
            SaveHighscores();
        }

        private void DisplayHighscores()
        {
            Console.Clear();
            Console.WriteLine("Highscores:");
            Console.WriteLine("Player | Date | Guessing Time | Guessing Tries | Capital");
            foreach (var highscore in Highscores)
            {
                Console.WriteLine(highscore.ToString());
            }
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey(true);
        }

        private void DisplaySuccess()
        {
            Console.Clear();
            Console.WriteLine($"Congratulations! You won! The capital was {CurrentCapital}.");
            Console.WriteLine($"It took you {GuessCount} guesses and {Timer.Elapsed.TotalSeconds:0.00} seconds.");
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
                if (Helpers.IsMaskClear(CityMask))
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
                CityMask = Helpers.MakeMaskFromString(CurrentCapital);
                PlayerWon = true;
            }
            else
            {
                Lives -= WordPenalty;
            }
        }

        private string GetPlayerName()
        {
            string input;
            string clarification = "Please write at least one letter.";
            bool displayClarification = false;
            while (true)
            {
                Console.WriteLine("Write your name:");
                if (displayClarification)
                {
                    Console.WriteLine(clarification);
                }
                input = Console.ReadLine();
                if (input.Length == 0 || input.Trim().Length == 0)
                {
                    displayClarification = true;
                }
                else
                {
                    return input;
                }
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
                if (input.Length == 0 || input.Trim().Length == 0)
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
            Console.WriteLine("\n" + hiddenCityName.ToString() + "\n");
        }

        private void ResetTheGame()
        {
            int cityIndex = random.Next(CountriesAndCapitals.Count);
            (CurrentCountry, CurrentCapital) = CountriesAndCapitals[cityIndex];

            Lives = MaxLives;
            PlayerWon = false;
            CityMask = Helpers.MakeMaskFromString(CurrentCapital);
            NotInWordList = new List<char>();
            GuessCount = 0;
            Timer.Reset();
            Timer.Start();
        }

    }
}