/*  VSH: Verbose Command Shell
 *
 *  Author:     Taylor Spencer
 *  CS485001:   Systems Programming
 *  Submitted:  03/26/2013
 */

int    CommandBuilder( Input*, char** );
int    InputPrompt( char** );
Input* ParseInput( Input* );
Input* ScanForMultiples( Input*, char* );
void   ParseCommand( Input* input );
void   ParseMeta( Input* );
void   ParseWord( Input* );
void   ParseString( Input* );
void   PrintTokens( Input* );
