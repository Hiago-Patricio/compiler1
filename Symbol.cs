namespace Compiler
{
    public class Symbol
    {
        public Symbol(string type, string value)
        {
            this.type = type;
            this.value = value;
        }

        public string value {get; set; }
        public string type {get; set; }

        public override string ToString()
        {
            return $"Token[{type}, {value}]";
        }
    }
}