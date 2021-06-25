namespace Hangman
{
    public class HighscoreRecord
    {
        public string Name { get; set; }
        public string Date { get; set; }
        public string GuessingTime { get; set; }
        public int GuessingTries { get; set; }
        public string GuessedWord { get; set; }
        public override string ToString()
        {
            return Name + " | " + Date + " | " + GuessingTime + " | " + GuessingTries.ToString() + " | " + GuessedWord;
        }
    }
}