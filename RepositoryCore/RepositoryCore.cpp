////////////////////////////////////////////////////////////////////////////////////////
// RepositoryCore.cpp :  provide means to check-in , check-out,browse                 //
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
* This package provide means to check-in , check-out,browse
* public Interfaces:
* ------------------
* identify(): prints the filename
* checkin(): supports to check-in file
* showDatab(): supports means to display database
* returnDb(): supports means to return database
* doBrowse(): provides the functionality of browsing
* checkingout(): procides functionality of checking out
* browse(): provides means to browse
*
* Required Files:
* ---------------
* DbCore.h, DbCore.cpp
* Utilities.h, Utilities.cpp
* Query.h, Query.cpp
* Process.h,Process.cpp
* FileSystem.h,FileSystem.cpp
* PayLoad.h,PayLoad.cpp
* CheckIn.h,CheckIn.cpp
* Checkout.h,Check.out.cpp
* Browse.h,Browse.cpp
*
* Build Process:
* --------------
* devenv Cpp11 - NoSqlDb.sln /rebuild debug
*
* Maintenance History:
* --------------------
* ver 1.0 : 4th mar 2018
*/

#include "../CppCommWithFileXfer/Utilities/StringUtilities.h"
#include "../CppCommWithFileXfer/Utilities/TestUtilities.h"
#include "../RepositoryCore/RepositoryCore.h"

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


//----< testing repository core functionalities >----------------------

bool testRepositoryCore()
{
	DbProvider dbp;
	DbCore<PayLoad> db = dbp.db();
	Utilities::Title("\nChecking-in a file");
	Checkin<PayLoad> ckin;
	RepositoryCore<PayLoad> rep;
	auto status = [](DbElement<PayLoad>& elem) {
		if (elem.payLoad().status() == "Closed") return "Closed";
		else if (elem.payLoad().status() == "PartialClosed") return "PartialClosed";
		else if (elem.payLoad().status() == "Open") return "Open";
		return "";
	};
	DbElement<PayLoad> elem;
	elem.name("Test.h");
	elem.descrip("Tests the databse functionalities");
	rep.checkin(elem);
	Utilities::Title("\nChecking out a file");
	Checkout<PayLoad> ckout;
	RepositoryCore<PayLoad> rep1;
	rep1.checkingout("Test.h");

	return true;
}

//----< test-stub >----------------------

#ifdef TEST_REPOSITORYCORE

using namespace Utilities;

int main()
{
	Utilities::Title("Testing Repository core functionalities");
	putLine();

	TestExecutive ex;

	// define test structures with test function and message
	TestExecutive::TestStr ts1{ testRepositoryCore, "checking out a file" };

	// register test structures with TestExecutive instance, ex
	ex.registerTest(ts1);


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

