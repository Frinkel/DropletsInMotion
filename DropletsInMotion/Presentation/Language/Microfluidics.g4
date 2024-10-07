grammar Microfluidics;

// Parser rules
program
    : (command ';')* EOF
    ;

// Commands
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
    | assignment
    | ifStatement
    | whileLoop
    | sensorCommand
    | actuatorCommand
    ;

// Declarations
dropletDeclaration
    : 'Droplet' '(' IDENTIFIER ',' arithmeticExpression ',' arithmeticExpression ',' arithmeticExpression ')'
    ;

dispense
    : 'Dispense' '(' IDENTIFIER ',' IDENTIFIER ',' arithmeticExpression ')'
    ;

// Move
moveDroplet
    : 'Move' '(' IDENTIFIER ',' arithmeticExpression ',' arithmeticExpression ')'
    ;

// Split
splitByRatio
    : 'SplitByRatio' '(' IDENTIFIER ',' IDENTIFIER ',' IDENTIFIER ',' arithmeticExpression ',' arithmeticExpression ',' arithmeticExpression ',' arithmeticExpression ',' arithmeticExpression ')'
    ;

splitByVolume
    : 'SplitByVolume' '(' IDENTIFIER ',' IDENTIFIER ',' IDENTIFIER ',' arithmeticExpression ',' arithmeticExpression ',' arithmeticExpression ',' arithmeticExpression ',' arithmeticExpression ')'
    ;

// Merge
merge
    : 'Merge' '(' IDENTIFIER ',' IDENTIFIER ',' IDENTIFIER ',' arithmeticExpression ',' arithmeticExpression ')'
    ;

// Mix
mix
    : 'Mix' '(' IDENTIFIER ',' arithmeticExpression ',' arithmeticExpression ',' arithmeticExpression ',' arithmeticExpression ',' arithmeticExpression ')'
    ;

// Store
store
    : 'Store' '(' IDENTIFIER ',' arithmeticExpression ',' arithmeticExpression ',' arithmeticExpression ')'
    ;

// Wait
wait
    : 'Wait' '(' arithmeticExpression ')'
    ;

waitForUserInput
    : 'WaitForUserInput' '(' ')'
    ;

// Variable Assignment
assignment
    : IDENTIFIER '=' arithmeticExpression
    ;

// If-Else Statement
ifStatement
    : 'if' '(' booleanExpression ')' block ('else' block)?
    ;

// While Loop
whileLoop
    : 'while' '(' booleanExpression ')' block
    ;

// Sensor access
sensorCommand
    : IDENTIFIER '=' 'sensor' '(' STRING ',' arithmeticExpression ')'
    ;

// Actuator access
actuatorCommand
    : IDENTIFIER '=' 'actuate' '(' STRING ',' arithmeticExpression ')'
    ;

// Block (for if/else and loops)
block
    : '{' (command ';')* '}'
    ;



// BooleanExpression for conditions (e.g. comparisons and logical expressions)
booleanExpression
    : booleanExpression '&&' booleanExpression
    | booleanExpression '||' booleanExpression
    | '!' booleanExpression
    | arithmeticExpression op=('>'|'<'|'=='|'!=') arithmeticExpression
    | '(' booleanExpression ')'
    ;

// ArithmeticExpression for all numerical expressions
arithmeticExpression
    : arithmeticExpression op=('*'|'/') arithmeticExpression
    | arithmeticExpression op=('+'|'-') arithmeticExpression
    | '(' arithmeticExpression ')'
    | '-' ArithmeticExpression
    | FLOAT
    | INT
    | IDENTIFIER
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

STRING
    : '"' (~["\\] | '\\' .)* '"'
    ;

WS
    : [ \t\r\n]+ -> skip
    ;

// Operators
fragment
ADD: '+';
fragment
SUB: '-';
fragment
MUL: '*';
fragment
DIV: '/';
fragment
LT: '<';
fragment
GT: '>';
fragment
EQ: '==';
fragment
NEQ: '!=';
fragment
AND: '&&';
fragment
OR: '||';
