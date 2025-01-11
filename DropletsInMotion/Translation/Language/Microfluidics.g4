grammar Microfluidics;

// Parser rules
program
    : (command ';')* EOF
    ;

// Commands
command
    : dropletDeclaration
    | dispenserDeclaration
    | dispense
    | moveDroplet
    | splitByRatio
    | splitByVolume
    | merge
    | mix
    | store
    | wait
    | waitForUserInput
    | sensorCommand
    | assignment
    | ifStatement
    | whileLoop
    | actuatorCommand
    | printStatement
    | waste
    ;

// Declarations
dropletDeclaration
    : 'Droplet' '(' IDENTIFIER ',' arithmeticExpression ',' arithmeticExpression ',' arithmeticExpression (',' STRING)? ')'    
    ;

dispenserDeclaration
    : 'DeclareDispenser' '(' IDENTIFIER ',' STRING  ',' STRING')'
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
    : 'Store' '(' IDENTIFIER ',' arithmeticExpression ',' arithmeticExpression ',' arithmeticExpression ')' # StoreWithPositions
    | 'Store' '(' IDENTIFIER ',' arithmeticExpression ')' # StoreWithTimeOnly
    ;


// Waste
waste
	: 'Waste' '(' IDENTIFIER ',' arithmeticExpression ',' arithmeticExpression ')'
	;

// Wait
wait
    : 'Wait' '(' arithmeticExpression ')'
    ;

waitForUserInput
    : 'WaitForUserInput' '(' ')'
    ;

// Sensor access
sensorCommand
    : IDENTIFIER '=' 'Sensor' '(' IDENTIFIER ',' STRING ',' STRING ')'
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

// Actuator access with dynamic key-value arguments
actuatorCommand
    : 'Actuator' '(' (IDENTIFIER ',')? STRING (',' argumentKeyValuePair)* ')'
    ;

// Key-Value pair (e.g., temperature=80)
argumentKeyValuePair
    : IDENTIFIER '=' arithmeticExpression
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
    | '-' arithmeticExpression
    | FLOAT
    | INT
    | IDENTIFIER
    ;

// Print Statement
printStatement
    : 'print' '(' (printArgument (',' printArgument)*)? ')'
    ;

// Print arguments: can be strings or arithmetic expressions (variables/numbers)
printArgument
    : STRING 
    | arithmeticExpression
    | booleanExpression
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

// Single-line comment
LINE_COMMENT
    : '//' ~[\r\n]* -> skip
    ;

// Multi-line comment
BLOCK_COMMENT
    : '/*' .*? '*/' -> skip
    ;

// Operators
fragment
NOT: '!';
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
