namespace Compiler
{
    public class Symbol
    {
        public Symbol(EnumToken type, string value)
        {
            this.type = type;
            this.value = value;
        }

        public string value {get; set; }
        public EnumToken type {get; set; }

        public override string ToString()
        {
            return $"Token[{type}, {value}]";
        }
    }
}