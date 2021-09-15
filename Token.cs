using System;

namespace Compiler
{
    class Token
    {
        public Token(EnumToken type, string value)
        {
            this.type = type;
            this.value = value;
        }

        public string value {get; set; }
        public EnumToken type {get; set; }

        public override string ToString()
        {
            return $"Token[{type},{value}]";
        }

    }
}