using System;
using System.Threading;

namespace SudokuCollective.Dev.Classes
{
    public class ConsoleSpiner
    {
        int counter;

        public ConsoleSpiner()
        {
            counter = 0;
        }

        public void Turn()
        {
            Thread.Sleep(100);
            counter++;

            switch (counter % 4)
            {
                case 0: Console.Write("  Working    / "); break;
                case 1: Console.Write("  Working.   - "); break;
                case 2: Console.Write("  Working..  \\ "); break;
                case 3: Console.Write("  Working... | "); break;
            }

            Console.SetCursorPosition(Console.CursorLeft - 15, Console.CursorTop);
        }
    }
}
