/*	Title:		cs216program5_studentRecord.h
 *	Date:		11/29/2012
 *	Function:	studentRecord class specification
 */

#ifndef STUDENTRECORD_H
#define STUDENTRECORD_H

#include <iostream>
/* dynamic list structure */
#include <list>
#include <fstream>
/* exit function */
#include <stdlib.h>
/* stringstream */
#include <sstream>

using namespace std;

/* global return code to track execution status */
extern signed int RET_CODE;

class StudentRecord{
private:
	/* structure that is created for each node in list; 
	 * an overload function is included within the struct so that the list
	 * member is able to access the data and sort as necessary
	 */
	struct StudentNode{
		/* input data storage */
		string id;
		string last;
		string first;
		string major;
		string stClass;

		/* overload function to compare node values;
		 * second node is a const reference, current node is directly accessed
		 * returns 1 if second node has an id value less than the current node
		 */
		int operator<( const StudentNode &rhs ) const 
		{
			if ( id < rhs.id ){ return 1; }
			else { return 0; }
		}

	}; /* end struct */

	/* student list using StudentNode structure */
    list<StudentNode> slist;
    /* list iterator */
    list<StudentNode>::iterator current;

	/* list index marker */
	unsigned int index;
	/* name of student data file */
	const char *STUDENTFILE;

public:
	/* Constructor:
	 *		when object is created the constructor will act as a data parser,
	 *		reading all data from input file and storing them into nodes and,
	 *		adding them to the list, once all of the data is added the sort
	 *		function from the list member is called to sort the nodes by ID
	 *		in descending order;
	 *
	 *		function takes no inputs;  
	 *
	 *		function returns nothing;
	 */
	StudentRecord();

	/*  Destructor:
   	 *		nothing explicitly done;
   	 *		list data is destroyed by list destructor on scope exit;
	 */
	~StudentRecord();

	/* size:
	 *		returns object list size when called;
	 *		takes no input;
	 */
	unsigned int size( void );

	/*	print():
	 *		prints all data from node that is contained at index marker;
	 *		determines if index has exceeded list size and prints error if so,
	 *		otherwise utilizes the list advance member to print specific node;
	 *
	 *		function takes no inputs;
	 *
	 *		function returns nothing, only outputs to stdout;
	 */
	void print( void );

	/* 	searchID():
	 *		iterates through entire node list to find particular match to
	 *		input;
	 *		increments list index marker when interating so that next
	 *		position is remembered and not lost on scope exit;
	 *		if data does not exist in list error is printed to stdout;
	 *		if match is found print() is called;
	 *
	 *		function takes string input representing studentID;
	 *
	 *		function returns nothing, only outputs to stdout;			
	 */
	void searchID( string );

	/* 	searchName():
	 *		iterates through entire node list to find all matches to
	 *		input;
	 *		increments list index marker when interating so that next
	 *		position is remembered and not lost on scope exit;
	 *		if data does not exist in list error is printed to stdout;
	 *		print() is called everytime a match is found;
	 *
	 *		function takes string input representing studentID;
	 *
	 *		function returns nothing, only outputs to stdout;			
	 */
	void searchName( string );	

/*      upperCase():
 *
 *      Preconditions:
 *			passed any valid string;
 *
 *      Postconditions:
 *			returns the same string in UPPERCASE;
 *
 *      Implementation: 
 *			accesses individual character indices in a loop
 *          and calls the 'toupper()' function to evaluate
 *          the letter's case and change if necessary or ignore if not;
 *			modified (or not) string is returned;
*/
	string upperCase( string );
};
#endif
