/*  VSH: Verbose Command Shell
 *
 *  Author:     Taylor Spencer
 *  CS485001:   Systems Programming
 *  Submitted:  03/26/2013
 */

/*  CURRENT ISSUES:
 *     1. Variables replacement not currently implemented
 *     2. Token parsing is incomplete
 */

/*  TODO:
 *     1. Implement piping
 *     2. Implement redirection
 */

#include <stdio.h>
#include <stdlib.h>
#include <termios.h>   /* catch control^D exits */
#include <string.h>
#include <unistd.h>    /* process control */
#include <sys/wait.h>
#include <sys/types.h> /* pid_t */

#include "vsh_util.h"
#include "vsh_parser.h"

void  InitShell( char** );
void  SetPrompt( char* );
void  ExecuteExternal( char**, int );
_bool ExecuteInternal( char**, char **);

static unsigned int resets = 0;
static const int loopTimer = 600; /* timer to catch loops */

char PROMPT[BUFFER_LENGTH];

int main( int argc, char** argv )
{
    char*  varlist[255];
    char*  command[BUFFER_LENGTH];
    int    argCount = 1;

    signal( SIGSEGV, FaultHandler );
    signal( SIGBUS,  FaultHandler );
    signal( SIGALRM, FaultHandler );
    signal( SIGILL,  FaultHandler );

    alarm( loopTimer );

    EXIT_STATUS = 0;

    if ( argc > 1 ){
        DEBUG_LEVEL = atoi( argv[1] );
    } else {
        DEBUG_LEVEL = 0;
    }

    if ( setjmp( buf )) {
        resets++;

        if (resets > 2 ){
            ExitShell(-1);
        }

        printf("\tWARNING: recovered from an exception\n");
    }

    /* set default values */
    InitShell( command );

    while ( EXIT_STATUS != 1 ){
        /* prompt only if input is from STDIN */
        if( isatty( STDIN_FILENO )){
            /* display prompt */
            printf("%s", PROMPT);
        }

        /* get input and parse command */
        argCount = InputPrompt( command );

        /* make sure returned command is not empty and command is not internally recognized */
        if ( argCount ){

            if ( !ExecuteInternal( command, varlist )) {

                ExecuteExternal( command, argCount );
            }
        }
    }

    ExitShell( EXIT_STATUS );
}

_bool ExecuteInternal( char** command, char** varlist )
{
    _bool isShellCMD = FALSE;

    /* is a comment, ignore following */
    if ( command[0][0] == '%' ){
        isShellCMD = TRUE;
        return isShellCMD;
    }

    if ( strcmp( command[0], "setvar" ) == 0 ){
        isShellCMD = TRUE;

        if ( command[1] == NULL ||
             command[2] == NULL ){

            printf("\tUnable to set variable, missing argument\n");
            return isShellCMD;
        }

        //snprintf( *varlist[0], sizeof(char) * ( BUFFER_LENGTH ),  "%s", command[1] );
        //snprintf( varlist->value[index], sizeof varlist->value[index], "%s", command[2] );
    }

    /* set prompt string */
    if ( strcmp( command[0], "setprompt" ) == 0 ){
        isShellCMD = TRUE;

        if ( command[1] == NULL ){
            printf("\tUnable to set prompt, missing argument\n");
            return isShellCMD;
        }

        SetPrompt( command[1] );
    }

    if ( strcmp( command[0], "echocmd" ) == 0 ){
        isShellCMD = TRUE;

        if ( command[1] == NULL ){
            printf("\tUnable to set echocmd, missing argument\n");
            return isShellCMD;
        }

        if ( strcmp( command[1], "on" ) == 0 ){
            echo = TRUE;

            printf("\tcommand echoing enabled\n");
        }

        if ( strcmp( command[1], "off" ) == 0 ){
            echo = FALSE;

            printf("\tcommand echoing disabled\n");
        }
    }

    if ( strcmp( command[0], "parsecmd" ) == 0 ){
        isShellCMD = TRUE;

        if ( command[1] == NULL ){
            printf("\tUnable to set parsecmd, missing argument\n");
            return isShellCMD;
        }

        if ( strcmp( command[1], "on" ) == 0 ){
            parse = TRUE;

            printf("\tcommand parsing enabled\n");
        }

        if ( strcmp( command[1], "off" ) == 0 ){
            parse = FALSE;

            printf("\tcommand parsing disabled\n");
        }
    }

    if ( strcmp( command[0], "showchild" ) == 0 ){
        isShellCMD = TRUE;

        if ( command[1] == NULL ){
            printf("\tUnable to set showchild, missing argument\n");
            return isShellCMD;
        }

        if ( strcmp( command[1], "on" ) == 0 ){
            child = TRUE;

            printf("\tchild info enabled\n");
        }

        if ( strcmp( command[1], "off" ) == 0 ){
            child = FALSE;

            printf("\tchild info disabled\n");
        }
    }

    if ( strcmp( command[0], "cd" ) == 0 ){
        isShellCMD = TRUE;

        if ( command[1] == NULL ){
            printf("\tUnable to change directory, missing destination\n");
            return isShellCMD;
        }

        if ( chdir( command[1] ) == 0 ){
            if ( DEBUG_LEVEL > 0 ){
                printf("\tDIRECTORY CHANGED TO: %s\n", command[1] );
            }
        } else {
            printf("\tERROR: directory change failed\n");
        }
    }

    if ( strcmp( command[0], "exit" ) == 0 ||
         strcmp( command[0], "EOF" ) == 0 ){

        isShellCMD = TRUE;
        EXIT_STATUS = 1;
    }

    if ( DEBUG_LEVEL > 0 && isShellCMD ){
        printf("SHELL COMMAND EXECUTED: %s\n", command[0]);
    }

    return isShellCMD;
}

void ExecuteExternal( char** command, int argCount )
{
    pid_t pid_c; /* child process pid */
    int   child_status = 0;

    if (( pid_c = fork() ) < 0 ){

        printf("\tERROR: failed to fork process: %s\n", command[0]);
        exit(1);

    } else if ( pid_c == 0 ){

        if ( DEBUG_LEVEL > 0  ||
             echo ){

            int i;

            printf("\nExecuting: ");
            for ( i = 0; i < argCount; i++ ){
                printf("%s ", command[i]);
            }
            printf("\n");
        }

        /* execute command */
        execvp( command[0], command );

        /* catch execution errors */
        perror("\tERROR: execution failed");

        exit(1); /* error if child did not return */
    } else {
        while ( wait( &child_status ) != pid_c ){
            ; /* do nothing */
        }

        if ( DEBUG_LEVEL > 0 ||
             child ){

            printf("\n");
            printf("\tchild process id is %d\n", pid_c );
            /* bitshift to ensure code is printed */
            printf("\tchild returned an exit status of: %d\n", child_status >> 8 );
        }
    }
}

/* set shell defaults */
void InitShell( char** parsedCMD )
{
    /* shell command values */
    echo  = FALSE;
    parse = FALSE;
    child = FALSE;

    SetPrompt( DEFAULT_PROMPT );

    if ( DEBUG_LEVEL > 0 ){
        printf("\t*DEBUG ENABLED(%d)*\n\n", DEBUG_LEVEL );
    }
}

/* set prompt value */
void SetPrompt( char* newPrompt )
{
   snprintf( PROMPT, sizeof( PROMPT ), "%s ", newPrompt );
}

