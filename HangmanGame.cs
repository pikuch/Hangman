using System;
using System.Collections.Generic;
using System.Linq;

namespace Hangman
{
    public class HangmanGame
    {
        private const int MaxLives = 5;

        private Random random = new Random();

        private List<(string, string)> CountriesAndCities;

        private string CurrentCity;
        private string CurrentCountry;
        private List<bool> CityMask;
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
                    // TODO: the actual gameplay
                    Lives--;
                }
                // TODO: play again?
                break;
            }
        }

        private void ResetTheGame()
        {
            int cityIndex = random.Next(CountriesAndCities.Count);
            (CurrentCountry, CurrentCity) = CountriesAndCities[cityIndex];

            Lives = MaxLives;
            CityMask = Enumerable.Repeat(true, CurrentCity.Length).ToList();
        }
    }
}