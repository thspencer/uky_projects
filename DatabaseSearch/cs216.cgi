#! /usr/bin/env perl

# CS216PROGRAM5: cs216.cgi
#        AUTHOR: Taylor Spencer
#          DATE: 11/29/2012

# DESCRIPTION:
#     This is the cgi script that the HTML form submission calls
#     when the user submits their request;

#     Precondition (upon submission):
#        Valid submission of key/value pairs for:
#                program type, request type, request value;

#     Postcondition:
#        HTML formatted code that contains the response from
#        either the C++ executable or Perl script;

# STATUS CODES:
# 0  : normal exit, with no returned data (read as UNKNOWN error)
# 1  : abnormal exit: system fault
# 2  : normal exit, data found (specific, read ouput)
# 3  : normal exit, data not found
#
# 10 : abnormal exit, invalid arguments supplied
# 20 : abnormal exit, input file error: unable to open
# 30 : abnormal exit, input file error: no data exists
# 40 : abnormal exit, input file error: invalid number of tokens
# 50 : abnormal exit, input file error: invalid token (specific, read output)
# 99 : abnormal exit, unknown error caught

use strict;

# enable debug output to see returned execution parameters
my $DEBUG = "FALSE"; # (TRUE/FALSE)

my $cppExec = "search"; # C++ executable path
my $plExec  = "search.pl"; # Perl script path

# official UK blue
# found at:  http://www.uky.edu/Graphics/WebGraphicStandards.pdf
my $UKBLUE  = "#005DAA";

# begin HTML output formatting
print "Content-type: text/html\n";
print "\n";

# print info header and open HTML & HEAD tags
print "<!--\n";
print "<PRE>CS216PROGRAM5: cs216.cgi\n";
print "            AUTHOR: Taylor Spencer\n";
print "              DATE: 11/29/2012</PRE>\n";
print "-->\n";
print "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.01//EN\" \"http://www.w3.org/TR/html4/strict.dt\">";
print "<HTML>\n\n<HEAD>\n";
print "<META HTTP-EQUIV=\"CONTENT-TYPE\" CONTENT=\"text/html; charset=utf-8\">\n";
print "<TITLE>";

# title
print "The Taylor Spencer Data Search Engine";

# close TITLE & HEAD tags
print "</TITLE>\n</HEAD>\n\n";

# open BODY tag
print "<BODY>\n";

# body of page
print "<H2 ALIGN=\"CENTER\"><FONT COLOR=\"$UKBLUE\" FACE=\"wide latin\">\n";
print "<B>Spartan University\n<BR>Database Search</B>\n</FONT></H2>\n\n";

my $form = <STDIN>; # recieved via POST method on STDIN

# convert special chars represented by their hex value
# ( preceded by % ) vi a pattern match/substitution
# code from Paul Piwowarski, CS316echo.cgi
$form =~ s/%(..)/sprintf("%c", hex $1)/eg;

# name/value pairs are separated by '&'
# put each pair into an arrray element
my @pairs = split(/&/, $form);

my $number = @pairs;

my @perlArgs;  # Array to hold N/V arguments

# match to get the arguments, after the equal sign
for (my $i=0; $i < $number; $i++) {
  $pairs[$i]    =~ /(.+)=(.+)/;
  $perlArgs[$i] =  $2;
}

my $searchLang = $perlArgs[0]; # user requested language type
my $searchType = $perlArgs[1]; # user requested search type
my $searchReq  = $perlArgs[2]; # requested info to search for

my @returned; # executable output will be stored in @returned
my $count = 0;# number of lines returned

# determine requested language type
if ( $searchLang eq "perl" ){
  @returned = `./$plExec $searchType $searchReq`; # store $plExec output

  # begin HTML output
  print "<P><CENTER><B>\nFor the student data using Perl\n";
  print "<BR>The data you requested:\n</B></CENTER></P>\n\n";

  parseData(@returned); # call to print returned output

  # script returns here and continues
}
elsif ( $searchLang eq "c++" ){
  @returned = `./$cppExec $searchType $searchReq`; # store $cppExec output

  # begin HTML output
  print "<P><CENTER><B>\nFor the student data using C++\n";
  print "<BR>The data you requested:\n</B></CENTER></P>\n\n";

  parseData(@returned); # call to print returned output

  # script returns here and continues
}
else{
  print "<H3 ALIGN=\"center\">\n<B>INVALID LANGUAGE SELECTION</B>\n</H3>\n";
}

# reset form button (return to previous page)
print "<P><CENTER>\n<FORM>\n";
print "<INPUT TYPE=\"BUTTON\" VALUE=\"Start Over\" onClick=\"history.go(-1)\"; return true;\">\n";
print "</FORM>\n</CENTER></P>\n";

# footer
print "\n<H5 align=\"center\">\n";
print "<BR>CS 216 - Program 5\n";
print "<BR>Copyright &copy; 2012, Taylor Spencer.  All rights reserved.\n";
print "</H5>\n\n";

# close BODY & HTML tags
print "</BODY>\n</HTML>";

# parseData()
# Preconditions:
#    - passed a valid array containing returned output from executable
#    - executable returned a valid exit code to system
# Postcondtions:
#    - executable exit code is determined;
#    - returned output is shifted into a new array by groups of 5**
#      to account for multiplicities of similar data;
#    - nothing is returned to caller;
#
#   ** assumption is that if data was found the then returned
#      output was in groupings of 5 lines each, if not then invalid
#      results may be seen;
sub parseData{
  my @data = @_; # assign passed array

  # need to normalize by bit shifting right by 8
  # only need the high bits of 16 bit field
  my $RET = $? >> 8;

  if ( $RET == 2 ){
    $count = @data; # find number of lines in array

    # this loop shifts the existing data into a new array
    # which will call printOutput() every 5 lines;
    # the original data in data[0] is inserted into the new array
    # at the proper index (0-4);
    # each iteration of the inner loop deletes the item in first position
    # this assumes the returned data is good and exists in groupings of 5
    while ( $count > 0 ){
      my @dataSeg;

      for ( my $i = 0; $i < 5; $i++){
        @dataSeg[$i] = $data[0]; # new array[i] = data in first index of old
        shift @data; # remove the first element in the array
      }

      printOutput(@dataSeg); # print segmented 5 items
      $count = $count - 5; # decrement line count by 5, then restart loop
    }
  }elsif ( $RET == 3 ){ # normal exit but no data returned
     print "<B><CENTER>'$searchReq' was not found in the database";
     print "</CENTER></B>\n";
  }else { print "<B><CENTER>$data[0]</CENTER></B>"; } # all other errors
}

# printOutput()
# Preconditions:
#    - passed a valid array of 5 items containing data to print
# Postcondtions:
#    - passed data from each of the 5 index positions in array
#      is printed in a HTML TABLE;
#    - nothing is returned to caller
sub printOutput{
  my @data = @_; # assign passed array

  # open table
  print "<P><CENTER>\n<TABLE BORDER=1>\n";
  # print returned data
  print "<TR><TD>Student ID:</TD><TD>$data[0]</TD></TR>\n";
  print "<TR><TD>Last Name:</TD><TD>$data[1]</TD>\n";
  print "<TR><TD>First Name:</TD><TD>$data[2]</TD></TR>\n";
  print "<TR><TD>Major:</TD><TD>$data[3]</TR>\n";
  print "<TR><TD>Class:</TD><TD>$data[4]</TR>\n";
  # close table
  print "</TABLE>\n</CENTER></P>\n\n";
}

### DEBUG CODE ###
if ( $DEBUG =~ m/(true)/i){
  my $RET = $? >> 8;
  $count = @returned; # get item count of returned data

  print "\n<H6><B>\n<U>DEBUG OUTPUT</U><BR>\n";
  print "<BR>LANGUAGE: $searchLang\n";
  print "<BR>ARGUMENTS: $searchType, $searchReq\n";
  print "<BR>RETURN CODE: $RET\n";
  print "<BR>ITEM COUNT: $count\n";
  print "</B></H6>\n";
}### END DEBUG CODE ###
