#  VSH: Verbose Command Shell
#
#  Author:     Taylor Spencer
#  CS485001:   Systems Programming
#  Submitted:  03/26/2013


# link compiled object files and output executable

vsh: vsh_main.o vsh_parser.o vsh_util.o
	gcc -o vsh vsh_main.o vsh_parser.o vsh_util.o

vsh_main.o: vsh_main.c
	gcc -g -c -Wall -std=c99 vsh_main.c

vsh_parser.o: vsh_parser.c
	gcc -g -c -Wall -std=c99 vsh_parser.c

vsh_util.o: vsh_util.c
	gcc -g -c -Wall -std=c99 vsh_util.c

# clean object files
clean:
	rm *.o

# clean object and executable files
cleanall:
	rm *.o
	rm vsh

