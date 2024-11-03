grammar Grammar;

/*
 * Parser Rules
 */

compileUnit: expression EOF;

expression:
	LPAREN expression RPAREN					# ParenthesizedExpr
	| operatorToken = (PLUS | MINUS) expression	# UnaryExpr
	| expression EXPONENT expression			# ExponentialExpr
	| expression operatorToken = (INC | DEC)	# IncDecExpr
	| MMIN LPAREN paramlist = arglist RPAREN	# MMinExpr
	| MMAX LPAREN paramlist = arglist RPAREN	# MMaxExpr
	| NUMBER									# NumberExpr
	| IDENTIFIER								# IdentifierExpr;

/*
 * Lexer Rules
 */

NUMBER: INT ('.' INT)?;
IDENTIFIER: [A-Z]+ [1-9][0-9]*;
INT: ('0' ..'9')+;
arglist: expression (',' expression)*;
EXPONENT: '^';
MINUS: '-';
PLUS: '+';
LPAREN: '(';
RPAREN: ')';
INC: '++';
DEC: '--';
MMAX: 'mmax';
MMIN: 'mmin';

// skip spaces, tabs, newlines
WS: [ \t\r\n] -> skip;