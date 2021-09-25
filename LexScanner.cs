using System;
using System.IO;
using System.Linq;

namespace Compiler
{
    class LexScanner
    {
        public LexScanner(string path)
        {
            try
            {
                using var sr = new StreamReader(path);
                Content = sr.ReadToEnd();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }

            Content = System.IO.File.ReadAllText(path);
        }

        private string Content {get; set; }
        private int Pos {get; set; }
        private int State {get; set; }

        private bool IsLetter(char c) 
        {
            return c is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z');
        }

        private bool IsDigit(char c) 
        {
            return c >= '0' && c <= '9';
        }

        private bool IsWhiteSpace(char c) 
        {
            return c is ' ' or '\n' or '\t' or '\r';
        }

        private bool IsSymbol(char c)
        {
            return (new [] {
                "(", 
                ")", 
                "$", 
                ".", 
                ",", 
                ":", 
                ";", 
                "=", 
                ">", 
                "<", 
                "-", 
                "+", 
                "*", 
                "/",
            }).Contains(c.ToString());
        }

        private bool IsReservedKeyword(string term) 
        {    
            return (new [] {
                "if",
                "then",
                "else",
                "read",
                "write",
                "program",
                "begin",
                "end",
                "real",
                "integer",
            }).Contains(term);
        }

        private bool IsEof() 
        {
            return Pos >= Content.Length;
        }

        private char NextChar() 
        {
            if (IsEof())
            {
                return (char)0;
            }
            return Content[Pos++];
        }

        private void Back()
        {
            Pos--;
        }

        public Token NextToken()
        {
            if (IsEof()) 
            {
                return null;
            }

            State = 0;
            char c;
            String term = "";
            while (true) 
            {
                if (IsEof())
                {
                    Pos = Content.Length + 1;
                }
                c = NextChar();
                switch (State)
                {
                    case 0:
                        if (IsWhiteSpace(c))
                        {
                            State = 0;
                        } 
                        else if (IsLetter(c))
                        {
                            State = 1;
                            term += c;
                        }
                        else if (IsSymbol(c))
                        {
                            State = c switch
                            {
                                ':' or '>' => 6,
                                '<' => 8,
                                _ => 5
                            };
                            term += c;
                        }
                        else if (IsDigit(c))
                        {
                            State = 2;
                            term += c;
                        }
                        else
                        {
                            return null;
                        }
                        break;
                    case 1:
                        if (IsLetter(c) || IsDigit(c))
                        {
                            term += c;
                        }
                        else if (IsReservedKeyword(term))
                        {
                            Back();
                            return new Token(EnumToken.RESERVED_KEYWORD, term);
                        }
                        else
                        {
                            Back();
                            return new Token(EnumToken.IDENTIFIER, term);
                        }
                        break;
                    case 2:
                        if (IsDigit(c))
                        {
                            term += c;
                        }
                        else if (c == '.')
                        {
                            State = 3;
                            term += c;
                        }
                        else
                        {
                            Back();
                            return new Token(EnumToken.INTEGER, term);
                        }
                        break;
                    case 3:
                        if (IsDigit(c))
                        {
                            State = 4;
                            term += c;
                        }
                        break;
                    case 4:
                        if (IsDigit(c))
                        {
                            term += c;   
                        }
                        else
                        {
                            Back();
                            return new Token(EnumToken.REAL, term);
                        }
                        break;
                    case 5:
                        Back();
                        return new Token(EnumToken.SYMBOL, term);
                    case 6:
                        if (c == '=') 
                        {
                            State = 7;
                            term += c;
                        }
                        else 
                        {
                            Back();
                            return new Token(EnumToken.SYMBOL, term);
                        }
                        break;
                    case 7:
                        Back();
                        return new Token(EnumToken.SYMBOL, term);
                    case 8:
                        if (c is '=' or '>')
                        {
                            State = 9;
                            term += c;
                        }
                        else
                        {
                            Back();
                            return new Token(EnumToken.SYMBOL, term);
                        }
                        break;
                    case 9:
                        Back();
                        return new Token(EnumToken.SYMBOL, term);
                }
            }
        }
    }
}