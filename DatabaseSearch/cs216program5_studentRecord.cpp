/*	Title:		cs216program5_studentRecord.cpp
 *	Date:		11/29/2012
 *	Function:	studentRecord class implementation
 */

 /* FUNCTION DESCRIPTORS IN HEADER FILE */

#include "cs216program5_studentRecord.h"

/* init return code (code definitions in cs216program5.cpp */
signed int RET_CODE = 0;
/* init exception error */
string student_error = "UKNOWN ERROR";

/* constructor */
StudentRecord::StudentRecord()
{
	STUDENTFILE = "student.txt";
	StudentNode student;

	index = 0;

	ifstream f_in( STUDENTFILE );

	if( f_in.is_open() ){
		string line;
		while( f_in.good() && getline(f_in, line) ){
			int words = 0;
			stringstream input(line);

			if (input >> student.id){
				words++;
			}
			if (input >> student.last){
				words++;
			}
			if (input >> student.first){
				words++;
			}
			if (input >> student.major){
				words++;
			}
			if (input >> student.stClass){
				words++;
			}

			/* test for correct amount of tokens on line */
			if ( words != 5 ){
				string size = static_cast<ostringstream*>( &(ostringstream() << slist.size() + 1 ) )->str();

				string s1 = "Line " + size + " of ";
				string s2 = STUDENTFILE;
				string s3 = " has incorrect number of data items\n";
				student_error = s1 + s2 + s3;
				RET_CODE = 40;
				throw (student_error);
			}

			/* test for correctness of ID token */
			if ( (student.id.length() != 9) ||
			    !(student.id.find_first_not_of("0123456789") == 
				  std::string::npos) ){

				string size = static_cast<ostringstream*>( &(ostringstream() << slist.size() + 1 ) )->str();

				string s1 = "Line " + size + " of ";
				string s2 = STUDENTFILE;
				string s3 = " has ID, (" + student.id +
					        "), with an invalid value\n";
				student_error = s1 + s2 + s3;
				RET_CODE = 50;
				throw (student_error);
			}

			/* test for corrrectness of Name tokens */
			if ( (student.last.length() > 20)  ||
				 (student.first.length() > 20) ||
			    !(student.last.find_first_of("0123456789") == 
				  std::string::npos)           ||
				!(student.first.find_first_of("0123456789") == 
				  std::string::npos) ){

				string size = static_cast<ostringstream*>( &(ostringstream() << slist.size() + 1 ) )->str();

				string s1 = "Line " + size + " of ";
				string s2 = STUDENTFILE;
				string s3 = " contains a name with an invalid value\n";
				student_error = s1 + s2 + s3;
				RET_CODE = 50;
				throw (student_error);
			}

			/* test for corrrectness of Major/Class tokens */
			if ( (student.major.length() != 2)   ||
			     (student.stClass.length() != 2) ||
			    !(student.major.find_first_of("0123456789") == 
				  std::string::npos)             ||
				!(student.stClass.find_first_of("0123456789") == 
				  std::string::npos) ){

				string size = static_cast<ostringstream*>( &(ostringstream() << slist.size() + 1 ) )->str();

				string s1 = "Line " + size + " of ";
				string s2 = STUDENTFILE;
				string s3 = " contains a Major/Class identifier with an invalid value\n";
				student_error = s1 + s2 + s3;
				RET_CODE = 50;
				throw (student_error);
			}

			current = slist.begin();

			while( current != slist.end() ){
				current++;
			}
			slist.insert( current, student );
		}
		slist.sort();

		if( slist.size() == 0 ){
			string s1 = "Error, no data exists in ";
			string s2 = STUDENTFILE;
		 	student_error = s1 + s2;
			RET_CODE = 30;
		 	throw (student_error);
		}
	}
	else {
		string s1 = "Error opening file ";
		string s2 = STUDENTFILE;
		student_error = s1 + s2;
                RET_CODE = 20;
		throw (student_error);
	}
	f_in.close();
}

/* destructor */
StudentRecord::~StudentRecord(){}

unsigned int StudentRecord::size( void ){ return slist.size(); }

void StudentRecord::print( void )
{
	if( index > slist.size() ){
		cout << "ERROR: INDEX OVERFLOW" << endl;
		index = 0;
                RET_CODE = 99;
		return;
	}
	current = slist.begin();
	advance( current, index );

	cout << current->id << endl
	     << current->last << endl
	     << current->first << endl
	     << current->major << endl
	     << current->stClass << endl;
}

void StudentRecord::searchID( string input )
{
	int found = 0;
	int lastGoodPos = 0;

	current = slist.begin();

	index = 0;

	while( current != slist.end() ){
		 if( current->id == input ){
			print();
			++found;
			lastGoodPos = index + 1;
			RET_CODE = 2;
			break; /* no more records expected */
		}
		++current;
		++index;
	}
	if( !found ){
		cout << "Requested data not found" << endl;
                RET_CODE = 3;
	}

	index = lastGoodPos;
}

void StudentRecord::searchName( string input )
{
	int found = 0;
	current = slist.begin();
		
	while( current != slist.end() ){
		/* call upperCase(), essentially ignoring case conditions */
		if( upperCase(current->last) == upperCase(input) ){
			print();
			++found;
			RET_CODE = 2;
		}
		++current;
		++index;
	}
	if( !found ){
		cout << "Requested data not found" << endl;
		RET_CODE = 3;
	}
	index = 0;
}

string StudentRecord::upperCase( string input )
 {
         int i = 0;
         char c;
         /* iterate through each index and change case only if needed */
         while ( input[i] ) {
                 c = input[i];
                 input[i] = toupper(c);
                 i++;
         }
	return input;
}
