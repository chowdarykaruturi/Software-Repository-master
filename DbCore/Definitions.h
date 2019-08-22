#ifndef DEFINITIONS_H
#define DEFINITIONS_H
///////////////////////////////////////////////////////////////////////
// Definitions.h - define aliases used throughout NoSqlDb namespace  //
// ver 1.0                                                           //
// Jim Fawcett, CSE687 - Object Oriented Design, Spring 2018         //
///////////////////////////////////////////////////////////////////////

#include <vector>
#include <string>

namespace NoSqlDb
{
	using Key = std::string;
	using Keys = std::vector<Key>;
	using Children = std::vector<Key>;
	using Parents = std::vector<Key>;
	using Categories = std::vector<Key>;
	using Status = std::string;

	const bool showKey = true;
	const bool doNotShowKey = false;

	const std::string localPath = "../StagingPath";
	const std::string RepoPath = "../ServerStorage";
	const std::string stagingpath = "../StagingPath";


	using FileSpec = std::string;    // c:/.../category/filename
	using FileRef = std::string;     // category/filename
	using FileName = std::string;    // filename may have version number at end
	using FullPath = std::string;    // full path with no filename
	using DirName = std::string;     // directory name 
	using Key = std::string;         // Dictionary key with format category.fileName
	using VerNum = std::int32_t;       // Dictionary value
}
#endif
