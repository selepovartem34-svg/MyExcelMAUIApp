grammar LabCalculator;

/*
 * Parser Rules
 */

compileUnit : expression EOF;

expression :
	LPAREN expression RPAREN #ParenthesizedExpr
	| <assoc=right> expression EXPONENT expression #ExponentialExpr
	| operatorToken=(ADD | SUBTRACT) expression #UnaryExpr
	| expression operatorToken=(MULTIPLY | DIVIDE) expression #MultiplicativeExpr
	| expression operatorToken=(ADD | SUBTRACT) expression #AdditiveExpr
	| operatorToken=(MOD | DIV) LPAREN expression ',' expression RPAREN #ModDivExpr
	| MMAX LPAREN paramlist=arglist RPAREN #MmaxExpr
	| MMIN LPAREN paramlist=arglist RPAREN #MminExpr
	| NUMBER #NumberExpr
	| IDENTIFIER #IdentifierExpr
	;

arglist: expression (',' expression)+;

/*
 * Lexer Rules
 */

NUMBER : INT ('.' INT)?;
IDENTIFIER : [a-zA-Z]+[1-9][0-9]*;

fragment INT : ('0'..'9')+;

EXPONENT : '^';
MULTIPLY : '*';
DIVIDE : '/';
SUBTRACT : '-';
ADD : '+';
LPAREN : '(';
RPAREN : ')';

MOD : 'mod';
DIV : 'div';
MMAX : 'mmax';
MMIN : 'mmin';

WS : [ \t\r\n] -> channel(HIDDEN);
