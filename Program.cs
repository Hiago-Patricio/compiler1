using System;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            LexScanner ls = new LexScanner("input.txt");
            Token token = null;
            while (true)
            {
                token = ls.nextToken();
                if (token == null)
                {
                    break;
                }

                Console.WriteLine(token);
            }
        }
    }
}
