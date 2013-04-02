#! /usr/bin/env perl

# Taylor Spencer
# cs216program5.pl
# 11/29/2012

# Perl script parses data and provides a interface
# to the user for searching stored data


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

my $argc = @ARGV;
my $RET_CODE = 0;
my $studentFile   = "student.txt";
my $student_count = 0;
my %sh;

# check passed execution arguments
if ( $argc != 2 ){
  print "INVALID ARGUMENTS\n";
  $RET_CODE = 10; # abnormal exit, invalid arguments supplied
  exitProg();
}

# open student file
if ( !open(S_FILE, "$studentFile") ){
  print "Error opening file $studentFile\n";
  $RET_CODE = 20; # abnormal exit, input file error: unable to open
  exitProg();
}

# read in data from student file
while( chomp( my $s_line = <S_FILE> )){
    my @p = split(/ +/, $s_line);
    my $tokens = @p;

    # test for correct amount of tokens on line
    if ( $tokens != 5 ){
        $student_count += 1;
        print "Line $student_count of $studentFile has incorrect number of data items\n";
        $RET_CODE = 40; # abnormal exit, input file error: invalid number of tokens
        exitProg();
    }

    # test for correctness of ID token
    if ( (@p[0] !~ /[0-9]{9}/) or (length(@p[0]) != 9) ){
        $student_count += 1;
        print "Line $student_count of $studentFile has ID, (@p[0]), with an invalid value\n";
        $RET_CODE = 50; # abnormal exit, input file error: invalid token
        exitProg();
    }

    # test for corrrectness of Name tokens
    if ( (length(@p[1]) > 20) or (length(@p[2]) > 20) or
         (@p[1] =~ /\d+/)     or (@p[2] =~ /\d+/) ){
        $student_count += 1;
        print "Line $student_count of $studentFile contains an invalid name\n";
        $RET_CODE = 50; # abnormal exit, input file error: invalid token
        exitProg();
    }

    # test for corrrectness of Major/Class tokens
    if ( (length(@p[3]) != 2) or (length(@p[4]) != 2) or
         (@p[3] =~ /\d+/)     or (@p[4] =~ /\d+/)){
        $student_count += 1;
        print "Line $student_count of $studentFile contains a Major/Class identifier with an invalid value\n";
        $RET_CODE = 50; # abnormal exit, input file error: invalid token
        exitProg();
    }

    # check against newline
    if ($s_line !~ /^(\n)/){
        $student_count += 1;

        # hash key and import data into array
        unless( exists($sh{$p[0]}) ){
            $sh{$p[0]}=[$p[0], $p[1], $p[2], $p[3], $p[4]];
        }
    }
}

if ( $student_count == 0 ){
  print "Error, no data exists in $studentFile\n";
  $RET_CODE = 30; # abnormal exit, input file error: no data exists
  exitProg();
}

if ( @ARGV[0] eq "id" ){
  printByStudentID( @ARGV[1] );
}
elsif ( @ARGV[0] eq "name" ){
  printByStudentName( @ARGV[1] );
}
else{
  print "INVALID ARGUMENTS(TYPE)\n";
  $RET_CODE = 10; # abnormal exit, invalid arguments supplied
  exitProg();
}

# passed user input, prints matching data
sub printByStudentID{
    my $input = shift; # assign input value to passed param
    if ( exists($sh{$input}) ){
        print "$sh{$input}[0]\n"; # ID
        print "$sh{$input}[1]\n"; # LAST NAME
        print "$sh{$input}[2]\n"; # FIRST NAME
        print "$sh{$input}[3]\n"; # MAJOR
        print "$sh{$input}[4]\n"; # CLASS
        $RET_CODE = 2; # normal exit, data found
    }else {
        print "DATA NOT FOUND\n";
        $RET_CODE = 3; # normal exit, data not found
    }
}

# passed user input, prints all matching data
sub printByStudentName{
    my $input = shift; # assign input value to passed param
    my $count = 0;
    while ( (my $key, my $value) = each %sh ){
        if( $sh{$key}[1] =~ m/($input)/i ){ # match and ignore case
            print "$sh{$key}[0]\n"; # ID
            print "$sh{$key}[1]\n"; # LAST NAME
            print "$sh{$key}[2]\n"; # FIRST NAME
            print "$sh{$key}[3]\n"; # MAJOR
            print "$sh{$key}[4]\n"; # CLASS
            $count += 1;
        }
    }
    if ( $count == 0 ){
      print "DATA NOT FOUND\n";
      $RET_CODE = 3;  # normal exit, data not found
    }else { $RET_CODE = 2; } # normal exit, data found
}

# exit
exitProg();

# exits script returning RET_CODE
sub exitProg{
  # close file handle
  close S_FILE;
  exit $RET_CODE;
}

