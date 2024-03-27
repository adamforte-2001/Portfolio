#include <stdio.h>
#include <unistd.h>
#include <fcntl.h>
#include <string.h>
#include <stdlib.h>
#include <stdbool.h>
#include <sys/wait.h>

#define COMMAND_MAX 100 //maximum number of parallel commands
#define LINE_MAX 255 //max number of chars in a line fed to the shell
#define SEPARATOR 32 // space delimeter for commands, purely internal
#define NUM_BUILTINS 3 //number of builtin commands
#define MAX_PATH_LENGTH 4096 //maximum number of characters in a single PATH directory
#define MAX_PATH_DIRS 100 // maximum number of path dirs

struct Command{ // data structure to store info about a command, several of these are created for parallel commands
    char* name;
    char** argv;
    int argc;
    char * filename; //do not modify 
};
struct Path{ //data structure to store information about the current PATH
    char** dirs;
    int n;
};

struct Command* parse_command(struct Command* c, char* command_str); // takes line from terminal with extraneous whitespace removed, returns the Command data structure
void get_commands(char** out, int* len, char* debug); //reads a line from the terminal and strips extraneous whitespace
int externalCommand(struct Command* c, struct Path * p); //handles external commands
void changePath(struct Command* c, struct Path* p); //called when user enters path, overwrites the current path with their entry
void printError(); // prints the error message to stderr
char * findFile(struct Command* c, struct Path * p, int * n); //runs the the dirs in the path to look for a requested file
int cd(struct Command* c); //simple change directory

int main(int argc, char *argv[]) {
    if (argc > 1) {    
        printError();
        exit(1);
    }
    //int number = 2; //debug line
    char *commandQueue[] = { //this is for debugging purposes only. It has no effect on the output, and little effect on the operation of the shell.
        "path this/is/a/test/path and/this/is/the/other/dir",
        "doSomething some args"
    };

    char *builtinList[NUM_BUILTINS] = {
        "exit",
        "cd",
        "path"
    }; // list of builtin commands
    
    struct Path * p = (struct Path*) malloc(sizeof(struct Path));
    p->dirs = (char **) malloc(sizeof(char*));
    p->n = 1;
    p->dirs[0] = (char*) malloc(5 * sizeof(char));
    strncpy(p->dirs[0], "/bin", 5);
    p->dirs[0][4] = '\0';
    int cs = 0;
    while (1){ //main loop
        char** commands = (char **) malloc(COMMAND_MAX * sizeof(char *));
        int numCommands;
        printf("rush> "); 
        fflush(stdout);
        get_commands(commands, &numCommands, commandQueue[cs]);
        commands = (char **) realloc(commands, numCommands * sizeof(char *));
        struct Command * parsed_commands = (struct Command *) malloc(numCommands * sizeof(struct Command));
        bool badInput = false;
        for (int i = 0; i < numCommands; i++){
            if (!parse_command(&parsed_commands[i], commands[i])){
                badInput = true;
                numCommands = i;
                break;
            }
        }
        if (badInput){
            printError();
            for (int i = 0; i < numCommands; i++){
                for (int j = 0; j < parsed_commands[i].argc; j++){
                    free(parsed_commands[i].argv[j]);
                }
                free(parsed_commands[i].name);
                free(parsed_commands[i].filename);
                free(commands[i]);
            }
            free(commands);
            free(parsed_commands);  
            continue;
        }
        bool newProcesses = 1; //flag for external commands
        int pids[numCommands];
        for (int i = 0; i < numCommands; i++){
            struct Command * ps = &parsed_commands[i];  
            for(int j = 0; j < NUM_BUILTINS; j++) {
                if (strcmp(parsed_commands[i].name, builtinList[j]) == 0){
                    newProcesses = 0; //set flag to false
                    switch(j){
                        case 0:
                            if (ps->argc != 0){
                                printError();        
                            }
                            else {exit(0);}
                            break;
                        case 1:
                            if (cd(ps)){
                                printError();
                            }
                            break;
                        case 2:
                            changePath(&(parsed_commands[i]), p);
                            break;
                    }
                }
            }
            if (newProcesses && (pids[i] = externalCommand(ps, p)) == -1){ //creates the list of pids to wait for and detects errors
                printError();
            }
        }
        
        if (newProcesses){ //wait for child processes to finish
            for (int i = 0; i < numCommands; i++){
                if (pids[i] == -1) continue;
                else waitpid(pids[i], NULL, 0);
            }
        }
        fflush(stdout);
        fflush(stderr);
        
        /* Do not alter */
        /* Deallocations */
        
        for (int i = 0; i < numCommands; i++){ // deallocate command structures and other heap memory created for the commands
            for (int j = 0; j < parsed_commands[i].argc; j++){
                free(parsed_commands[i].argv[j]);
            }
            free(parsed_commands[i].name);
            free(parsed_commands[i].filename);
            free(commands[i]);
        }
        free(commands);
        free(parsed_commands);
    }
    return 0;
}

void get_commands(char** out, int* len, char* debug){
    char lineBuffer[LINE_MAX + 1]; 
    char commandBuffer[LINE_MAX + 1];
    int n = 0;
    int numCommands = 1;
    
    while (n < LINE_MAX){ //get the line from input
        char current = getchar();
        if (current == EOF || current == '\n'){
            break;
        }
        else if (current == '&'){
            numCommands++;
        }
        lineBuffer[n++] = current;
    }
    
    lineBuffer[n++] = '\0';
    int strLen = n;
    n = 0;
    int sentinel = numCommands;
    for (int i = 0; i < sentinel; i++){
        bool delimiterWritten = 0;
        int commandLen = 0;
        while(n < strLen){
            if (lineBuffer[n] == '&' || lineBuffer[n] == '\0'){ //end of command
                n++;
                break;
            }
            if ((lineBuffer[n] == ' ' || lineBuffer[n] == '\t') && !delimiterWritten){ //handle spaces
                commandBuffer[commandLen++] = SEPARATOR;
                delimiterWritten = 1; 
            }
            else if (!(lineBuffer[n] == ' ' || lineBuffer[n] == '\t')) {//regular chars
                commandBuffer[commandLen++] = lineBuffer[n]; 
                delimiterWritten = 0;
            }
            n++;
        }
        delimiterWritten = 0;
        if (commandLen == 0 || (commandLen == 1 && commandBuffer[0] == ' ')){
            numCommands--;
            continue;
        }
        
        commandBuffer[commandLen++] = '\0';

        if (commandBuffer[commandLen - 2] == ' ' || commandBuffer[commandLen - 2] == '\t'){
            commandLen--;
            commandBuffer[commandLen - 1] = '\0';
        }
        out[i] = (char *) malloc(commandLen * sizeof(char));
        int start;
        if (commandBuffer[0] == ' ' || commandBuffer[0] == '\t'){
            start = 1;
            commandLen--;
        }
        else {
            start = 0;
        }
        strncpy(out[i], &(commandBuffer[start]), commandLen);
                
    }
    *len = numCommands;
}

struct Command* parse_command(struct Command* c, char* command_str){
    char buffer[LINE_MAX + 1];
    c->argv = (char **) malloc(100 * sizeof(char *)); // allow for 100 command args maximum
    c->argc = 0;
    char* p = command_str, *q = buffer;
    int i = 0;
    for (;*p != '\0' && *p != ' '; p++, q++){
        *q = *p;
        i++;
    }
    i++;
    *q = '\0';
    if (*p != '\0') p++;
    c->name = (char *) malloc(i * sizeof(char));
    strncpy(c->name, buffer, i);
    q = buffer;
    i = 0;
    
    while (*p != '\0' && *p != '>'){
        if (*p != ' '){
            *(q++) = *(p++);
            
            char t = *q;
            *q = '\0';
            *q = t;
            
            i++; 

        }
        else {
            p++;
            *q = '\0'; //end of arg, terminate str
            i++;
            c->argv[(c->argc)++] = (char *) malloc(i * sizeof(char)); // allocate memory in command, increment argc
            strncpy(c->argv[c->argc - 1], buffer, i);// copy buffer into command struct
            q = buffer;
            i = 0;
        }
    }
    if (*p != '>') p++;
    *q = '\0'; //end of arg, terminate str
    i++;
    if (buffer[0] != '\0'){
        c->argv[(c->argc)++] = (char *) malloc(i * sizeof(char)); // allocate memory in command, increment argc
        strncpy(c->argv[c->argc - 1], buffer, i);// copy buffer into command struct
    }
    q = buffer;
    i = 0;
    q = buffer;
    i = 0;
    if (*p == '>'){
        p++;
        while (*p == ' ' || *p == '\t'){
            p++;
        }
        while (*p != '\0' && *p != ' '){
            *(q++) = *(p++);
            i++;
        }
        if (*(p + 1) == '>'){
            *q = '\0';
            i++;       
            for (int k = 0; k < c->argc; k++){
                free(c->argv[k]);
            } 
            free(c->name);
            free(c->argv);
            return NULL;

        }
        else if (*p == ' '){
            c->filename = (char *)malloc(sizeof(char)); //indicates error;
            strncpy(c->filename, "", 1);
        }   
        else {
            c->filename = (char *) malloc(i * sizeof(char));
            strncpy(c->filename, buffer, i);
        }
        
    }
    else {
        c->filename = (char *)malloc(sizeof(char)); //indicates error;
        strncpy(c->filename, "", 1);
    }
    return c;
}

int externalCommand(struct Command* c, struct Path * p){ // most of this function is generating the argv to feed to execv after the fork
    char* newArgv[c->argc + 2];
    int pathArgSize;
    char * pathArg = findFile(c, p, &pathArgSize);
    if (!pathArg){
        return -1;
    }
    int t0 = strlen(c->name) + 1;
    newArgv[0] = (char *) malloc(t0 * sizeof(char));// avoid double free in main w/t free(c->name)
    strncpy(newArgv[0], c->name, t0);
    newArgv[0][t0 - 1] = '\0';
    for (int i = 1; i < c->argc + 1; i++){
        int n = strlen(c->argv[i-1]) + 1;
        newArgv[i] = (char *) malloc(n * sizeof(char));
        strncpy(newArgv[i], c->argv[i-1], n);
        newArgv[i][n - 1] = '\0';
    }
    newArgv[c->argc + 1] = NULL;
    int rc = fork();
    if (rc == -1){
       return -1;
    }
    else if (rc == 0){ 
        if (c->filename[0] != 0){
            //close(STDOUT_FILENO);
            int new = open(c->filename, O_CREAT | O_WRONLY | O_TRUNC, S_IRWXU);
            dup2(new, STDOUT_FILENO); // redirect to new file descriptor (i don't really understand this code, but I think it works. :) )
        }
        execv(pathArg, newArgv);
        return -1;
    }
    for (int i = 0; i < c->argc + 1; i++){
        free(newArgv[i]);
    }
    return rc; // return the pid
}

int cd(struct Command* c){
    if (c->argc != 1) return -1;
    return chdir(c->argv[0]);
}

void changePath(struct Command* c, struct Path* p){
    
    int n1 = (p->n < c->argc ? p->n : c->argc );
    int n2 = c->argc - p->n; // if negative, free remaining bits, otherwise, malloc
    int i;
    for (i = 0; i < n1; i++){
        int n = strlen(c->argv[i]);     
        p->dirs[i] = realloc(p->dirs[i], n + 1);
        strncpy(p->dirs[i], c->argv[i], n);
        p->dirs[i][n] = '\0';
    }
    if (n2 < 0){
        for (;i < n1 + (-1 * n2); i++){
            free(p->dirs[i]);
        }
    }
    else if (n2 > 0){
        for (;i < n1 + n2; i++){
            int n = strlen(c->argv[i]);
            p->dirs[i] = (char*) malloc((n + 1) * sizeof(char));
            strncpy(p->dirs[i], c->argv[i], n);
            p->dirs[i][n] = '\0';   
        }
    }
    p->n = c->argc;

}

void printError(){
    char * message = "An error has occurred\n";
    write(STDERR_FILENO, message, strlen(message)); 
    fflush(stderr);
}

char * findFile(struct Command* c, struct Path * p, int * n){
    int t, t2;
    int size = strlen(c->name) + (t = strlen(p->dirs[0])) + 1;
    if (p->dirs[0][t - 1] != '/'){
        size++;
        t2 = 1;
    }
    else {t2 = 0;}
    
    char * completePath = (char*) malloc(size * sizeof(char));
    strncpy(completePath, p->dirs[0], t);
    if (t2) {
        completePath[t] = '/';
    }
    strncpy(&completePath[t + t2], c->name, size - t - t2);
    completePath[size - 1] = '\0'; 
    if (!access(completePath, X_OK)){
        *n = size;
        return completePath;
    }
    for (int i = 1; i < p->n; i++){ //check dirs in the path in the order in which they were entered.
        size = strlen(c->name) + (t = strlen(p->dirs[i])) + 1;
        if (p->dirs[i][t - 1] != '/'){
            size++;
            t2 = 1;
        }
        else {t2 = 0;}
        
        completePath = (char*) realloc(completePath, size);
        strncpy(completePath, p->dirs[i], size);
        if (t2) {completePath[t] = '/';}
        strncpy(&completePath[t + t2], c->name, size - t - t2);
        completePath[size - 1] = '\0'; 
        if (!access(completePath, X_OK)){
            *n = size;
            return completePath;
        }

    }
    free(completePath);
    *n = 0;
    return NULL;
}
