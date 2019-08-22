////////////////////////////////////////////////////////////////////////////////////////
// Version.cpp :  supports versioning of files                                        //
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
* identify(): prints the filename
* getVersion():extract version number from specified fileName
* contains():is the key present in dictionary?
* restoreVersionsFromFiles(): creates dictionary of version numbers
* hasVersion():does this fileName have a version number?
* removeVersion():strips version number from filename
* addVersion():adds version number to filename
* getLatestVersion():returns the current version number for filename
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



#include "Version.h"
#include "../CppCommWithFileXfer/Utilities/StringUtilities.h"
#include "../CppCommWithFileXfer/Utilities/TestUtilities.h"
#include <iostream>
#include <iomanip>
#include <functional>
#include <vector>
#include "../DbCore/Definitions.h"
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"
using namespace NoSqlDb;
using namespace FileSystem;


//----< show file name >---------------------------------------------

void Version::identify(std::ostream& out)
{
	out << "\n  \"" << __FILE__ << "\"";
}

/*----< extract version number from specified fileName >-------*/
/*
*  Not necessarily the latest version number.
*  Returns zero if file has no version number.
*/
VerNum Version::getVersion(const FileName& fileName)
{
	VerNum ver = 0;
	int pos = fileName.find_last_of(".");
	std::string verStr = fileName.substr(pos + 1);
	try
	{
		ver = std::stoi(verStr);
	}
	catch (std::exception& ex)
	{
		return 0;
		std::cout << "\n" << ex.what();
	}
	return ver;
}

/*----< is the key present in dictionary? >----------------*/

bool Version::contains(const Key& key)
{
	iterator1 iter = currentVersion_.find(key);
	return iter != currentVersion_.end();
}

/*----< creates Dictionary of version numbers >----------------*/
/*
*  Reads filenames from each Storage category and builds
*  Dictionary with category.filename keys and largest
*  version number values.
*/
void Version::restoreVersionsFromFiles()
{
	currentVersion_.clear();

	std::vector<std::string> files = Directory::getFiles(RepoPath);
	for (size_t i = 0; i < files.size(); i++)
	{

		std::string file_nv = removeVersion(files[i]);
		std::string key = file_nv;
		int versionNum = getVersion(files[i]);
		if (contains(key))
		{
			if (versionNum > currentVersion_[key])
				currentVersion_[key] = versionNum;
		}
		else
		{
			currentVersion_[key] = versionNum;
		}
	}
}
/*----< does this fileName have a version number? >------------*/

bool Version::hasVersion(const std::string& fileName)
{
	int pos = fileName.find_last_of(".");
	std::string end = fileName.substr(pos + 1);
	try
	{
		std::stoi(end);
	}
	catch (std::exception& ex)
	{
		return false;
		std::cout << "\n" << ex.what();
	}
	return true;
}


/*----< strips version number from filename >------------------*/
/*
* Used when retrieving file from storage.
*/
FileName Version::removeVersion(const FileName& filename)
{
	int pos = filename.find_last_of(".");
	std::string end = filename.substr(pos + 1);
	try
	{
		std::stoi(end);
		return filename.substr(0, pos);
	}
	catch (std::exception& ex)
	{
		return filename;
		std::cout << "\n" << ex.what();
	}

	return filename;
}


/*----< adds version number to filename >----------------------*/
/*
* Adds version number equal to 1 + latestVersionNumber.
* Used before storing file in Storage.
*/

FileName Version::addVersion(const FileName& fileName)
{
	int version = 0;

	std::string key = removeVersion(fileName);
	if (contains(key))
		version = currentVersion_[key];
	currentVersion_[key] = ++version;
	return key + "." + std::to_string(version);

}

/*----< returns the current version number for filename >------*/

VerNum Version::getLatestVersion(FileName& filename)
{
	int temp = 0;
	filename = removeVersion(filename);
	std::string key = filename;
	if (contains(key))
		temp = currentVersion_[key];
	return temp;
}



#ifdef VERSION
int main()
{
	Version ver;
	std::string x = ver.addVersion("abc.cpp.115");
	std::cout << x;
	std::getchar();
}
#endif
