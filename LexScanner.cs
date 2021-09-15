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
                using (var sr = new StreamReader(path))
                {
                    this.content = sr.ReadToEnd();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
            }

            this.content = System.IO.File.ReadAllText(path);
        }

        private string content {get; set; }
        private int pos {get; set; }
        private int state {get; set; }

        private bool isLetter(char c) 
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        private bool isDigit(char c) 
        {
            return c >= '0' && c <= '9';
        }

        private bool isWhiteSpace(char c) 
        {
            return c == ' ' || c == '\n' || c == '\t';
        }

        private bool isSymbol(char c)
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

        private bool isReservedKeyword(string term) 
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

        private bool isEOF() 
        {
            return pos >= content.Length;
        }

        private char nextChar() 
        {
            if (isEOF())
            {
                return (char)0;
            }
            return content[pos++];
        }

        private void back()
        {
            pos--;
        }

        public Token nextToken()
        {
            if (isEOF()) 
            {
                return null;
            }

            state = 0;
            char c;
            String term = "";
            while (true) 
            {
                if (isEOF())
                {
                    pos = content.Length + 1;
                }
                c = nextChar();
                switch (state)
                {
                    case 0:
                        if (isWhiteSpace(c))
                        {
                            state = 0;
                        } 
                        else if (isLetter(c))
                        {
                            state = 1;
                            term += c;
                        }
                        else if (isSymbol(c))
                        {
                            if (c == ':' || c == '>') {
                                state = 6;
                            }
                            else if (c == '<')
                            {
                                state = 8;
                            } 
                            else
                            {
                                state = 5;
                            }
                            term += c;
                        }
                        else if (isDigit(c))
                        {
                            state = 2;
                            term += c;
                        }
                        break;
                    case 1:
                        if (isLetter(c) || isDigit(c))
                        {
                            term += c;
                        }
                        else if (isReservedKeyword(term))
                        {
                            back();
                            return new Token(EnumToken.RESERVED_KEYWORD, term);
                        }
                        else
                        {
                            back();
                            return new Token(EnumToken.IDENTIFIER, term);
                        }
                        break;
                    case 2:
                        if (isDigit(c))
                        {
                            term += c;
                        }
                        else if (c == '.')
                        {
                            state = 3;
                            term += c;
                        }
                        else
                        {
                            back();
                            return new Token(EnumToken.INTEGER, term);
                        }
                        break;
                    case 3:
                        if (isDigit(c))
                        {
                            state = 4;
                            term += c;
                        }
                        break;
                    case 4:
                        if (isDigit(c))
                        {
                            term += c;   
                        }
                        else
                        {
                            back();
                            return new Token(EnumToken.REAL, term);
                        }
                        break;
                    case 5:
                        back();
                        return new Token(EnumToken.SYMBOL, term);
                    case 6:
                        if (c == '=') 
                        {
                            state = 7;
                            term += c;
                        }
                        else 
                        {
                            back();
                            return new Token(EnumToken.SYMBOL, term);
                        }
                        break;
                    case 7:
                        back();
                        return new Token(EnumToken.SYMBOL, term);
                    case 8:
                        if (c == '=' || c == '>')
                        {
                            state = 9;
                            term += c;
                        }
                        else
                        {
                            back();
                            return new Token(EnumToken.SYMBOL, term);
                        }
                        break;
                    case 9:
                        back();
                        return new Token(EnumToken.SYMBOL, term);
                }
            }
        }
    }
}