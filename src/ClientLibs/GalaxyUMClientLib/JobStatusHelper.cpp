#include "JobStatusHelper.h"

using namespace Galaxy::UnManagedLib;
using namespace COMClientLib;
using namespace std;

CJobStatusHelper::CJobStatusHelper()
{
	m_pJobStatusHelper = NULL;
}

CJobStatusHelper::~CJobStatusHelper()
{
	this->UnInitialize();
}

void CJobStatusHelper::UnInitialize()
{
	CoUninitialize();
}

bool CJobStatusHelper::Initialize(long lPortNumber)
{
	bool fRet = true;
	CoInitialize(NULL);	
	HRESULT hr = m_pJobStatusHelper.CreateInstance(COMClientLib::CLSID_JobStatusHelper);
	if (SUCCEEDED(hr))
	{
		if (m_pJobStatusHelper->Initialize(lPortNumber) == VARIANT_TRUE)
		{
			//
		}
		else
		{
			fRet = false;
		}
	}
	else
	{
		fRet = false;
	}

	return fRet;
}

long CJobStatusHelper::SetJobProperty(string strPropName, string strPropValue)
{
	long lRet = 0;
	if (m_pJobStatusHelper == NULL)
	{
		lRet = -1;
	}
	else
	{
		_bstr_t bstrPropName = strPropName.c_str();
		_bstr_t bstrPropValue = strPropValue.c_str();
		lRet = m_pJobStatusHelper->SetJobProperty(bstrPropName, bstrPropValue);
	}

	return lRet;
}

long CJobStatusHelper::SetJobFinishProperty(bool fSuccessful)
{
	long lRet = 0;
	if (m_pJobStatusHelper == NULL)
	{
		lRet = -1;
	}
	else
	{
		lRet = m_pJobStatusHelper->SetJobFinishProperty(fSuccessful);
	}

	return lRet;
}