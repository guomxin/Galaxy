#include <string>
#include <fstream>
using namespace std;

int main(int argc, char* argv[])
{
    ofstream outFile("Hello.txt");
    outFile.close();
    int i = 0;
    for (int j = 0; j < 1000000; j++)
    {
        for (int k = 0; k < 20000; k++)
        {
            i += j;
            i -= j;
        }
    }

    char *p = NULL;
    string str = p;
    //str.c_str();

	return 0;
}

