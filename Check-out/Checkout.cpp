////////////////////////////////////////////////////////////////////////////////////////
// Checkout.cpp :  supports check-out of files from repository                        //
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
* This package supports checkout of files from repository
* public Interfaces:
* ------------------
* identify(): prints the filename
* contains(): checks whether the db already contains this file
* addDependencies(): adds the dependencies files into a vector
* doCheckout(): provides core functionality of check-out
*
* Required Files:
* ---------------
* DbCore.h, DbCore.cpp
* Utilities.h, Utilities.cpp
* FileSystem.h,FileSystem.cpp
* Version.h,version.cpp
* Definitions.h
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

#include "../Version/Version.h"
#include <iostream>
#include <fstream>
#include "Checkout.h"

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

bool testDoCheckout()
{

	DbProvider dbp;
	DbCore<PayLoad> db = dbp.db();
	Checkout<PayLoad> ckt(db);

	ckt.doCheckout("Test.h");
	dbp.db() = db;
	return true;
}

//----< test stub >----------------------
#ifdef TEST_CHECKOUT

using namespace Utilities;

int main()
{
	Utilities::Title("Testing DbCore - He said, she said database");
	putLine();

	TestExecutive ex;

	// define test structures with test function and message
	TestExecutive::TestStr ts3a{ testDoCheckout, "checking out a file" };

	// register test structures with TestExecutive instance, ex
	ex.registerTest(ts3a);



	// run tests

	bool result = ex.doTests();
	if (result == true)
		std::cout << "\n  all tests passed";
	else
		std::cout << "\n  at least one test failed";

	putLine(2);
	std::cout << "Hello World";
	return 0;
}
#endif

