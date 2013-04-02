/*  VSH: Verbose Command Shell
 *
 *  Author:     Taylor Spencer
 *  CS485001:   Systems Programming
 *  Submitted:  03/26/2013
 */

#include <stdio.h>
#include <stdlib.h> /* exit */
#include <string.h>
#include <ctype.h> /* test for whitespace */

#include "vsh_util.h"
#include "vsh_parser.h"

/* build a character array combining the parsed input data */
int CommandBuilder( Input* input, char** parsedCMD )
{
    /* store the root node */
    Input* ptr = input;

    int args   = 0;
    int indexC = 0; /* commands index */

    /* initialize string array to store command */
    memset( parsedCMD, 0, sizeof(char*) * ( BUFFER_LENGTH ) );

    /* find number of nodes in list */
    while ( input != NULL ){
        args++;
        input = input->next;
    }

    /* restore root node pointer */
    input = ptr;

    if ( input->command[0] == NULL ){
        return 0;
    }

    while ( input != NULL ){

        /* test for parse errors, return nothing if error */
        if ( input->parseError ){
            return 0;
        }

        parsedCMD[indexC] = input->tokenValue;

        /* null terminate end of string */
        parsedCMD[indexC][BUFFER_LENGTH-1] = '\0';

        indexC++;

        /* set next node */
        input = input->next;
    }

    ptr->argc = args;
    return args;
}

int InputPrompt( char** parsedCMD )
{
    int index = 0;
    int args  = 0;

    Input* currentInput  = CreateInputNode();
    currentInput->isRoot = TRUE;

    do {
        currentInput->command[index] = getchar();

        /* command is complete once newline received*/
        if ( currentInput->command[index] == '\n' ||
             currentInput->command[index] == EOF ){

            /* send input to parser */
            currentInput = ParseInput( currentInput );

            /* build command from parsed tokens */
            args = CommandBuilder( currentInput, parsedCMD );

            if ( DEBUG_LEVEL > 0 ||
                 parse ){

                PrintTokens( currentInput );
            }

            /* free input list */
            FreeInput( currentInput );

            /* return command list to execute */
            return args;
        }
    } while ( index++ < BUFFER_LENGTH );

    return args;
}

Input* ParseInput( Input* input )
{
    Input* current  = CreateInputNode();
    Input* root_ptr = current;

    int indexRoot = 0;
    int indexCurr = 0;

    current->isRoot = TRUE; /* set root node, only once */

    while ( indexRoot < BUFFER_LENGTH &&
            indexCurr < BUFFER_LENGTH &&
            input->command[indexRoot] != NULL ){

        /* create next input node on encountering a space AND more input exists*/
        if ( isspace( input->command[indexRoot] )){

            /* reset counter */
            indexRoot++;
            indexCurr = 0;

            if ( input->command[indexRoot] != NULL ){
                Input* next = CreateInputNode();

                /* update next pointer */
                current->next = next;

                /* move current to next */
                current = current->next;
            }
        } else if ( input->command[indexRoot] == '\"' ){
            /* assume there is a string close if the open exists */
            _bool closureFound = FALSE;

            /* split input for string data */
            if ( indexCurr != 0 ){
                /* create new node as start of string */
                Input* next = CreateInputNode();

                /* update next pointer */
                current->next = next;

                /* move current to next */
                current = current->next;

                /* reset counter */
                indexCurr = 0;
            }

            do {
                current->command[indexCurr] = input->command[indexRoot];

                /* increment before loop is terminated */
                indexRoot++;

                if ( current->command[indexCurr] == '\"' &&
                     indexCurr != 0 ){

                    closureFound = TRUE;
                    indexCurr = 0;
                    break; /* exit loop when string closure found */
                }

                indexCurr++;
            } while (( indexRoot < BUFFER_LENGTH &&
                       indexCurr < BUFFER_LENGTH ) &&
                       input->command[indexRoot] != '\n' );

            if ( !closureFound ){
                printf("\tERROR: string was not properly closed: %s\n", current->command);
                current->parseError = TRUE;
                break;
            }

            if ( input->command[indexRoot] !=  NULL &&
                 input->command[indexRoot] != '\n' ){

                /* increment input counter if next character is whitespace */
                if ( isspace(input->command[indexRoot]) ){
                    indexRoot++;
                }

                /* create new node for next input */
                Input* next = CreateInputNode();
                /* update next pointer */
                current->next = next;

                /* move current to next */
                current = current->next;

                /* reset counter */
                indexCurr = 0;
            }
        } else {
            /* assign the character input to current command seqence */
            current->command[indexCurr++] = input->command[indexRoot++];
        }
    }

    ParseString( root_ptr );
    ParseCommand( root_ptr );
    ParseMeta( root_ptr );
    ParseWord( root_ptr );

    return root_ptr; /* return the first command struct */
}

void ParseString( Input* input )
{
    while ( input != NULL ){

        if ( input->command[0] == '\"' ){
            char str[] = { "string" };

            int index;

            for ( index = 1; index < BUFFER_LENGTH && input->command[index] != NULL; index++ ){

                if ( input->command[index] == '\"' ){
                    snprintf( input->tokenType, sizeof input->tokenType, "%s", str );
                    snprintf( input->tokenValue, sizeof input->tokenValue, "%s", input->command );
                    break;
                }
            }
        }

        /* set next node */
        input = input->next;
    }
}

/* assign word token type to all values that have not been assigned */
void ParseWord( Input* input )
{
    char word[] = { "word" };

    while ( input != NULL ){

        /* check if token has already been set */
        if ( input->tokenType[0] != NULL ){
            /* set next node */
            input = input->next;

            continue;
        }

        snprintf( input->tokenType, sizeof input->tokenType, "%s", word );
        snprintf( input->tokenValue, sizeof input->tokenValue, "%s", input->command );

        /* set next node */
        input = input->next;
    }
}

void ParseCommand( Input* input )
{
    while ( input != NULL ){

        /* check if token has already been set */
        if ( input->tokenType[0] != NULL ){
            /* set next node */
            input = input->next;

            continue;
        }

        char* token = input->command;

        if ( input->isRoot == TRUE &&
           ( strcmp( token, "setvar" )    == 0 ||
             strcmp( token, "setprompt" ) == 0 ||
             strcmp( token, "echocmd" )   == 0 ||
             strcmp( token, "parsecmd" )  == 0 ||
             strcmp( token, "showchild" ) == 0 ||
             strcmp( token, "cd" )        == 0 ||
             strcmp( token, "exit" )      == 0 )){

            snprintf( input->tokenValue, sizeof input->tokenValue, "%s", token );

            input->isCMD = TRUE;
        }

        /* set next node */
        input = input->next;
    }
}

void ParseMeta( Input* input )
{
    char meta[] = { "metachar" };
    char eol[] = { "end-of-line" };
    char com[] = { "%" };
    char var[] = { "variable" };

    while ( input != NULL ){
        int index;

        /* check if token has already been set */
        if ( input->tokenType[0] != NULL ){
            /* set next node */
            input = input->next;

            continue;
        }

        /* must be at beginning of line */
        if ( input->isRoot == TRUE && input->command[0] == '%' ){
            snprintf( input->tokenType,  sizeof input->tokenType,  "%s", meta );
            snprintf( input->tokenValue, sizeof input->tokenValue, "%s", com );
            snprintf( input->append_R,   sizeof input->append_R,   "%s", strtok( input->command, com ));
        }

        /* must be first character of sequence */

        /* WORK IN PROGRESS */
        if ( input->command[0] == '$' ){
            int index = 0;

            snprintf( input->tokenType,  sizeof input->tokenType,  "%s", var );
            snprintf( input->tokenValue, sizeof input->tokenValue, "%s", input->command );
            snprintf( input->append_R,   sizeof input->append_R,   "%s", strtok( input->command, "$" ));

            while ( input->append_R[index] != NULL ){
                if ( isalpha( input->append_R[index] ) == 0 ){
                    printf("\tERROR: variable contains invalid characters\n");

                    snprintf( input->tokenType,  sizeof input->tokenType,  "word" );
                    snprintf( input->tokenValue, sizeof input->tokenValue, "%s", input->command );
                }

                index++;
            }
        }

        for ( index = 0; input->command[index] != '\0'; index++ ){

            if ( input->command[index] == EOF ){
                snprintf( input->command,    sizeof input->command, "exit" );
                snprintf( input->tokenType,  sizeof input->tokenType, "end-of-file" );
                snprintf( input->tokenValue, sizeof input->tokenValue, "EOF" );
            }

            if ( input->command[index] == '\n' ){
                snprintf( input->tokenType,  sizeof input->tokenType, "%s", eol );
                snprintf( input->tokenValue, sizeof input->tokenValue, "EOL" );
            }

            if ( input->command[index] == '|' ){
                snprintf( input->tokenType,  sizeof input->tokenType,  "%s", meta );
                snprintf( input->tokenValue, sizeof input->tokenValue, "|" );
                snprintf( input->append_L,   sizeof input->append_L,   "%s", strtok( input->command, "|" ));
                snprintf( input->append_R,   sizeof input->append_R,   "%s", strtok( NULL, "|" ));
            }

            if ( input->command[index] == '>' ){
                snprintf( input->tokenType,  sizeof input->tokenType,  "%s", meta );
                snprintf( input->tokenValue, sizeof input->tokenValue, " %s ", input->command );
                snprintf( input->append_L,   sizeof input->append_L,   "%s", strtok( input->command, ">" ));
                snprintf( input->append_R,   sizeof input->append_R,   "%s", strtok( NULL, ">" ));
            }

            if ( input->command[index] == '<' ){
                snprintf( input->tokenType,  sizeof input->tokenType,  "%s", meta );
                snprintf( input->tokenValue, sizeof input->tokenValue, "<" );
                snprintf( input->append_L,   sizeof input->append_L,   "%s", strtok( input->command, "<" ));
                snprintf( input->append_R,   sizeof input->append_R,   "%s", strtok( NULL, "<" ));
            }

            if ( input->command[index] == '&' ){
                snprintf( input->tokenType,  sizeof input->tokenType,  "%s", meta );
                snprintf( input->tokenValue, sizeof input->tokenValue, "&" );
                snprintf( input->append_L,   sizeof input->append_L,   "%s", strtok( input->command, "&" ));
                snprintf( input->append_R,   sizeof input->append_R,   "%s", strtok( NULL, "&" ));
            }
        }

        /* set next node */
        input = input->next;
    }
}

/* prints debug output of processed tokens */
void PrintTokens( Input* input )
{
    int argc = 0;

    while ( input != NULL ){

        if ( input->isRoot ){
            printf("ARGUMENTS: %d\n", input->argc+1);
        }

        if ( DEBUG_LEVEL > 0 ){
            printf("SEQUENCE: %s\n", input->command );
        }

        /* print token info and concatenate if split */
        printf("TOKEN TYPE: %s\t", input->tokenType );
        printf("TOKEN: %s%s%s \t", input->append_L, input->tokenValue, input->append_R );

        /* determine what token is used for */
        if ( input->isCMD ){
            printf("USAGE: %s\n", input->tokenValue );

        } else if ( strcmp ( input->tokenType, "word" ) != 0 ){

            if ( strcmp ( input->tokenType, "end-of-file" ) == 0 ){
                printf("USAGE: exit" );

            } else {
                printf("USAGE: %s", input->tokenType );
            }
        } else if ( input->isRoot ){
            printf("USAGE: cmd\n" );

        } else {
            printf("USAGE: arg %d\n", argc );
        }

        argc++;

        /* set next node */
        input = input->next;
    }

    printf("\n"); /* formatting */
}