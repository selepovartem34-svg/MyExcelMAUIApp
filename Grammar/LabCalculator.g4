grammar LabCalculator;

/*
 * Варіант 45: 1, 4, 6, 8, 10
 * +, -, *, /  |  ^  |  max(x,y), min(x,y)  |  =, <, >  |  not
 */

compileUnit : EQUAL? expression EOF;

expression :
	LPAREN expression RPAREN #ParenthesizedExpr
	| <assoc=right> expression EXPONENT expression #ExponentialExpr
	| expression operatorToken=(MULTIPLY | DIVIDE) expression #MultiplicativeExpr
	| expression operatorToken=(ADD | SUBTRACT) expression #AdditiveExpr
	| expression operatorToken=(EQUAL | LESS | GREATER) expression #ComparisonExpr
	| NOT expression #NotExpr
	| operatorToken=(MAX | MIN) LPAREN expression ',' expression RPAREN #MaxMinExpr
	| NUMBER #NumberExpr
	| IDENTIFIER #IdentifierExpr
	;

NUMBER : ('0'..'9')+;

MAX : [mM][aA][xX];
MIN : [mM][iI][nN];
NOT : [nN][oO][tT];

IDENTIFIER : [a-zA-Z]+[1-9][0-9]*;

EXPONENT : '^';
MULTIPLY : '*';
DIVIDE : '/';
SUBTRACT : '-';
ADD : '+';
LPAREN : '(';
RPAREN : ')';
EQUAL : '=';
LESS : '<';
GREATER : '>';

WS : [ \t\r\n] -> channel(HIDDEN);
