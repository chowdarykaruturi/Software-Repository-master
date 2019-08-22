#pragma once
////////////////////////////////////////////////////////////////////////////////////////
// Browse.h :  supports browsing of files in repository                               //
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
* identify(): prints the filename
* contains(): checks whether dependency filelist contains that file
* addDependencies(): adds dependency files into a vector
* doBrowse(): provides the functionality of browsing
*
* Required Files:
* ---------------
* DbCore.h, DbCore.cpp
* Utilities.h, Utilities.cpp
* Query.h, Query.cpp
* Process.h,Process.cpp
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

#include <string>
#include <vector>
#include <iostream>
#include "../DateTime/DateTime.h"
#include "../DbCore/DbCore.h"
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"
#include "../Version/Version.h"

#include "../DbCore/Definitions.h"
#include "../Query/Query.h"

using namespace NoSqlQuery;
namespace NoSqlDb
{

	/////////////////////////////////////////////////////////////////////
	// Browse class
	// -  provide browse class to support browsing
	template<typename T>
	class Browse
	{
	public:
		Browse() = default;
		Browse(const DbCore<T>& dbCore) { db_ = dbCore; }
		using Dependencies = std::vector<FileName>;
		bool doBrowse(const FileName& filename);
		bool contains(const FileName& filename);
		bool addDependencies(FileName& filename);
		static void identify(std::ostream& out = std::cout);
	private:
		Dependencies dependencies_;
		DbCore<T> db_;

	};


	//----< show file name >---------------------------------------------

	template<typename T>
	void Browse<T>::identify(std::ostream& out)
	{
		out << "\n  \"" << __FILE__ << "\"";
	}

	//----< does dependencies contain this key? >----------------------------------

	template<typename T>
	bool Browse<T>::contains(const FileName& filename)
	{
		if (std::find(dependencies_.begin(), dependencies_.end(), filename) != dependencies_.end())
			return true;
		else
			return false;
	}

	//----< adds dependency files into a vector >----------------------------------

	template<typename T>
	bool Browse<T>::addDependencies(FileName& filename)
	{
		std::vector<std::string> files = Directory::getFiles(RepoPath, filename);
		if (!contains(filename))
		{
			dependencies_.push_back(filename);
			if (files.size() == 1)
			{
				Children children = db_[filename].children();
				Children::iterator iter;
				for (iter = children.begin(); iter != children.end(); ++iter)
				{
					addDependencies(*iter);
				}
			}
		}
		return true;
	}

	//----< Provides functionality of browsing >----------------------------------

	template<typename T>
	bool Browse<T>::doBrowse(const FileName& filename)
	{
		Version v;
		std::cout << "\n Querying on the databse for the file " + filename;
		Query<T> q(db_);
		Conditions<PayLoad> conds2;
		conds2.name(".*" + filename + ".*");
		q.select(conds2);
		Keys keys = q.keys();
		std::vector<std::string> files = Directory::getFiles(RepoPath, filename);
		dependencies_.push_back(filename);
		if (files.size() == 1 && keys.size()>0)
		{
			Children children = db_[filename].children();
			Children::iterator iter;
			for (iter = children.begin(); iter != children.end(); ++iter)
			{
				addDependencies(*iter);
			}
		}
		std::cout << "\n Displaying the metadata of the record\n";
		showHeader();
		showRecord(filename, db_[filename]);
		std::vector<FileName>::iterator iter;
		if (dependencies_.size() > db_[filename].children().size())
		{
			std::cout << "\n\nAll the dependencies of the file " + filename + " are:\n";
			for (iter = dependencies_.begin(); iter != dependencies_.end(); ++iter)
			{
				std::cout << "\t" + *iter;
			}
		}
		std::cout << "\n";

		return true;
	}
}