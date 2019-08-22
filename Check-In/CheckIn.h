#pragma once
////////////////////////////////////////////////////////////////////////////////////////
// Checkin.h :  supports check-in of files into repository                            //
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
* identify(): prints the filename
* checkStatusofDep(): checks the status of dependency files
* checkStatusofFile(): checks the status of check-in file
* checkDependencies(): checks the dependencies files status
* copyFile(): copies file from source to destination
* doCheckin(): provides core functionality of check-in
* insertRecord(): inserts record into database
* showdatabase(): displays the database on console
* returndatabase(): returns database
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

#include <unordered_map>
#include <string>
#include <vector>
#include <iostream>
#include "../DateTime/DateTime.h"
#include "../DbCore/DbCore.h"
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"
#include "../Version/Version.h"

using namespace FileSystem;
namespace NoSqlDb
{

	/////////////////////////////////////////////////////////////////////
	// Checkin class
	// -  provide checkin class to support checking in of files
	template<typename T>
	class Checkin
	{
	public:
		Checkin(){}
		Checkin(const DbCore<T>& db) :db_(db) {}
		template<typename CallObj>
		bool checkDependencies(const DbElement<T>& dbElem, CallObj callObj, FileName filename);
		template<typename CallObj>
		std::string doCheckin(const DbElement<T>& dbElem, CallObj callObj);
		bool copyFile(FileName& srcPath, FileName& destPath ,const DbElement<T>& dbElem);
		static void identify(std::ostream& out = std::cout);
		template<typename CallObj>
		bool checkStatusofFile(CallObj callObj, FileName filename, const DbElement<T>& dbElem);
		template<typename CallObj>
		bool checkStatusofDep(CallObj callObj, FileName dep, const DbElement<T>& dbElem, FileName filename);
		bool insertRecord(const DbElement<T>& dbElem, std::string filename);
		DbCore<T> returnDatabase();
		void showdatabase();
	private:
		DbCore<T> db_;
	};


	//----< show file name >---------------------------------------------

	template<typename T>
	void Checkin<T>::identify(std::ostream& out)
	{
		out << "\n  \"" << __FILE__ << "\"";
	}


	//----< checks the status of dependencies files >---------------------------------------------

	template<typename T>
	template<typename CallObj>
	bool Checkin<T>::checkStatusofDep(CallObj callobj, FileName dep, const DbElement<T>& elem, FileName filename)
	{
		DbElement<T> dbElem = db_[dep];
		std::string status = callobj(dbElem);
		if (status == "Closed")
			return true;

		else if (status == "Open")
		{
			DbElement<T> ele = elem;
			std::string status1 = callobj(ele);
			if (status1 == "PartialClosed")
				insertRecord(elem, filename);
			return false;
		}
		else if (status == "PartialClosed")
		{
			return true;
		}
		return true;
	}

	//----< checks the status of check-in  file >---------------------------------------------

	template<typename T>
	template<typename CallObj>
	bool Checkin<T>::checkStatusofFile(CallObj callobj, FileName filename, const DbElement<T>& elem)
	{
		DbElement<T> dbElem = db_[filename];
		std::string status = callobj(dbElem);
		if (status == "Closed")
			return true;

		else if (status == "Open")
		{
			DbElement<T> ele = elem;
			std::string status1 = callobj(ele);
			if (status1 == "PartialClosed")
				insertRecord(elem, filename);
			return false;
		}
		else if (status == "PartialClosed")
		{
			return false;
		}
		return true;
	}

	//----< checks the dependencies whether they are repository and closed status >---------------------------------------------

	template<typename T>
	template<typename CallObj>
	bool Checkin<T>::checkDependencies(const DbElement<T>& dbElem, CallObj callObj, FileName filename)
	{
		Version v;
		Children dep = dbElem.children();
		PayLoad p = dbElem.payLoad();
		Children::iterator iter;
		for (iter = dep.begin(); iter != dep.end(); ++iter)
		{
			std::cout << "\n child is" << *iter;
			std::vector<std::string> files = FileSystem::Directory::getFiles(p.filePath(), *iter);
			if (files.size() == 0)
			{
				std::cout << "\n cannot check-in file because the dependency file " + *iter + " is not present in repository\n";
				return false;
			}
			if (!checkStatusofDep(callObj, *iter, dbElem, filename))
			{
				std::cout << "\n cannot check-in file because the dependency file " + *iter + " is in open status\n";
				return false;
			}
		}
		return true;
	}

	//----< copies file from source path to destination path  >----------------------------

	template<typename T>
	bool Checkin<T>::copyFile(FileName& srcFile, FileName& destFile,const DbElement<T>& dbElem)
	{
		PayLoad p = dbElem.payLoad();

		std::string srcPath = Path::fileSpec(localPath, srcFile);
		std::string destPath = Path::fileSpec(p.filePath(), destFile);
		std::cout << "\n\n copying file from " << srcPath << "\n to " << destPath;
		FileSystem::File::copy(srcPath, destPath, false);
		return true;
	}


	//----< provides the core functionality of check-in >---------------------------------------------

	template<typename T>
	template<typename CallObj>
	std::string Checkin<T>::doCheckin(const DbElement<T>& dbElem, CallObj callObj)
	{
		std::string filename = dbElem.name();
		if (dbElem.children().size() >= 1) {
			if (!checkDependencies(dbElem, callObj, filename))
				return "Cannot check-in "+filename +" because dependency file is in open status or not found in repository server";
		}
		Version v;
		std::string verStr;
		bool copied = false;
		int version = v.getLatestVersion(filename);
		if (version != 0)
			verStr = filename + "." + std::to_string(version);
		else if (version == 0) verStr = filename;

		std::vector<std::string> files = Directory::getFiles(RepoPath, verStr);
		if (files.size()>0 && files[0] == verStr)
		{
			if (checkStatusofFile(callObj, verStr, dbElem))
			{
				std::string verStr = v.addVersion(filename);
				std::string nonVerStr = v.removeVersion(filename);
				copied = copyFile(nonVerStr, verStr,dbElem);
				insertRecord(dbElem, verStr);
			}
			else
			{
				int version = v.getLatestVersion(filename);
				std::string nonVerStr = v.removeVersion(filename);
				if (version != 0)
					verStr = filename + "." + std::to_string(version);
				else if (version == 0) verStr = filename;
				copied = copyFile(nonVerStr, verStr, dbElem);
				insertRecord(dbElem, verStr);
			}
		}
		else
		{
			copied = copyFile(filename, filename, dbElem);
			insertRecord(dbElem, filename);
		}
		if (copied)
			std::cout << "\n Checked-in successfully\n";
		return "File Checked-in Successfully";
	}

	//----< inserts record into the database >---------------------------------------------

	template<typename T>
	bool Checkin<T>::insertRecord(const DbElement<T>& dbElem, std::string filename)
	{
		db_[filename] = dbElem;
		return true;
	}

	//----< displays database >---------------------------------------------

	template<typename T>
	void Checkin<T>::showdatabase()
	{
		showDb(db_);
	}

	//----< returns the database >---------------------------------------------

	template<typename T>
	DbCore<T> Checkin<T>::returnDatabase()
	{
		return db_;
	}
}