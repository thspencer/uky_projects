/*  VSH: Verbose Command Shell
 *
 *  Author:     Taylor Spencer
 *  CS485001:   Systems Programming
 *  Submitted:  03/26/2013
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h> /* memset */

#include "vsh_util.h"

Input* CreateInputNode()
{
    Input* node = malloc( sizeof( Input ));

    node->argc = 0;
    node->next = NULL;

    node->isRoot     = FALSE;
    node->isCMD      = FALSE;
    node->parseError = FALSE;

    initArray( node->command,    retLen( node->command ),    NULL );
    initArray( node->tokenType,  retLen( node->tokenType ),  NULL );
    initArray( node->tokenValue, retLen( node->tokenValue ), NULL );
    initArray( node->append_L,   retLen( node->append_L ),   NULL );
    initArray( node->append_R,   retLen( node->append_R ),   NULL );

    return node;
}

Var* CreateVarNode()
{
    Var* node = malloc( sizeof( Var ));

    //memset( node->name, 0, sizeof(char*) * ( BUFFER_LENGTH ) );
    //memset( node->value, 0, sizeof(char*) * ( BUFFER_LENGTH ) );

    node->count = 0;

    node->next = NULL;

    return node;
}

/* free*() iterates through nodes to free existing objects */
void FreeInput( Input* rootNode )
{
    Input* node_ptr;

    while ( rootNode != NULL ){
        node_ptr = rootNode;
        rootNode = rootNode->next;
        free( node_ptr );
    }
}

void FreeVars( Var* rootNode )
{
    Var* node_ptr;

    while ( rootNode != NULL ){
        node_ptr = rootNode;
        rootNode = rootNode->next;
        free( node_ptr );
    }
}

void ExitShell( int code )
{
    if ( code == -1 ){
        printf("\tVSH shell unable to recover, exiting");

    } else if ( code != 0 && code != 1 ){
        printf("\tWARNING: VSH shell is exiting abnormally with status code: %d", code );

    } else if ( DEBUG_LEVEL > 0 ){
        printf("\n\t*DEBUG*\tSHELL EXITING WITH STATUS CODE: %d", code );

    }

    printf("\n");
    exit( code );
}

void FaultHandler( int signal )
{
    switch( signal ){

        case SIGSEGV:
            puts("\n\tERROR: segmentation fault detected, attempting recovery\n");
            longjmp(buf, 1);
            break;

        case SIGBUS:
            puts("\tERROR: illegal error detected, attempting recovery\n");
            longjmp(buf, 1);
            break;

        case SIGALRM:
            puts("\tERROR: race condition detected, attempting recovery\n");
            longjmp(buf, 1);
            break;

        case SIGILL:
            puts("\tERROR: illegal error detected, attempting recovery\n");
            longjmp(buf, 1);
            break;

        default:
            puts("\tERROR: fatal exception detected\n");
    }

    ExitShell(-1);
}
