grammar Microfluidics;

// Parser rules
program
    : (command ';')* EOF
    ;

command
    : dropletDeclaration
    | dispense
    | moveDroplet
    | splitByRatio
    | splitByVolume
    | merge
    | mix
    | store
    | wait
    | waitForUserInput
    ;

// Declarations
dropletDeclaration
    : 'Droplet' '(' IDENTIFIER ',' INT ',' INT ',' FLOAT ')'
    ;

dispense
    : 'Dispense' '(' IDENTIFIER ',' IDENTIFIER ',' FLOAT ')'
    ;

// Move
moveDroplet
    : 'Move' '(' IDENTIFIER ',' INT ',' INT ')'
    ;

// Split
splitByRatio
    : 'SplitByRatio' '(' IDENTIFIER ',' IDENTIFIER ',' IDENTIFIER ',' INT ',' INT ',' INT ',' INT ',' FLOAT ')'
    ;

splitByVolume
    : 'SplitByVolume' '(' IDENTIFIER ',' IDENTIFIER ',' IDENTIFIER ',' INT ',' INT ',' INT ',' INT ',' FLOAT ')'
    ;

// Merge
merge
    : 'Merge' '(' IDENTIFIER ',' IDENTIFIER ',' IDENTIFIER ',' INT ',' INT ')'
    ;

// Mix
mix
    : 'Mix' '(' IDENTIFIER ',' INT ',' INT ',' INT ',' INT ',' INT ')'
    ;

// Store
store
    : 'Store' '(' IDENTIFIER ',' INT ',' INT ',' INT ')'
    ;

// Wait
wait
    : 'Wait' '(' INT ')'
    ;

waitForUserInput
    : 'WaitForUserInput' '(' ')'
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
