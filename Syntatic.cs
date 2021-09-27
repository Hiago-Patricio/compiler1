using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Compiler
{
    class Syntatic
    {
        private LexScanner lexScanner;
        private Token token;
        private int temp = 1;
        private int currentLine = 0;
        private EnumToken? tipo;
        private Dictionary<String, Symbol> SymbolTable = new Dictionary<string, Symbol>();
        private string code = new string("operator;arg1;arg2;result\n");

        public Syntatic(string path)
        {
            lexScanner = new LexScanner(path);
        }

        public void analysis()
        {
            getToken();
            programa();
            if (token == null)
            {
                Console.WriteLine(code);
            }
            else
            {
                throw new Exception(
                    $"Erro sintático, era esperado um fim de cadeia, mas foi encontrado '{(token == null ? "NULL": token.value)}'.");
            }
        }
        
        private string generateTemp()
        {
            return $"t{temp++}";
        }

        private void generateCode(string op, string arg1, string arg2, string result)
        {
            code += $"{op};{arg1};{arg2};{result}\n";
            currentLine++;
        }

        private void getToken()
        {
            token = lexScanner.NextToken();
        }

        private bool verifyTokenValue(params string[] terms)
        {
            return terms.Any(t => token != null && token.value.Equals(t));
        }

        private bool verifyTokenType(params EnumToken[] enums)
        {
            return enums.Any(e => token != null && token.type.Equals(e));
        }

        private void replaceLastOccurence(string oldString)
        {
            int lastIndex = code.LastIndexOf(oldString);

            string beginString = code.Substring(0, lastIndex);
            string endString = code.Substring(lastIndex + oldString.Length);

            code = beginString + currentLine.ToString() + endString;
        }
        
        // <programa> -> program ident <corpo> .
        private void programa()
        {
            if (verifyTokenValue("program"))
            {
                getToken();
                if (verifyTokenType(EnumToken.IDENTIFIER))
                {
                    corpo();
                    getToken();
                    if (!verifyTokenValue("."))
                    {
                        throw new Exception($"Erro sintático, '.' era esperado, mas foi encontrado '{(token == null ? "NULL": token.value)}'.");
                    }
                    generateCode("PARA", "", "", "");
                    getToken();
                }
                else
                {
                    throw new Exception($"Erro sintático, identificador era esperado, mas foi encontrado '{(token == null ? "NULL": token.value)}'.");    
                }
            }
            else
            {
                throw new Exception($"Erro sintático, 'program' era esperado, mas foi encontrado '{(token == null ? "NULL": token.value)}'.");
            }
        }

        // <corpo> -> <dc> begin <comandos> end
        private void corpo()
        {
            dc();
            if (verifyTokenValue("begin"))
            {
                comandos();
                if (!verifyTokenValue("end"))
                {
                    throw new Exception($"Erro sintático, 'end' ou ';' era esperado, mas foi encontrado '{(token == null ? "NULL": token.value)}'.");
                }
            }
            else
            {
                throw new Exception($"Erro sintático, 'begin' ou ';' era esperado, mas foi encontrado '{(token == null ? "NULL": token.value)}'.");
            }
        }
        
        // <dc> -> <dc_v> <mais_dc>  | λ
        private void dc()
        {
            getToken();
            if (!verifyTokenValue("begin"))
            {
                dc_v();
                mais_dc();
            }
        }

        // <dc_v> ->  <tipo_var> : <variaveis>
        private void dc_v()
        {
            var tipoVarDir = tipo_var();
            getToken();
            if (verifyTokenValue(":"))
            {
                variaveis(tipoVarDir);
            }
            else
            {
                throw new Exception($"Erro sintático, ':' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
            }
        }

        // <tipo_var> -> real | integer
        private EnumToken tipo_var()
        {
            if (token.value == "real")
            {
                return EnumToken.REAL;
            }
            else if (token.value == "integer")
            {
                return EnumToken.INTEGER;
            }
            
            throw new Exception($"Erro sintático, 'real', 'integer' ou 'begin' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
        }

        // <variaveis> -> ident <mais_var>
        private void variaveis(EnumToken variaveisEsq)
        {
            getToken();
            if (verifyTokenType(EnumToken.IDENTIFIER))
            {
                if (SymbolTable.ContainsKey(token.value))
                {
                    throw new Exception($"Erro semântico, o identificador '{token.value}' já foi declarado.");
                }
                SymbolTable.Add(token.value, new Symbol(variaveisEsq ,token.value));
                generateCode("ALME", variaveisEsq == EnumToken.REAL ? "0.0" : "0", "", token.value);
                mais_var(variaveisEsq);
            }
            else
            {
                throw new Exception($"Erro sintático, identificador era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
            }
        }
        
        // <mais_var> -> , <variaveis> | λ
        private void mais_var(EnumToken maisVarEsq)
        {
            getToken();
            if (verifyTokenValue(","))
            {
                variaveis(maisVarEsq);
            }
        }
        
        // <mais_dc> -> ; <dc> | λ
        private void mais_dc()
        {
            if (verifyTokenValue(";"))
            {
                dc();
            }
        }
        
        // <comandos> -> <comando> <mais_comandos>
        private void comandos()
        {
            comando();
            mais_comandos();
        }

        /*
         * <comando> -> read (ident)
		 * <comando> ->	write (ident)
		 * <comando> ->	ident := <expressao>
		 * <comando> ->	if <condicao> then <comandos> <pfalsa> $
         */
        private void comando()
        {
            getToken();
            if (verifyTokenValue("read", "write"))
            {
                var opCode = token.value;
                getToken();
                if (verifyTokenValue("("))
                {
                    getToken();
                    if (verifyTokenType(EnumToken.IDENTIFIER))
                    {
                        var ident = token.value;
                        if (!SymbolTable.ContainsKey(ident))
                        {
                            throw new Exception($"Erro semântico, o identificador '{ident}' não foi declarado.");
                        }
                        
                        getToken();
                        if (!verifyTokenValue(")"))
                        {
                            throw new Exception($"Erro sintático, ')' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
                        }

                        if (opCode == "read")
                        {
                            generateCode("read", "", "", ident);
                        }
                        else
                        {
                            generateCode("write", ident, "", "");
                        }
                        
                        getToken();
                    }
                    else
                    {
                        throw new Exception($"Erro sintático, identificador era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");    
                    }
                }
                else
                {
                    throw new Exception($"Erro sintático, '(' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
                }
            }
            else if (verifyTokenType(EnumToken.IDENTIFIER))
            {
                var ident = token.value;
                if (!SymbolTable.ContainsKey(ident))
                {
                    throw new Exception($"Erro semântico, o identificador '{ident}' não foi declarado.");
                }
                getToken();
                if (verifyTokenValue(":="))
                {
                    tipo = SymbolTable[ident].type;
                    var expressaoDir = expressao();
                    tipo = null;
                    generateCode(":=", expressaoDir, "", ident);
                }
                else
                {
                    throw new Exception($"Erro sintático, ':=' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
                }
            }
            else if (verifyTokenValue("if"))
            {   
                var condicaoDir = condicao();
                if (verifyTokenValue("then"))
                {
                    generateCode("JF", condicaoDir, "JF_line", "");
                    comandos();
                    generateCode("goto", "goto_line", "", "");
                    replaceLastOccurence("JF_line");
                    pfalsa();
                    replaceLastOccurence("goto_line");
                    if (!verifyTokenValue("$"))
                    {
                        throw new Exception($"Erro sintático, '$' ou ';' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
                    }
                    getToken();
                }
                else
                {
                    throw new Exception($"Erro sintático, 'then' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
                }
            }
            else
            {
                throw new Exception($"Erro sintático, 'read', 'write', 'if' ou identificador era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
            }
        }
        
        // <mais_comandos> -> ; <comandos> | λ 
        private void mais_comandos()
        {
            if (verifyTokenValue(";"))
            {
                comandos();
            }
        }

        // <expressao> -> <termo> <outros_termos>
        private string expressao()
        {
            var termoDir = termo();
            var outrosTermosDir = outros_termos(termoDir);

            return outrosTermosDir;
        }

        // <outros_termos> -> <op_ad> <termo> <outros_termos> | λ
        private string outros_termos(string outrosTermosEsq)
        {
            if (verifyTokenValue("+", "-"))
            {
                var opAdDir = op_ad();
                getToken();
                var termoDir = termo();
                
                var t = generateTemp();
                generateCode(opAdDir, outrosTermosEsq, termoDir, t);
                termoDir = t;
                
                return outros_termos(termoDir);
            }

            return outrosTermosEsq;
        }

        // <op_ad> -> + | -
        private string op_ad()
        {
            return token.value;
        }

        // <condicao> -> <expressao> <relacao> <expressao>  
        private string condicao()
        {
            var expressaoDir = expressao();
            var relacaoDir = relacao();
            var expressaoLinhaDir = expressao();
            var t = generateTemp();
            generateCode(relacaoDir, expressaoDir, expressaoLinhaDir, t);
            return t;
        }

        // <pfalsa> -> else <comandos> | λ  
        private void pfalsa()
        {
            if (verifyTokenValue("else"))
            {
                comandos();
            }
        }

        // <relacao> -> = | <> | >= | <= | > | <  
        private string relacao()
        {
            if (!verifyTokenValue("=", "<>", "<>", ">=", "<=", ">", "<"))
            {
                throw new Exception($"Erro sintático, '=', '<>', '>=', '<=', '>' ou '<' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
            }

            return token.value;
        }

        // <termo> -> <op_un> <fator> <mais_fatores> 
        private string termo()
        {
            var opUnDir = op_un();
            var fatorDir = fator(opUnDir);
            var maisFatoresDir = mais_fatores(fatorDir);

            return maisFatoresDir;
        }

        // <op_un> -> - | λ
        private string op_un()
        {
            if (!verifyTokenType(EnumToken.IDENTIFIER))
            {
                getToken();
                if (verifyTokenValue("-"))
                {
                    getToken();
                    return token.value;
                }
            }
            return "";
        }

        // <fator> -> ident | numero_int | numero_real | (<expressao>)   
        private string fator(string fatorEsq)
        {
            if (verifyTokenValue("("))
            {
                var expressaoDir = expressao();
                
                getToken();
                if (!verifyTokenValue(")"))
                {
                    throw new Exception($"Erro sintático, ')' esperado, mas foi recebido: '{(token == null ? "NULL": token.value)}'.");
                }
                
                if (fatorEsq == "-")
                {
                    var t = generateTemp();
                    generateCode("minus", expressaoDir, "", t);
                    return t;
                }

                return expressaoDir;

            }
            else if (verifyTokenType(EnumToken.INTEGER, EnumToken.REAL))
            {
                var number = token.value;

                if (tipo != null && token.type != tipo)
                {
                    throw new Exception($"Erro semântico, os tipos são diferentes.");
                }
                
                if (fatorEsq == "-")
                {
                    var t = generateTemp();
                    generateCode("minus", number, "", t);
                    return t;
                }

                return number;
            } else if (verifyTokenType(EnumToken.IDENTIFIER))
            {
                var ident = token.value;
                
                if (!SymbolTable.ContainsKey(ident))
                {
                    throw new Exception($"Erro semântico, o identificador '{ident}' não foi declarado.");
                }

                if (tipo != null && tipo != SymbolTable[ident].type)
                {
                    throw new Exception($"Erro semântico, os tipos são diferentes.");
                }
                
                if (fatorEsq == "-")
                {
                    var t = generateTemp();
                    generateCode("minus", ident, "", t);
                    return t;
                }

                return ident;
            }
            throw new Exception($"Erro sintático, identificador, número inteiro, número real ou '(' era esperado, mas foi encontrado: '{(token == null ? "NULL": token.value)}'.");
        }

        // <mais_fatores> -> <op_mul> <fator> <mais_fatores> | λ  
        private string mais_fatores(string maisFatoresEsq)
        {
            getToken();
            if (verifyTokenValue("*", "/"))
            {
                var opMulDir = op_mul();
                getToken();
                var fatorDir = fator(opMulDir);

                var t = generateTemp();
                if (opMulDir == "*")
                {
                    generateCode("*", maisFatoresEsq, fatorDir, t);
                }
                else
                {
                    generateCode("/", maisFatoresEsq, fatorDir, t);
                }

                fatorDir = t;
                return mais_fatores(fatorDir);
            }
            return maisFatoresEsq;
        }

        // <op_mul> -> * | / 
        private string op_mul()
        {
            return token.value;
        }
    }
}
