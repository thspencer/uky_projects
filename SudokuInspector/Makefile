# Author: Taylor Spencer
# Project: Sudoku Inspector
# CS375:  Discrete Mathematics II
# Submission Date: 03/17/2013

# link compiled object files and output executable
inspector: inspector.o
	gcc -o inspector inspector.o

inspector.o: inspector.c
	gcc -g -c -Wall -std=c99 inspector.c

# clean object files
clean:
	rm *.o

# clean object and executable files
cleanall:
	rm *.o
	rm inspector
	rm tests/*.cnf

