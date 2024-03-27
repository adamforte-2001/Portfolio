#include <unordered_map>
#include <string>
#include <fstream>
#include <iostream>
#include <vector>

using namespace std;
string** getMaze(const string& filename, int& n, int& m);
int convert2to1(int m, int x, int y); // x grows right, y grows down
void convert1to2(int i, int m, int& x, int& y);
string* decodeSolution(int* a, int n, string** maze, int m);

template <typename T>
bool checkArr(T* a, int n);
class Graph{
public:
    
    Graph(string** a, int n, int m){
        for (int i = 0; i < n*m; i++){
            vector<int> elem; 
            this->adjList.push_back(elem);
            this->adjListWeights.push_back(elem);
        }
        listFromMat(a, n, m);
        //cout<< this->adjList[m*n - 1][0] << endl;
    }
    void findGoalWithDFS(int goalV, string** maze, int m, ofstream & fout) const { // return a pointer to a dynamically created array and place its size in n
        int size = this->adjList.size();  
        int n = 0;
        bool discovered[size], explored[size];
        for (int i = 0; i < size; i++){
            discovered[i] = false;
            explored[i] = false;
        }
        vector<int> path;
        while (path.empty()){
            int i = 0;
            while((i < this->adjList.size()) && explored[i++]);
            if (i == this->adjList.size()) {
                n = 0;
                //cout<< "No Solution" << endl;
                return;
            }
            i--;
            auto start = path.begin(), end = path.end();
            path.erase(start, end);

            findGoalWithDFSREC(goalV, i, path, discovered, explored);
        }
        n = path.size();
        int* a = new int[n];
        for (int i = 0; i < n; i++){
            a[i] = path[i];
        }
        //return a;
        printSolution(a, n, maze, m, fout);
        delete[] a;
    }

private:
    vector<vector<int>> adjList;
    vector<vector<int>> adjListWeights;
    void listFromMat(string** a, int n, int m){
        for (int i = 0; i < n; i++){
            //cout<< "on " << i << "th row" << endl; 
            for(int j = 0; j < m; j++){
                if (a[i][j][0] == 'O'){
                    this->adjList[convert2to1(m,j,i)].push_back(convert2to1(m,j,i)); //goal is adjacent to itself to signal that the goal has been reached.
                    return;
                }
                bool red = a[i][j][0] == 'R';
                int vertical = 0, horizontal = 0;
                if (a[i][j].length() > 2){
                    switch(a[i][j][2]){ //this switch and the next handle the iteration order for finding adjacent vertices
                        case 'N':
                            vertical = -1;
                            break;
                        case 'S':
                            vertical = 1;
                            break; 
                        case 'E':
                            horizontal = 1;
                            break;
                        case 'W':
                            horizontal = -1;
                            break;
                    }
                }
                if (a[i][j].length() == 4){
                    switch(a[i][j][3]){
                    case 'N':
                        vertical = -1;
                        break;
                    case 'S':
                        vertical = 1;
                        break;
                    case 'E':
                        horizontal = 1;
                        break;
                    case 'W':
                        horizontal = -1;
                        break;
                    }
                }
                if (horizontal == 0 && vertical == 0) horizontal = vertical = 1;
                //cout<< a[i][j] << "-->";
                for (int x = j, y = i, distance = 0; x >= 0 && x < m && y >= 0 && y < n; x += horizontal, y += vertical, distance++){ 
                    if (a[y][x][0] == 'O' || (red ? a[y][x][0] == 'B' : a[y][x][0] == 'R')){
                        this->adjList[convert2to1(m,j,i)].push_back(convert2to1(m,x,y));
                        this->adjListWeights[convert2to1(m,j,i)].push_back(distance);
                        //cout<< a[y][x] << "(" << distance << ") ";
                    }
                }
                //cout<< endl;
            }
        }
    }
    
    void findGoalWithDFSREC(int goalV, int v, vector<int> & path, bool* discovered, bool* explored) const{
        if (v == goalV){
            path.push_back(v);
            return;
        }
        discovered[v] = true;
        for (int i = 0; i < this->adjList[v].size(); i++){
            if (!discovered[this->adjList[v][i]]){
                findGoalWithDFSREC(goalV, this->adjList[v][i], path, discovered, explored);
                if (!path.empty()){ 
                    path.push_back(v);
                    return;
                }
            }
        }
        explored[v] = true;
        return;
    }

    void printSolution(int* a, int n, string** maze, int m, ofstream & fout) const{
        int x, y;
        //auto solution = new string[n];
        for (int i = n-1; i > 0; i--){
            convert1to2(a[i], m, x, y);
            int k = -1, d;
            while (this->adjList[a[i]][++k] != a[i-1]);
            fout << this->adjListWeights[a[i]][k] << maze[y][x][2];
            if (maze[y][x].length() == 4) fout<< maze[y][x][3];
            fout << " ";
        } 
        fout << endl;
        
    }
  
};

int main(){
    int n, m;
    auto maze = getMaze("input.txt", n, m);
    Graph g(maze, n, m);
    ofstream output("output.txt");
    g.findGoalWithDFS(n*m - 1, maze, m, output);
    delete[] maze;

    return 0;
}

string** getMaze(const string& filename, int& n, int& m){
    ifstream infile(filename);
    auto consoleIn = cin.rdbuf();
    cin.rdbuf(infile.rdbuf());
    cin >> n >> m;
    //cout<< "dimensions: " << n << "x" << m << endl;
    string** a = new string*[n];
    for(int i = 0; i < n; i++){
        a[i] = new string[m];
        for (int j = 0; j < m; j++){
            cin >> a[i][j];
            //cout<< a[i][j] << " "; 
        }
        //cout<< endl;
    }

    cin.rdbuf(consoleIn);
    return a;
} 

int convert2to1(int m, int x, int y){
    return m*y + x;
}

void convert1to2(int i, int m, int& x, int& y){
    y = i/m;
    x = i % m;
}

template <typename T>
bool checkArr(T* a, int n){
    for (int i = 0; i < n; i++){
        if (!a[i]) return false;
    }
    return true;
}


