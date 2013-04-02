/*	Title:		cs216program5.cpp
 *	Date:		11/29/2012
 *	Function:	Create class objects that read in file data representing
 *				student and class data;
 *				presents user with interface to search parsed data;
 */

/* studentRecord class header*/
#include "cs216program5_studentRecord.h"

#include <exception>

/*
 STATUS CODES:
 * 0  : normal exit, with no returned data (read as UNKNOWN error)
 * 1  : abnormal exit: system fault
 * 2  : normal exit, data found (specific, read ouput)
 * 3  : normal exit, data not found
 * 
 * 10 : abnormal exit, invalid arguments supplied
 * 20 : abnormal exit, input file error: unable to open
 * 30 : abnormal exit, input file error: no data exists
 * 40 : abnormal exit, input file error: invalid number of tokens
 * 50 : abnormal exit, input file error: invalid token (specific, read output)
 * 99 : abnormal exit, unknown error caught
 */

/* using 'extern signed int RET_CODE' declared in header */

using namespace std;

/*	main:
 *		creates student and class objects;
 *		presents user with interface to search through data or quit;
 *		if user input is valid then search is passed to appropriate
 *		object member function;
 *		if invalid, menu loops until valid input is recieved;
 *
 *		inputs are execution parameter argument count and values;
 *
 *		function returns execution status integer;
 */
int main( int argc, char **argv )
{
 /* need at least 3 arguments {executable, search type, request} */
if ( argc != 3 ){
	cout << "INVALID EXECUTION PARAMETERS" << endl;
	RET_CODE = 10;
}
else{ 
 try{
	StudentRecord students;

	string requestType, requestVal;

	requestType = argv[1];
	requestVal  = argv[2];

	if( requestType == "id" ){
		students.searchID ( requestVal );
	}
	else if( requestType == "name" ){
		students.searchName( requestVal );
	}
	else{
		cout << "INVALID SEARCH TYPE" << endl;
		RET_CODE = 10;
	}

}catch (string err){
	  cout << err << endl;
 }
}
return RET_CODE;
}

