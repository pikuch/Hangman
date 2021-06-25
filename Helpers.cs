using System;

namespace Hangman
{
    class Helpers
    {
        public static bool[] MakeMaskFromString(string input)
        {
            bool[] mask = new bool[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                mask[i] = (input[i] != ' ');
            }
            return mask;
        }

        public static bool IsMaskClear(bool[] mask)
        {
            foreach (bool bit in mask)
            {
                if (bit)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
