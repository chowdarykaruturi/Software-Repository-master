////////////////////////////////////////////////////////////////////////////////////////
// Browse.cpp :  supports browsing of files in repository                               //
// ver 1.0                                                                            //
//                                                                                    //
//Language     : C++                                                                  //
// Platform    : Lenovo 510S Ideapad, Win Pro 10, Visual Studio 2017                  //
// Application : CSE-687 OOD Project 4                                                //
// Author      : Akhil Karuturi,Syracuse University                        //
// Source      : Dr. Jim Fawcett, EECS, SU                                            //
////////////////////////////////////////////////////////////////////////////////////////
/*
* Package Operations:
* -------------------
* This package supports browsing od files on repository
* public Interfaces:
* ------------------
* testDoBrowse(): tests browsing functionality
*
* Required Files:
* ---------------
* DbCore.h, DbCore.cpp
* Utilities.h, Utilities.cpp
* FileSystem.h,FileSystem.cpp
*
* Build Process:
* --------------
* devenv Cpp11 - NoSqlDb.sln /rebuild debug
*
* Maintenance History:
* --------------------
* ver 1.0 : 4th mar 2018
*/

#include "../DbCore/Definitions.h"
#include "../CppCommWithFileXfer/Utilities/StringUtilities.h"
#include "../CppCommWithFileXfer/Utilities/TestUtilities.h"
#include "../DbCore/DbCore.h"
#include "../PayLoad/PayLoad.h"
#include <iostream>
#include <fstream>
#include "Browse.h"
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"
using namespace FileSystem;

using namespace NoSqlDb;


//----< reduce the number of characters to type >----------------------

auto putLine = [](size_t n = 1, std::ostream& out = std::cout)
{
	Utilities::putline(n, out);
};

///////////////////////////////////////////////////////////////////////
// DbProvider class
// - provides mechanism to share a test database between test functions
//   without explicitly passing as a function argument.

class DbProvider
{
public:
	DbCore<PayLoad>& db() { return db_; }

private:
	static DbCore<PayLoad> db_;
};
DbCore<PayLoad> DbProvider::db_;


//----<tests browsing of a file >----------------------

bool testDoBrowse()
{
	DbProvider dbp;
	DbCore<PayLoad> db = dbp.db();
	DbElement<PayLoad> elem;
	elem.name("Persist.h");
	elem.descrip("Persists database into xml");
	db["Persist.h"] = elem;
	Browse<PayLoad> brw(db);
	brw.doBrowse("Persist.h");

	return true;
}

//----< test stub >----------------------------------------------------

#ifdef TEST_BROWSE

using namespace Utilities;

int main()
{
	Utilities::Title("Testing browsing in repository");
	putLine();

	TestExecutive ex;

	// define test structures with test function and message
	TestExecutive::TestStr ts5{ testDoBrowse, "browsing in a file" };

	// register test structures with TestExecutive instance, ex

	ex.registerTest(ts5);


	// run tests

	bool result = ex.doTests();
	if (result == true)
		std::cout << "\n  all tests passed";
	else
		std::cout << "\n  at least one test failed";

	putLine(2);
	return 0;
}
#endif