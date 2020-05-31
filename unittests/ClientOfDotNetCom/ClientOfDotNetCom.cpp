// ClientOfDotNetCom.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#import "DotNetCom.tlb" named_guids raw_interfaces_only
#include <iostream>
using namespace std;

int _tmain(int argc, _TCHAR* argv[])
{
	CoInitialize(NULL);
	DotNetCom::IDotNetInterfacePtr dotNetInterface;	
	
	HRESULT hRes = 
	  dotNetInterface.CreateInstance(DotNetCom::CLSID_MyDotNetClass);
	if (hRes == S_OK)
	{
		long lProcessId;
		hRes = dotNetInterface->TellProcessId(&lProcessId);
		if (hRes == S_OK)
		{
			cout << lProcessId << endl;
		}
		else
		{
			cout << "Call com object error!" << endl;
		}
	}
	else
	{
		cout << "Create com object failed!" << endl;
	}

	CoUninitialize ();   //DeInitialize all COM Components

	return 0;
}

