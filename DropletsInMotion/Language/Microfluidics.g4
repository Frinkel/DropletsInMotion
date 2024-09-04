grammar Microfluidics;

// Parser rules
program
    : (statement ';')* EOF
    ;

statement
    : dropletDeclaration
    | moveDroplet
    ;

dropletDeclaration
    : 'Droplet' '(' IDENTIFIER ',' INT ',' INT ',' FLOAT ')'
    ;

moveDroplet
    : 'Move' '(' IDENTIFIER ',' INT ',' INT ')'
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
