#pragma once
////////////////////////////////////////////////////////////////////////////////////////
// RepositoryCore.h :  provide means to check-in , check-out,browse                   //
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

#include "../DbCore/DbCore.h"
#include "../Browse/Browse.h"
#include "../Check-In/CheckIn.h"
#include "../Check-out/Checkout.h"
#include "../PayLoad/PayLoad.h"
#include "../Version/Version.h"
#include "../DbCore/Definitions.h"
#include "../Persist/Persist.h"

namespace NoSqlDb
{

	/////////////////////////////////////////////////////////////////////
	// RepositoryCore class
	// -  provide RepositoryCore class to provide means to check-in , check-out,browse
	template<typename T>
	class RepositoryCore
	{
	public:
		RepositoryCore() ;
		Status checkin(const DbElement<T>& dbElem);
		Children checkingout(const FileName& filename);
		static void identify(std::ostream& out = std::cout);
		DbCore<T> returnDb();
		void  showDatab();
		void formdb();
		Categories getfilesForcategory(const std::string& category);
		Children getfilesForFilename(const std::string& filename);
		Children getfilesForDependency(const std::string& filename);
		bool browse(const FileName& filename);
		bool insertRecord(const DbElement<T>& dbElem, std::string filename);
		DbElement<T> viewMetaData(const FileName& filename);
		void persistInDb();
		DbElement<T> ViewFileContent(const FileName& filename);
		DbElement<T> formMetaData(std::string name, std::string description, Children children, std::string status, Categories categories);
	private:
		DbCore<T> db_;
		bool dbformed = false;
	};

	//----< Queries on database to return files of given category >-------------------

	template<typename T>
	Categories RepositoryCore<T>::getfilesForcategory(const std::string& category)
	{
		Query<T> qry_(db_);
		qry_.selectPayloadCategory(category);
		qry_.keys();
		return qry_.keys();
	}

	//----< Queries on database to return files of given filename >---------

	template<typename T>
	Categories RepositoryCore<T>::getfilesForFilename(const std::string& filename)
	{
		Query<T> qry_(db_);
		qry_.showMatchKeys(".*"+filename+".*");
		qry_.keys();
		return qry_.keys();
	}

	//----< Queries on database to return files of given dependency >---------------------

	template<typename T>
	Children RepositoryCore<T>::getfilesForDependency(const std::string& filename)
	{
		Query<T> qry_(db_);
		qry_.selectChildren(".*" + filename + ".*");
		qry_.keys();
		return qry_.keys();
	}

	//----< inserts record into the database >---------------------------------------------

	template<typename T>
	bool RepositoryCore<T>::insertRecord(const DbElement<T>& dbElem, std::string filename)
	{
		db_[filename] = dbElem;
		return true;
	}

	//----< forms metadata for the DbElement >------------------------------------------
	template<typename T>
	DbElement<T> RepositoryCore<T>::formMetaData(std::string name, std::string description, Children children, std::string status, Categories categories)
	{
		DbElement<PayLoad> dbElem;
		dbElem.name(name);
		std::cout << "\n name is" << name;
		dbElem.descrip(description);
		std::cout << "\n description is" << description;
		dbElem.children(children);
		PayLoad p;
		p.filePath("../ServerStorage");
		for (auto item : categories) {
			p.categories().push_back(item);
		}
		p.status(status);
		std::cout << "\n status is" << status;
		dbElem.payLoad(p);
		return dbElem;
	}

	//----< Constructior >---------------------

	template<typename T>
	RepositoryCore<T>::RepositoryCore()
	{
		std::cout << "\n The database is persisted in the form of Xml, and \n everytime the server starts, it restores the database from Xml \n Initially there wont be any database so the file open will fail. \n If the project is ran again , then the database will be restored from that xml";
		formdb();
	}

	//----< Forms Db >---------------------

	template<typename T>
	void RepositoryCore<T>::formdb()
	{
		PersistDb::Persist<PayLoad> p(db_);
		bool success = false;
		try {
			success = p.restoreFromFile("DbXml.xml");
		}
		catch (std::exception e) {
			std::cout << e.what();
		}
		if (success) {
			db_ = p.returnDb();
		}
		
	}


	//----< show file name >---------------------------------------------

	template<typename T>
	void RepositoryCore<T>::identify(std::ostream& out)
	{
		out << "\n  \"" << __FILE__ << "\"";
	}

	template<typename T>
	void RepositoryCore<T>::persistInDb()
	{
		PersistDb::Persist<PayLoad> p(db_);
		if (p.saveToFile("DbXml.xml"))
			std::cout << "\n  db persisted into an xml \"DbXml.xml\" \n";
		else
			std::cout << "\n  filesave failed";
	}


	//----< provides means to check-in >---------------------------------------------

	template<typename T>
	Status RepositoryCore<T>::checkin(const DbElement<T>& dbElem)
	{
		Checkin<T>  ckin_(db_);
		auto status = [](DbElement<PayLoad>& elem) {
			if (elem.payLoad().status() == "Closed") return "Closed";
			else if (elem.payLoad().status() == "PartialClosed") return "PartialClosed";
			else if (elem.payLoad().status() == "Open") return "Open"; else return "";	};
		
		std::string checkin_status =ckin_.doCheckin(dbElem, status);
		db_ = ckin_.returnDatabase();
		showDb(db_);
		persistInDb();
		return checkin_status;
	}


	//----< provides means to show database >---------------------------------------------

	template<typename T>
	void  RepositoryCore<T>::showDatab()
	{
		showDb(db_);
	}

	//----< provides means to view file content >---------------------------------------------

	template<typename T>
	DbElement<T> RepositoryCore<T>::ViewFileContent(const FileName& filename)
	{
		Query<T> qry_(db_);
		return  qry_.ReturnValue(filename);
	}


	//----< provides means to check-out >---------------------------------------------

	template<typename T>
	Children RepositoryCore<T>::checkingout(const FileName& filename)
	{
		Checkout<T> ckout_(db_);

		Children files = ckout_.doCheckout(filename);
		Children files1;
		Version v;
		for (auto item : files) {
			files1.push_back(v.removeVersion(item));
		}

		return files1;
	}


	//----< provides means to show meta data >---------------------------------------------

	template<typename T>
	DbElement<T> RepositoryCore<T>::viewMetaData(const FileName& filename)
	{
		Query<T> qry_(db_);
		return  qry_.ReturnValue(filename);
	}
}