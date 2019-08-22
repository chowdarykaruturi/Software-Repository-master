#pragma once
////////////////////////////////////////////////////////////////////////////////////////
// Version.h :  supports versioning of files                                          //
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
* This package supports versioning of files
* public Interfaces:
* ------------------
*
* Required Files:
* ---------------
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

#include <string>
#include <iostream>
#include "../DbCore/Definitions.h"
#include <unordered_map>


namespace NoSqlDb
{
	/////////////////////////////////////////////////////////////////////
	// Version class
	// -  provide functionalities of versioning to files

	class Version
	{
	public:
		Version() { restoreVersionsFromFiles(); }
		VerNum getVersion(const FileName& fileName);
		void restoreVersionsFromFiles();
		bool hasVersion(const std::string& fileName);
		FileName removeVersion(const FileName& filename);
		FileName addVersion(const FileName& fileName);
		VerNum getLatestVersion(FileName& filename);
		bool contains(const Key& key);
		using currentVersion = std::unordered_map<Key, int>;
		using iterator1 = typename currentVersion::iterator;
		static void identify(std::ostream& out = std::cout);
	private:
		currentVersion currentVersion_;
	};

}
