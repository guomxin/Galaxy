#pragma once
#import "COMClientLib.tlb" named_guids
#include <string>

namespace Galaxy {
namespace UnManagedLib {

class CJobStatusHelper
{
public:
	CJobStatusHelper();
	~CJobStatusHelper();

public:
	//////////////////////////////////////////////////////
	// Initialize the job status helper
	// 
	// Params:
	//		lPortNumber - the port number of the dest PN		
	//
	// Returns:
	//		true - successfully
	//		false - failed
	bool Initialize(long lPortNumber);

	//////////////////////////////////////////////////////
	// UnInitialize the job status helper
	void UnInitialize();

	//////////////////////////////////////////////////////
	// Set the property of the job
	// 
	// Returns:
	//		-1 - error
	//		0 - successfully
	//		1 - PN offline
	long SetJobProperty(std::string strPropName, std::string strPropValue);

	//////////////////////////////////////////////////////
	// Set the property of the job
	// 
	// Returns:
	//		-1 - error
	//		0 - successfully
	//		1 - PN offline
	long SetJobFinishProperty(bool fSuccessful);

private:
	COMClientLib::IJobStatusHelperPtr m_pJobStatusHelper;
};

} // namespace UnManagedLib
} // namespace Galaxy