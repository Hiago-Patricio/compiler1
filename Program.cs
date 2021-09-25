using System;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var path = args.Length > 0 ? args[0] : @"input.txt";
            new Syntatic(path).analysis();
        }
    }
}
