using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hangman
{
    public class HangmanGame
    {
        private const int MaxLives = 5;
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

        private List<(string, string)> LoadCountriesAndCities()
        {
            return new List<(string, string)> { ("Poland", "Warsaw") };
            // TODO: actually load data from the file
        }

        public void Play()
        {
            CountriesAndCities = LoadCountriesAndCities();
            if (CountriesAndCities.Count == 0)
            {
                Console.WriteLine("Sorry, there are no city names to guess.");
                return;
            }

            while (true)
            {
                ResetTheGame();
                while (Lives > 0)
                {
                    Display();
                    // TODO: the actual gameplay
                    Lives--;
                }
                // TODO: play again?
                break;
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
            CityMask = Enumerable.Repeat(true, CurrentCity.Length).ToList();
            NotInWordList = new List<char>();
        }
    }
}