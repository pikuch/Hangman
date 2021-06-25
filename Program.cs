// There aren't many comments because I'm trying fo follow Uncle Bob's advice and use descriptive names instead

using System;

namespace Hangman
{
    class Program
    {
        static void Main()
        {
            HangmanGame theGame = new HangmanGame();
            theGame.Play();
        }
    }
}
