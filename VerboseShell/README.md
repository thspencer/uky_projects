VSH: Verbose Command Shell
Author: Taylor Spencer
CS485-001: Systems Programming

VSH is a system shell that implements basic commands as well as utilizing process forking to execute external commands.  Input is taken on STDIN and then parsed for identifiable tokens such as strings or meta-characters.

The data structure is a linked list instead of the more traditional array of c strings that is used to contain the various parsed tokens.  As one token is identified a new node is added to the list to store the next.

Building VSH:

    run 'make' inside the VerboseShell directory
    use './vsh' to execute

VSH Commands:

    % any text - comment, everything following is ignored

    setprompt "string" - changes VSH prompt

    setvar $variable_name "string" - adds a variable to the shell that will be used thereafter in variable replacement situations

    echocmd on/off - displays input command before parsing

    parsecmd on/off - displays parsed command

    showchild on/off - displays process fork log

    cd directory_name - change current working directory

    exit - gracefully exit shell

Token Types:

    word - any sequence of characters excluding meta-characters containing no white-space

    string - "enclosed text"

    meta - '%', '|', '>', '<', '&'

    end-of-line - indication that current line ended
