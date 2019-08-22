#pragma once
////////////////////////////////////////////////////////////////////////////////////////
// Checkout.h :  supports check-out of files from repository                          //
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

#include <string>
#include <vector>
#include <iostream>
#include "../DateTime/DateTime.h"
#include "../DbCore/DbCore.h"
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"

namespace NoSqlDb
{

	/////////////////////////////////////////////////////////////////////
	// Checkout class
	// -  provide Checkout class to support checking out of files
	template<typename T>
	class Checkout
	{
	public:
		Checkout() = default;
		Checkout(const DbCore<T>& dbCore) { db_ = dbCore; }
		using Dependencies = std::vector<FileName>;

		Children doCheckout(const FileName& filename);
		bool addDependencies(FileName& filename);
		bool contains(const FileName& filename);
		Dependencies& dependencies() { return dependencies_; }
		Dependencies dependencies() const { return dependencies_; }
		void dependencies(const Dependencies& dependencies) { dependencies_ = dependencies; }
		static void identify(std::ostream& out = std::cout);

	private:
		Dependencies dependencies_;
		DbCore<T> db_;
	};

	//----< show file name >---------------------------------------------

	template<typename T>
	void Checkout<T>::identify(std::ostream& out)
	{
		out << "\n  \"" << __FILE__ << "\"";
	}
	//----< does db contain this key? >----------------------------------

	template<typename T>
	bool Checkout<T>::contains(const FileName& filename)
	{
		if (std::find(dependencies_.begin(), dependencies_.end(), filename) != dependencies_.end())
			return true;
		else
			return false;
	}

	//----< adds dependencies files into a vector >----------------------------------

	template<typename T>
	bool Checkout<T>::addDependencies(FileName& filename)
	{
		DbElement<T> dbElem = db_[filename];
		PayLoad p = dbElem.payLoad();
		std::vector<std::string> files = Directory::getFiles(p.filePath(), filename);
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


	//----< provides core functionality of check-out >----------------------------------

	template<typename T>
	Children Checkout<T>::doCheckout(const FileName& filename)
	{
		DbElement<T> dbElem = db_[filename];
		PayLoad p = dbElem.payLoad();
		dependencies_.clear();
		Version v;
		std::vector<std::string> files = Directory::getFiles(p.filePath(), filename);
		dependencies_.push_back(filename);
		if (files.size() == 1)
		{
			Children children = db_[filename].children();
			Children::iterator iter;
			for (iter = children.begin(); iter != children.end(); ++iter)
			{
				addDependencies(*iter);
			}

			std::vector<FileName>::iterator iter1;
			for (iter1 = dependencies_.begin(); iter1 != dependencies_.end(); ++iter1)
			{
				FileName nonVerFileName = v.removeVersion(*iter1);
				std::string  srcPath = Path::fileSpec(p.filePath(), *iter1);
				std::string  destPath = Path::fileSpec(stagingpath, nonVerFileName);
				std::cout << "\n copying file from " << srcPath << "\n to " << destPath;
				FileSystem::File::copy(srcPath, destPath, false);
				std::cout << "\n File successfully checked-out\n";
			}
		}
		else std::cout << "\n File is not present in repository";
		return dependencies_;
	}
}
