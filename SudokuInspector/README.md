Sudoku Inspector
Author: Taylor Spencer
CS375:  Discrete Mathematics II

Sudoku Inspector encodes Sudoku solution files (assumes 60) into CNF format and feeds them to minisat for determining solvability.  Depending on the provided solutions the puzzle may be SOLVEABLE, UNSOLVABLE, or represent a TYPE-II error (multiple solutions found).

Results are printed on screen as well as in a file called "SUMMARY.TXT".

Building Sudoku Inspector:

    run 'make' inside the SudokuInspector directory
    use './inspector solutions_directory' to execute

Requirements to Use:

    minisat is required to properly execute the sudoku inspector. It is freely available from: http://minisat.se
