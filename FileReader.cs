using System;
using System.Collections.Generic;
using System.IO;

namespace Hangman
{
    class FileReader
    {
        public List<string[]> Read(string filename)
        {
            string line;
            List<string[]> records = new List<string[]>();

            try
            {
                using (StreamReader inputStream = new StreamReader(filename))
                {
                    while ((line = inputStream.ReadLine()) != null)
                    {
                        string[] items = line.Split('|');
                        records.Add(items);
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine($"Failed to open the file {filename}. Reason: {e.Message}");
            }

            return records;
        }
    }
}
