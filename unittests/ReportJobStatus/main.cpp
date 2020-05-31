// ReportJobStatus.cpp : Defines the entry point for the console application.
//

#include "JobStatusHelper.h"
#include <iostream>
using namespace Galaxy::UnManagedLib;
using namespace std;

int main(int argc, char* argv[])
{
	CJobStatusHelper jobStatusHelper;
	jobStatusHelper.Initialize(8001);
	jobStatusHelper.SetJobProperty("Prop5", "1");

	int i = 0;
    for (int j = 0; j < 100000; j++)
    {
        for (int k = 0; k < 100000; k++)
        {
            i += j;
            i -= j;
        }
    }

	jobStatusHelper.SetJobProperty("Prop5", "2");
	jobStatusHelper.SetJobProperty("Prop6", "1");

	for (int j = 0; j < 100000; j++)
    {
        for (int k = 0; k < 100000; k++)
        {
            i += j;
            i -= j;
        }
    }

	jobStatusHelper.SetJobFinishProperty(true);

	return 0;
}

