////////////////////////////////////////////////////////////////////////////////////////
// Checkin.cpp :  supports check-in of files into repository                          //
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
* This package supports checkin of files into repository
* public Interfaces:
* ------------------
* testDoCheckin(): tests the check-in of files
*
* Required Files:
* ---------------
* DbCore.h, DbCore.cpp
* Utilities.h, Utilities.cpp
* Process.h,Process.cpp
* FileSystem.h,FileSystem.cpp
* Version.h,version.cpp
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
#include "CheckIn.h"

using namespace NoSqlDb;
using namespace FileSystem;


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

//----< tests check-in of file >----------------------

bool testDoCheckin()
{
	DbProvider dbp;
	DbCore<PayLoad> db = dbp.db();
	Checkin<PayLoad> ckin;
	auto status = [](DbElement<PayLoad>& elem) {
		if (elem.payLoad().status() == "Closed") return "Closed";
		else if (elem.payLoad().status() == "PartialClosed") return "PartialClosed";
		else if (elem.payLoad().status() == "Open") return "Open";
		return "";
	};
	DbElement<PayLoad> elem;
	elem.name("Test.h");
	elem.descrip("Tests the databse functionalities");
	ckin.doCheckin(elem, status);

	return true;
}

//----<test stub>----------------------

#ifdef TEST_CHECKIN

using namespace Utilities;

int main()
{
	Utilities::Title("Testing Checkin into repository");
	putLine();

	TestExecutive ex;

	// define test structures with test function and message
	TestExecutive::TestStr ts3a{ testDoCheckin, "checking in a file" };

	// register test structures with TestExecutive instance, ex
	ex.registerTest(ts3a);


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