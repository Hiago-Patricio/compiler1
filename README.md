# compiler
This project was developed for Compilers 1 subject. The implementation is just the frontend of a compiler, thus it analysis is just the syntactic and semantic. 

To run the program use ```dotnet run``` or ```dotnet run file```.

Automaton:
![alt text](https://github.com/Hiago-Patricio/compiler/blob/main/Automaton.png)

Syntactic grammar:

```
<programa> -> program ident <corpo> .
<corpo> -> <dc> begin <comandos> end
<dc> -> <dc_v> <mais_dc>  | λ
<mais_dc> -> ; <dc> | λ
<dc_v> ->  <tipo_var> : <variaveis>
<tipo_var> -> real | integer
<variaveis> -> ident <mais_var>
<mais_var> -> , <variaveis> | λ
<comandos> -> <comando> <mais_comandos>
<mais_comandos> -> ; <comandos> | λ
<comando> -> read (ident) |
             write (ident) |
             ident := <expressao> |
             if <condicao> then <comandos> <pfalsa> $
<condicao> -> <expressao> <relacao> <expressao>
<relacao> -> = | <> | >= | <= | > | <
<expressao> -> <termo> <outros_termos>
<termo> -> <op_un> <fator> <mais_fatores>
<op_un> -> - | λ
<fator> -> ident | numero_int | numero_real | (<expressao>)
<outros_termos> -> <op_ad> <termo> <outros_termos> | λ
<op_ad> -> + | -
<mais_fatores> -> <op_mul> <fator> <mais_fatores> | λ
<pfalsa> -> else <comandos> | λ
<op_mul> -> * | /
```

Semantic grammar:
```
<programa> -> program ident <corpo> . {generateCode("PARA", "", "", "")}
<corpo> -> <dc> begin <comandos> end
<dc> -> <dc_v> <mais_dc>  | λ
<mais_dc> -> ; <dc> | λ
<dc_v> ->  <tipo_var> : {variaveis.esq = tipo_var.dir} <variaveis>
<tipo_var> -> real | integer
<variaveis> -> ident {
    addEntry(ident, variaveis.esq),
    if (variaveis.esq == "real") {
        generateCode("ALME", 0.0, "", ident)
    } else {
        generateCode("ALME", 0, "", ident)
    }, 
    mais_var.esq = variaveis.esq
} <mais_var>
<mais_var> -> , {variaveis.esq = mais_var.esq} <variaveis>
<mais_var> -> λ
<comandos> -> <comando> <mais_comandos>
<mais_comandos> -> ; <comandos> | λ
<comando> -> read (ident) {generateCode("read", "", "", ident)}
<comando> -> write (ident) {generateCode("write", ident, "", "")}
<comando> -> ident := <expressao> {generateCode(":=", expressao.dir, "", ident)}
<comando> -> if <condicao> then {
    generateCode("JF", condicao.dir, "JF_line", "")
} <comandos> {
    generateCode("goto", "goto_line", "", ""),
    replaceLastOccurence("JF_line")
} <pfalsa> {
    replaceLastOccurence("goto_line")
} $
<condicao> -> <expressao> <relacao> <expressaoLinha> {
    t = generateTemp(),
    generateCode(relacao.dir, expressao.dir, expressaoLinha.dir, t),
    condicao.dir = t
}
<relacao> -> =  {relacao.dir = "="}
<relacao> -> <> {relacao.dir = "<>"}
<relacao> -> >= {relacao.dir = ">="}
<relacao> -> <= {relacao.dir = "<="}
<relacao> -> >  {relacao.dir = ">"}
<relacao> -> <  {relacao.dir = "<"}
<expressao> -> <termo> {outros_termos.esq = termo.dir} <outros_termos> {expressao.dir = outros_termos.dir}
<termo> -> <op_un> {fator.esq = op_un.dir} <fator> {mais_fatores.esq = fator.dir} <mais_fatores> {termo.dir = mais_fatores.dir}
<op_un> -> - {op_un.dir = "-"}
<op_un> -> λ {op_un.dir = "λ"}
<fator> -> ident {
    if (fator.esq == "-") {
        t = generateTemp()
        generateCode("minus", ident, "", t)
        fator.dir = t
    } else {
        fator.dir = ident            
    }    
}
<fator> -> numero_int {
    if (fator.esq == "-") {
        t = generateTemp()
        generateCode("minus", numero_int, "", t)
        fator.dir = t
    } else {
        fator.dir = numero_int            
    }    
}
<fator> -> numero_real {
    if (fator.esq == "-") {
        t = generateTemp()
        generateCode("minus", numero_real, "", t)
        fator.dir = t
    } else {
        fator.dir = numero_real            
    }    
}
<fator> -> (<expressao>) {
    fator.esq = expressao.dir,
    if (fator.esq == "-") {
        t = generateTemp()
        generateCode("minus", expressao.dir, "", t)
        fator.dir = t
    } else {
        fator.dir = expressao.dir            
    }    
}
<outros_termos> -> <op_ad> <termo> {
    t = generateTemp(),
    generateCode(op_ad.dir, outros_termos.esq, termo.dir, t),
    term.dir = t,
    outros_termos.esq = termo.dir
} <outros_termos>
<outros_termos> -> λ {outros_termos.dir = outros_termos.esq}
<op_ad> -> + {op_ad.dir = "+"}
<op_ad> -> - {op_ad.dir = "-"}
<mais_fatores> -> <op_mul> {fator.esq = op_mul.dir} <fator> {
    t = generateTemp()
    if (op_mul.dir == "*") {
        generateCode("*", mais_fatores.esq, fator.dir, t)
    } else {
        generateCode("/", mais_fatores.esq, fator.dir, t)
    },
    fator.dir = t,
    mais_fatores_linha.esq = fator.dir    
} <mais_fatores_linha> {mais_fatores.dir = mais_fatores_linha.dir}
<mais_fatores> -> λ {mais_fatores.dir = mais_fatores.esq}
<pfalsa> -> else <comandos> | λ
<op_mul> -> * {op_mul.dir = "*"}
<op_mul> -> / {op_mul.dir = "/"}
```