 /*
 * Author: Taylor Spencer
 * Project: Sudoku Inspector
 * CS375:  Discrete Mathematics II
 * Submission Date: 03/17/2013
 *
 * Reads input files and encodes them to CNF format
 * Will submit to minisat executable in system PATH
 * Outputs results to screen and file "SUMMARY.TXT"
 *
 *
 * CURRENT ISSUES:
 *    ONLY 40/60 SOLUTIONS ARE CORRECTLY IDENTIFIED
 *    1. does not properly implement the Type-II negation check
 *       meaning that solvable puzzles are identified correctly however
 *       Type-II errors also return solvable erroneously
 *
 *
 * OUTPUT: ( to SUMMARY.TXT in provided directory )
 *  UNSOLVABLE PUZZLES: 20
 *  SOLVABLE PUZZLES: 20
 *  TYPE-II PUZZLES: (should be 20)
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>

#define AUTHOR "Taylor Spencer"

/* LOGISTICAL DESCRIPTION OF SUDOKU GRID */
#define ROWS      9    /* number of rows in grid */
#define COLUMNS   9    /* number of columns in grid */
#define BLOCKS    9    /* number of blocks in grid */
#define MAXVAL    9    /* maximum value a tile can hold */
#define TERM_LINE 0    /* line termination in CNF file format */
#define CLAUSES   3159 /* number of clauses that will be defined */
#define VARIABLES 729  /* number of variables that will be defined */

/* FILE LAYOUT DESCRIPTION */
#define FILE_NUM  60         /* number of files to iterate through */
#define FILE_NAME "puzzle." /* basename */
#define IFILE_FMT  ".txt"   /* input filename extension */
#define OFILE_FMT  ".cnf"   /* cnf output file */

#define MINISAT "minisat" /* minisat executable, from ENVPATH */

/* OBJECT LENGTH MACRO */
#define retLen( obj )  sizeof( obj )/sizeof(*obj)

/* ARRAY INITIALIZER MACRO */
#define initArray( encoded, size ) memset( encoded, 0, size * sizeof( *encoded ))

unsigned int DEBUG = 0; /* INFO PRINTED: 0=NONE; 1=NORMAL; 2=VERBOSE */
unsigned int CLAUSE_CNT = 0; /* counter for clauses that are created */

int encodeInput( int row, int column, int value )
{
    /* ENCODING FORMULA:
     *    place(i; j; k) = [(i-1)*81 + (j-1)*9 + (k-1) + 1]
     */

    return ( 81*( row    - 1 ) +
              9*( column - 1 ) +
              1*( value  - 1 ) + 1 );
}

/* prints file descriptor at bottom of ouput file */
void printDescription( FILE *o_fp )
{
    char* varStr    = "c NUMBER OF VARIABLES:";
    char* clauseStr = "c NUMBER OF CLAUSES:";
    char* cnfHeader = "p cnf";

    fprintf(
        o_fp,
        "c %s\n%s %d\n%s %d\n%s %d %d\n\n",
        AUTHOR,
        varStr, VARIABLES,
        clauseStr, CLAUSE_CNT,
        cnfHeader,
        VARIABLES,
        CLAUSE_CNT );
}

/* print a single line to output file */
void printLine( FILE *o_fp, int *encoded, int cnt )
{
    int i;
    for ( i = 0; i < cnt; i++ ){
        fprintf( o_fp, "%d ", encoded[i] );
    }
    fprintf( o_fp, "\n" );
}

/* sets up all cell constraints */
void cellConstraints( FILE *o_fp )
{
    int cellArray[3]; /* array to hold cell constraint values passed to file output each iteration */
    int rPos; /* row position */
    int cPos; /* column position */
    int val;  /* current cell value */
    int cnt;  /* cell testing value */
    int clauses = 0;

    /* SET CELL CONSTRAINTS */
    for ( rPos = 1; rPos <= ROWS; rPos++ ){

        for ( cPos = 1; cPos <= COLUMNS; cPos++ ){

            for ( val = 1; val <= MAXVAL; val++ ){

                for( cnt = 1; cnt <= MAXVAL; cnt++ ){

                    if ( cnt > val ){
                            cellArray[0] = -encodeInput( rPos, cPos, val );
                            cellArray[1] = -encodeInput( rPos, cPos, cnt );
                            cellArray[2] = 0;
                            clauses++;

                        if ( DEBUG >= 2 ){
                            printf("%d:%d:%d\n", rPos, cPos, val );
                            printf("%d:%d:%d\n", rPos, cPos, cnt );
                            printf("%d:%d:%d\n\n", cellArray[0], cellArray[1], cellArray[2] );
                        }

                        printLine( o_fp, cellArray, 3 );
                    }
                }
            } /* END COLUMN LOOP */
        } /* END COLUMN LOOP */
    } /* END ROW LOOP */

    CLAUSE_CNT += clauses;

    if ( DEBUG != 0 ){
        printf("\t*DEBUG* CLAUSES OF L(2): %d\n", clauses );
    }
}

/* row, column, block constraints */
void rcbConstraints( FILE *o_fp )
{
    int rcbArray[10]; /* array to hold constraint values passed to file output each iteration */
    int arrayCnt;
    int rPos; /* row position */
    int cPos; /* column position */
    int blck; /* current block */
    int val;  /* current cell value */
    int rCap = 3; /* initial row cap in block loop */
    int cCap = 3; /* initial column cap in block loop */
    int clauses = 0;

    /* SET ROW CONSTRAINTS */
    for ( rPos = 1; rPos <= ROWS; rPos++ ){

        for ( val = 1; val <= MAXVAL; val++ ){

            arrayCnt = 0;
            initArray( rcbArray, retLen( rcbArray ) );

            for ( cPos = 1; cPos <= COLUMNS; cPos++ ){
                rcbArray[arrayCnt] = encodeInput( rPos, cPos, val );

                if ( DEBUG >= 2 ){
                    printf("%d:%d:%d\t%d\n", rPos, cPos, val, rcbArray[arrayCnt] );
                }

                arrayCnt++;
            } /* END COLUMN LOOP */
        rcbArray[arrayCnt++] = 0;
        clauses++;

        printLine( o_fp, rcbArray, arrayCnt );
        } /* END COLUMN LOOP */
    } /* END ROW LOOP */

    /* SET COLUMN CONSTRAINTS */
    for ( cPos = 1; cPos <= COLUMNS; cPos++ ){

        for ( val = 1; val <= MAXVAL; val++ ){

            arrayCnt = 0;
            initArray( rcbArray, retLen( rcbArray ) );

            for ( rPos = 1; rPos <= ROWS; rPos++ ){
                rcbArray[arrayCnt] = encodeInput( rPos, cPos, val );

                if ( DEBUG >= 2 ){
                    printf("%d:%d:%d\t%d\n", rPos, cPos, val, rcbArray[arrayCnt] );
                }

                arrayCnt++;
            } /* END ROW LOOP */
        rcbArray[arrayCnt++] = 0;
        clauses++;

        printLine( o_fp, rcbArray, arrayCnt );
        } /* END VALUE LOOP */
    } /* END COLUMN LOOP */

    /* SET BLOCK CONSTRAINTS */
    for ( blck = 1; blck <= BLOCKS; blck++ ){
        /* value is incremented after every cell in block is mapped */
        for ( val = 1; val <= MAXVAL; val++ ){
                arrayCnt = 0;
                initArray( rcbArray, 10 );
            /* row is incremented after reaching 3rd column in block */
            /* row is incremented by 3 after reaching last column of last block in row */
            for ( rPos = rCap - 2; rPos <= rCap; rPos++ ){

                /* column is incremented every iteration to max of 3rd column in block */
                /* column is reset after reaching last column of last block in row */
                for ( cPos = cCap - 2; cPos <= cCap; cPos++ ){

                    rcbArray[arrayCnt] = encodeInput( rPos, cPos, val );

                    if ( DEBUG >= 2 ){
                        printf("%d:%d:%d\t%d\n", rPos, cPos, val, rcbArray[arrayCnt] );
                    }

                    arrayCnt++;
                } /* END COLUMN LOOP */
            } /* END ROW LOOP */

            rcbArray[arrayCnt++] = 0;
            clauses++;

            printLine( o_fp, rcbArray, arrayCnt );
        } /* END VALUE LOOP */

        /* increment column cap by 3 for next block */
        if ( cCap <= (COLUMNS - 3) ){
            cCap += 3;
        }
        /* increment rCap by 3 and reset cCap to 3 when block 3,6 or 9 */
        if ( blck % 3 == 0   &&
             cCap == COLUMNS &&
             rCap <= ( ROWS - 3 )){

            rCap += 3;
            cCap = 3;
        }
    } /* END BLOCK LOOP */

    CLAUSE_CNT += clauses;

    if ( DEBUG != 0 ){
        printf("\t*DEBUG* CLAUSES OF L(9): %d\n", clauses );
    }
}

/* read solutions from input files and send to encoder */
void processSolutions( FILE *i_fp, FILE *o_fp, int* solutions )
{
    char c;
    int solutionsCnt = 0;
    int encodedSol[2];
    int segment[3];
    int i = 0;

    /* grab single char per iteration and test */
    while (( c = fgetc( i_fp )) != EOF ){
        encodedSol[0] = 0;
        encodedSol[1] = 0;

        if ( c >= '0' && c <= '9' ){
            if ( i < 2 ){
                segment[i++] = c - '0';
            } else if ( i == 2 ){
                segment[i] = c - '0';

                encodedSol[0] = encodeInput( segment[0], segment[1], segment[2] );

                if ( DEBUG >= 2 ){
                    printf("%d:%d:%d\t%d\n", segment[0], segment[1], segment[2], encodedSol[0] );
                }

                printLine( o_fp, encodedSol, 2 );
                solutions[ solutionsCnt++ ] = encodedSol[0];

                i = 0; /* reset counter */
            } else {
                printf("Unknown Error\n");
                exit(0);
            }
        }
    }
    CLAUSE_CNT += solutionsCnt;

    if ( DEBUG != 0 ){
        printf("\t*DEBUG* PROVIDED SOLUTIONS: %d\n", solutionsCnt );
    }
}

/* forks process and executes minisat */
int executeSolver( char* cmd, char* O_FNAME )
{
    int child_status = 0;

    if ( fork() == 0 ){

        FILE* pipe_p = popen( cmd, "r" );
        char buffer[255];

        if ( pipe_p == 0 ){
            printf("Error executing minisat with file: %s\n", O_FNAME );
            exit(0);
        }

        while ( fgets(buffer, sizeof( buffer ), pipe_p ) != NULL ){

            if ( DEBUG >= 2 ){
                printf("\n\t*DEBUG* MINISAT OUTPUT: %s\n", buffer );
            }

            if ( strcmp( buffer, "SATISFIABLE\n" ) == 0 ){
                _exit(1);

            } else if ( strcmp( buffer, "UNSATISFIABLE\n" ) == 0 ){
                _exit(0);
            } else {
                printf("minisat returned an unknown response:\n\t%s\n", buffer );
                _exit(-1);
            }
        }

        fclose( pipe_p );

    } else {
        wait( &child_status ); /* block until child terminates */

        if ( child_status == 0 ){
            return 0; /* UNSATISFIABLE */

        } else if ( child_status == 1 || ( child_status >> 8 ) == 1){
            /* sometimes the exit code needs to be bit-shifted by 8 */
            return 1; /* SATISFIABLE */
        }
    }

    return -1; /* should not see this unless return is abnormal */
}

/* setup entire output file */
void createDIMACS( char* I_FNAME, char* O_FNAME, int* solutions, int testSol )
{
    FILE *i_fp = fopen( I_FNAME, "r");
    FILE *o_fp = fopen( O_FNAME, "w+");

    CLAUSE_CNT = 0; /* reset global clause counters */

    if ( i_fp == 0 || o_fp == 0 ){
        printf("\nCould not open file: %s\n...exiting\n", I_FNAME );
        exit(0);

    } else {
        rcbConstraints( o_fp );   /* set default row/column/block clauses */
        cellConstraints( o_fp );  /* set default cell clauses */
        processSolutions( i_fp, o_fp, solutions ); /* process file */

        /* add negation line for testing TypeII Errors */
        if ( testSol >= 0 ){

            CLAUSE_CNT++;
            fprintf( o_fp, "-%d %d\n", solutions[ testSol ], 0 );

            if ( DEBUG != 0 ){
                printf("\t*DEBUG* ADDED NEGATION: -%d\n\n", solutions[ testSol ] );
            }
        }

        printDescription( o_fp ); /* print cnf description (at bottom) */

        /* close file handle before calling minisat */
        fclose(i_fp);
        fclose(o_fp);

        if ( DEBUG != 0 ){
            printf("\t*DEBUG* TOTAL CLAUSES: %d\n", CLAUSE_CNT );
        }
    }
}

int main( int argc, char **argv )
{
    if ( argc < 2 ){
        printf("\tUSAGE: %s solutions_directory debug_level(0-2)\n", argv[0] );
        exit(0);

    } else {
        FILE* s_fp;

        int cnt = 0;

        int sumSolvable   = 0;  /* solution type counters */
        int sumUnsolvable = 0;
        int sumTypeII     = 0;

        char DIR_NAME[255];
        char S_FNAME[255]; /* summary filename */


        /* set solutions path */
        snprintf( DIR_NAME, sizeof DIR_NAME, "%s", argv[1] );
        snprintf( S_FNAME, sizeof S_FNAME, "%s/%s", DIR_NAME, "SUMMARY.txt");

        s_fp = fopen( S_FNAME, "w" ); /* summary file */;

        /* set debug level */
        if ( argc >= 3 ){
            DEBUG = atoi( argv[2] );
        }

        /* loop through all files ( 60 by default) */
        for ( ; cnt < FILE_NUM; cnt++ ){

            int solutions[ ROWS * COLUMNS ];
            int result;

            char I_FNAME[255]; /* input filename */
            char O_FNAME[255]; /* output filename */
            char CMD[255];     /* command string to execute minisat */

            /* initialize solutions array */
            initArray( solutions, retLen( solutions ) );

            snprintf(
                I_FNAME,
                sizeof I_FNAME,
                "%s/%s%d%s",
                DIR_NAME,
                FILE_NAME,
                cnt+1,
                IFILE_FMT );
            snprintf(
                O_FNAME,
                sizeof I_FNAME,
                "%s/%s%d%s",
                DIR_NAME,
                FILE_NAME,
                cnt+1,
                OFILE_FMT );
            snprintf(
                CMD,
                sizeof CMD,
                "%s -verb=0 %s 2>/dev/null",
                MINISAT,
                O_FNAME );

            /* create cnf file */
            createDIMACS( I_FNAME, O_FNAME, solutions, -1 );

            /* initial run of minisat */
            result = executeSolver( CMD, O_FNAME );

            if ( result == -1 ){
                exit(1); /* fatal error */
            }

            fprintf( s_fp, "PROBLEM %d IS: \t", cnt+1 );
            printf("PROBLEM %d IS: \t", cnt+1 );

            /* lets make sure the puzzle does not have multiple solutions */
            if ( result == 0 ){
                sumUnsolvable++;
                fprintf( s_fp, "UNSOLVABLE\n" );
                printf("UNSOLVABLE\n" );

            } else if ( result == 1 ){
                int solNum = 1;

                if ( DEBUG != 0 ){
                    printf("\n\t*DEBUG* START TYPE-II ERROR CHECKING\n\n");
                }

                for(int i = 0; i < retLen( solutions ) && solutions[i] != 0; i++ ){
                    /* re-initialize solutions array */
                    initArray( solutions, retLen( solutions ) );

                    createDIMACS( I_FNAME, O_FNAME, solutions, i );

                    result = executeSolver( CMD, O_FNAME );

                    if ( result == 1 ){
                        solNum++;
                    }

                    if ( DEBUG != 0 ){
                        printf("\n\t*DEBUG* TYPE-II RESULT: %d...enter to continue\n", result );
                        printf("\t*DEBUG* SOLUTIONS INDEX: %d\n", i );
                    }
                }

                /* all iterations of TypeII test (except first) should be UNSATISFIABLE */
                if ( solNum == 1 ){
                    sumSolvable++;
                    fprintf( s_fp, "WELL-DESIGNED\n" );
                    printf("WELL-DESIGNED\n" );
                } else {
                    sumTypeII++;  /* puzzle had multiple solutions */
                    fprintf( s_fp, "HAS TYPE-II FLAW " );
                    fprintf( s_fp, "AND IS SATISFIABLE WITH %d SOLUTIONS\n", solNum );

                    printf("HAS TYPE-II FLAW " );
                    printf("AND IS SATISFIABLE WITH %d SOLUTIONS\n", solNum );
                }
            }
        } /* end main for loop */

        printf("\n\t/*** SUMMARY ***/\n\n");
        printf("\tUNSOLVABLE PUZZLES: %d\n", sumUnsolvable );
        printf("\tSOLVABLE PUZZLES: %d\n", sumSolvable );
        printf("\tTYPE-II PUZZLES: %d\n", sumTypeII );
        printf("\n\t/***************/\n\n" );

        fclose( s_fp );
    } /* end else */
    return 0;
}

