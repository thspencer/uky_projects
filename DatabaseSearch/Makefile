# link compiled object files and output executable
search: cs216program5.o cs216program5_studentRecord.o
	g++ -o search cs216program5.o cs216program5_studentRecord.o

# compile main program object file, depends on .h class header
cs216program2.o: cs216program5.cpp cs216program5_studentRecord.h
	g++ -c -Wall cs216program5.cpp

# create studentRecord object file, depends on .h studentRecord header
cs216program5_studentRecord.o: cs216program5_studentRecord.cpp cs216program5_studentRecord.h
	g++ -c -Wall cs216program5_studentRecord.cpp

# clean object files 
clean:
	rm *.o

# clean object and executable files
cleanall:
	rm *.o
	rm search

