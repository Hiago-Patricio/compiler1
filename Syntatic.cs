using System;
using System.Text;
using System.Linq;

namespace Compiler
{
    class Syntatic
    {
        private LexScanner lexScanner;
        private Token token;
        private int temp = 0;

        private StringBuilder code = new StringBuilder("operator;arg1;arg2;result\n");

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
                    $"Erro sintático, era esperado um fim de cadeia, mas foi encontrado {(token == null ? "NULL": token.value)}");
            }
        }

        private string generateTemp()
        {
            return $"t{temp++}";
        }

        private void generateCode(string op, string arg1, string arg2, string result)
        {
            code.Append($"{op};{arg1};{arg2};{result}\n");
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
        
        // <programa> -> program ident <corpo> .
        private void programa()
        {
            Console.WriteLine($"programa");
            if (verifyTokenValue("program"))
            {
                getToken();
                if (verifyTokenType(EnumToken.IDENTIFIER))
                {
                    corpo();
                    getToken();
                    if (!verifyTokenValue("."))
                    {
                        throw new Exception($"Erro sintático, '.' era esperado, mas foi encontrado {(token == null ? "NULL": token.value)}");
                    }
                    getToken();
                }
                else
                {
                    throw new Exception($"Erro sintático, identificador era esperado, mas foi encontrado {(token == null ? "NULL": token.value)}");    
                }
            }
            else
            {
                throw new Exception($"Erro sintático, 'program' era esperado, mas foi encontrado {(token == null ? "NULL": token.value)}");
            }
        }

        // <corpo> -> <dc> begin <comandos> end
        private void corpo()
        {
            Console.WriteLine($"corpo");
            dc();
            if (verifyTokenValue("begin"))
            {
                comandos();
                if (!verifyTokenValue("end"))
                {
                    throw new Exception($"Erro sintático, 'end' ou ';' era esperado, mas foi encontrado {(token == null ? "NULL": token.value)}");
                }
            }
            else
            {
                throw new Exception($"Erro sintático, 'begin' ou ';' era esperado, mas foi encontrado {(token == null ? "NULL": token.value)}");
            }
        }
        
        // <dc> -> <dc_v> <mais_dc>  | λ
        private void dc()
        {
            Console.WriteLine($"dc");
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
            Console.WriteLine($"dc_v");
            tipo_var();
            getToken();
            if (verifyTokenValue(":"))
            {
                variaveis();
            }
            else
            {
                throw new Exception($"Erro sintático, ':' era esperado, mas foi encontrado: {(token == null ? "NULL": token.value)}");
            }
        }

        // <tipo_var> -> real | integer
        private void tipo_var()
        {
            Console.WriteLine($"tipo_var");
            if (!verifyTokenValue("real", "integer"))
            {
                throw new Exception($"Erro sintático, 'real', 'integer' ou 'begin' era esperado, mas foi encontrado: {(token == null ? "NULL": token.value)}");
            }
        }

        // <variaveis> -> ident <mais_var>
        private void variaveis()
        {
            Console.WriteLine($"variaveis");
            getToken();
            if (verifyTokenType(EnumToken.IDENTIFIER))
            {
                mais_var();
            }
            else
            {
                throw new Exception($"Erro sintático, identificador era esperado, mas foi encontrado: {(token == null ? "NULL": token.value)}");
            }
        }
        
        // <mais_var> -> , <variaveis> | λ
        private void mais_var()
        {
            Console.WriteLine($"mais_var");
            getToken();
            if (verifyTokenValue(","))
            {
                variaveis();
            }
        }
        
        // <mais_dc> -> ; <dc> | λ
        private void mais_dc()
        {
            Console.WriteLine($"mais_dc");
            if (verifyTokenValue(";"))
            {
                dc();
            }
        }
        
        // <comandos> -> <comando> <mais_comandos>
        private void comandos()
        {
            Console.WriteLine($"comandos");
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
            Console.WriteLine($"comando");
            getToken();
            if (verifyTokenValue("read", "write"))
            {
                getToken();
                if (verifyTokenValue("("))
                {
                    getToken();
                    if (verifyTokenType(EnumToken.IDENTIFIER))
                    {
                        getToken();
                        if (!verifyTokenValue(")"))
                        {
                            throw new Exception($"Erro sintático, ')' era esperado, mas foi encontrado: {(token == null ? "NULL": token.value)}");
                        }
                        getToken();
                    }
                    else
                    {
                        throw new Exception($"Erro sintático, identificador era esperado, mas foi encontrado: {(token == null ? "NULL": token.value)}");    
                    }
                }
                else
                {
                    throw new Exception($"Erro sintático, '(' era esperado, mas foi encontrado: {(token == null ? "NULL": token.value)}");
                }
            }
            else if (verifyTokenType(EnumToken.IDENTIFIER))
            {
                getToken();
                if (verifyTokenValue(":="))
                {
                    expressao();
                }
                else
                {
                    throw new Exception($"Erro sintático, ':=' era esperado, mas foi encontrado: {(token == null ? "NULL": token.value)}");
                }
            }
            else if (verifyTokenValue("if"))
            {   
                condicao();
                if (verifyTokenValue("then"))
                {
                    comandos();
                    pfalsa();
                    if (!verifyTokenValue("$"))
                    {
                        throw new Exception($"Erro sintático, '$' ou ';' era esperado, mas foi encontrado: {(token == null ? "NULL": token.value)}");
                    }
                    getToken();
                }
                else
                {
                    throw new Exception($"Erro sintático, 'then' era esperado, mas foi encontrado: {(token == null ? "NULL": token.value)}");
                }
            }
            else
            {
                throw new Exception($"Erro sintático, 'read', 'write', 'if' ou identificador era esperado, mas foi encontrado: {(token == null ? "NULL": token.value)}");
            }
        }
        
        // <mais_comandos> -> ; <comandos> | λ 
        private void mais_comandos()
        {
            Console.WriteLine($"mais_comandos");
            if (verifyTokenValue(";"))
            {
                comandos();
            }
        }

        // <expressao> -> <termo> <outros_termos>
        private void expressao()
        {
            Console.WriteLine($"expressao");
            termo();
            outros_termos();
        }

        // <outros_termos> -> <op_ad> <termo> <outros_termos> | λ
        private void outros_termos()
        {
            Console.WriteLine($"outros_termos");
            if (verifyTokenValue("+", "-"))
            {
                op_ad();
                getToken();
                termo();
                outros_termos();
            }
        }

        // <op_ad> -> + | -
        private void op_ad()
        {
            Console.WriteLine($"op_ad");
            // TODO semântico
        }

        // <condicao> -> <expressao> <relacao> <expressao>  
        private void condicao()
        {
            Console.WriteLine($"condicao");
            expressao();
            relacao();
            expressao();
        }

        // <pfalsa> -> else <comandos> | λ  
        private void pfalsa()
        {
            Console.WriteLine($"pfalsa");
            if (verifyTokenValue("else"))
            {
                comandos();
            }
        }

        // <relacao> -> = | <> | >= | <= | > | <  
        private void relacao()
        {
            Console.WriteLine($"relacao");
            if (!verifyTokenValue("=", "<>", "<>", ">=", "<=", ">", "<"))
            {
                throw new Exception($"Erro sintático, '=', '<>', '>=', '<=', '>' ou '<' era esperado, mas foi encontrado: {(token == null ? "NULL": token.value)}");
            }
        }

        // <termo> -> <op_un> <fator> <mais_fatores> 
        private void termo()
        {
            Console.WriteLine($"termo");
            op_un();
            fator();
            mais_fatores();
        }

        // <op_un> -> - | λ
        private void op_un()
        {
            Console.WriteLine($"op_un");
            if (!verifyTokenType(EnumToken.IDENTIFIER))
            {
                getToken();
                if (verifyTokenValue("-"))
                {
                    getToken();
                    // TODO
                }
            }
            
        }

        // <fator> -> ident | numero_int | numero_real | (<expressao>)   
        private void fator()
        {
            Console.WriteLine($"fator");
            if (verifyTokenValue("("))
            {
                expressao();
                getToken();
                if (!verifyTokenValue(")"))
                {
                    throw new Exception($"Erro sintático, ')' esperado, mas foi recebido: {(token == null ? "NULL": token.value)}");
                }
            }
            else if (verifyTokenType(EnumToken.IDENTIFIER, EnumToken.INTEGER, EnumToken.REAL))
            {
                // TODO
            }
            else
            {
                throw new Exception($"Erro sintático, identificador, número inteiro, número real ou '(' era esperado, mas foi encontrado: {(token == null ? "NULL": token.value)}");
            }
        }

        // <mais_fatores> -> <op_mul> <fator> <mais_fatores> | λ  
        private void mais_fatores()
        {
            Console.WriteLine($"mais_fatores");
            getToken();
            if (verifyTokenValue("*", "/"))
            {
                op_mul();
                getToken();
                fator();
                mais_fatores();
            }
        }

        // <op_mul> -> * | / 
        private void op_mul()
        {
            Console.WriteLine($"op_mul");
            // TODO semântico
        }
    }
}