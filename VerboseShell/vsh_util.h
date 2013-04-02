/*  VSH: Verbose Command Shell
 *
 *  Author:     Taylor Spencer
 *  CS485001:   Systems Programming
 *  Submitted:  03/26/2013
 */

#include <signal.h>  /* signal handling */
#include <setjmp.h>  /* jmp_buf */

#define DEFAULT_PROMPT "vsh %"
#define BUFFER_LENGTH  255

typedef struct Input Input;
typedef struct Variable Var;
typedef int    _bool;

/* input structure to store data and token types */
struct Input{
    char command[BUFFER_LENGTH];    /* the actual input command */
    char tokenType[15];             /* the token type for this sequence */
    char tokenValue[BUFFER_LENGTH]; /* the token value for this sequence */
    char append_L[BUFFER_LENGTH];
    char append_R[BUFFER_LENGTH];

    _bool isRoot;
    _bool isCMD;
    _bool parseError;

    unsigned int argc;

    Input* next; /* next sequence of input */
};

struct Variable{
    char*  name[BUFFER_LENGTH];
    char* value[BUFFER_LENGTH];

    int count;

    Var* next;
};

Input* CreateInputNode();
Var*   CreateVarNode();
void   FreeInput( Input* );
void   FreeVars( Var* );
void   ExitShell( int );
void   FaultHandler( int ); /* faults handler for signals */

#ifndef NULL
    #define NULL 0
#else
    #undef  NULL
    #define NULL 0
#endif

#ifndef TRUE
    #define TRUE 1
#else
    #undef  TRUE
    #define TRUE 1
#endif

#ifndef FALSE
    #define FALSE 0
#else
    #undef  FALSE
    #define FALSE 0
#endif

/* ARRAY LENGTH MACRO */
#define retLen( obj )  sizeof( obj ) / sizeof(*obj)

/* ARRAY INITIALIZER MACRO */
#define initArray( encoded, size, value ) memset( encoded, value, size * sizeof( *encoded ))

jmp_buf buf;
int EXIT_STATUS;
int DEBUG_LEVEL;
_bool echo;
_bool parse;
_bool child;
