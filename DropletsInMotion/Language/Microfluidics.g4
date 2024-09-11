grammar Microfluidics;

// Parser rules
program
    : (statement ';')* EOF
    ;

statement
    : dropletDeclaration
    | moveDroplet
    | expr
    ;

dropletDeclaration
    : 'Droplet' '(' IDENTIFIER ',' INT ',' INT ',' FLOAT ')'
    ;

moveDroplet
    : 'Move' '(' IDENTIFIER ',' INT ',' INT ')'
    ;


// Expressions
expr
    : expr '+' expr   # AddExpr  // Rule for addition
    | INT             # IntExpr  // Integers are now part of the expression rules
    ;

// Lexer rules
IDENTIFIER
    : [a-zA-Z_][a-zA-Z_0-9]*
    ;

INT
    : [0-9]+
    ;

FLOAT
    : [0-9]+ '.' [0-9]+
    ;

WS
    : [ \t\r\n]+ -> skip
    ;
