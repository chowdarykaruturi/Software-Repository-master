#ifndef PAYLOAD_H
#define PAYLOAD_H
///////////////////////////////////////////////////////////////////////
// PayLoad.h - application defined payload                           //
// ver 1.0                                                                            //
//                                                                                    //
//Language     : C++                                                                  //
// Platform    : Lenovo 510S Ideapad, Win Pro 10, Visual Studio 2017                  //
// Application : CSE-687 OOD Project 4                                                //
// Author      : Akhil Karuturi,Syracuse University                        //
// Source      : Dr. Jim Fawcett, EECS, SU                                            //
////////////////////////////////////////////////////////////////////////////////////////
/*
*  Package Operations:
*  -------------------
*  This package provides a single class, PayLoad:
*  - holds a payload string and vector of categories
*  - provides means to set and access those values
*  - provides methods used by Persist<PayLoad>:
*    - Sptr toXmlElement();
*    - static PayLoad fromXmlElement(Sptr elem);
*  - provides a show function to display PayLoad specific information
*  - PayLoad processing is very simple, so this package contains only
*    a header file, making it easy to use in other packages, e.g.,
*    just include the PayLoad.h header.
*
*  Required Files:
*  ---------------
*    PayLoad.h, PayLoad.cpp - application defined package
*    DbCore.h, DbCore.cpp
*
*  Maintenance History:
*  --------------------
*  ver 1.1 : 19 Feb 2018
*  - added inheritance from IPayLoad interface
*  Ver 1.0 : 10 Feb 2018
*  - first release
*
*/

#include <string>
#include <vector>
#include <iostream>
#include "../XmlDocument/XmlDocument.h"
#include "../XmlDocument/XmlElement.h"
#include "../DbCore/DbCore.h"
#include "IPayLoad.h"


///////////////////////////////////////////////////////////////////////
// PayLoad class provides:
// - a std::string value which, in Project #2, will hold a file path
// - a vector of string categories, which for Project #2, will be 
//   Repository categories
// - methods used by Persist<PayLoad>:
//   - Sptr toXmlElement();
//   - static PayLoad fromXmlElement(Sptr elem);

using namespace XmlProcessing;
namespace NoSqlDb
{
	class PayLoad : public IPayLoad<PayLoad>
	{
	public:

		//using FilePath = std::string;
		//using Categories = std::vector<std::string>;

		PayLoad() = default;
		PayLoad(const std::string& val) : filepath_(val) {}
		static void identify(std::ostream& out = std::cout);
		PayLoad& operator=(const std::string& val)
		{
			filepath_ = val;
			return *this;
		}
		operator std::string() { return filepath_; }

		std::string filePath() const { return filepath_; }
		std::string& filePath() { return filepath_; }
		void filePath(const std::string& value) { filepath_ = value; }

		std::vector<std::string>& categories() { return categories_; }
		std::vector<std::string> categories() const { return categories_; }

		std::string status() const { return status_; }
		std::string& status() { return status_; }
		void status(const std::string& status) { status_ = status; }


		bool hasCategory(const std::string& cat)
		{
			return std::find(categories().begin(), categories().end(), cat) != categories().end();
		}

		Sptr toXmlElement();
		static PayLoad fromXmlElement(Sptr elem);

		static void showPayLoadHeaders(std::ostream& out = std::cout);
		static void showElementPayLoad(NoSqlDb::DbElement<PayLoad>& elem, std::ostream& out = std::cout);
		static void showDb(NoSqlDb::DbCore<PayLoad>& db, std::ostream& out = std::cout);
	private:
		std::string filepath_;
		std::vector<std::string> categories_;
		std::string status_;
	};

	//----< show file name >---------------------------------------------

	inline void PayLoad::identify(std::ostream& out)
	{
		out << "\n  \"" << __FILE__ << "\"";
	}
	//----< create XmlElement that represents PayLoad instance >---------
	/*
	* - Required by Persist<PayLoad>
	*/
	inline Sptr PayLoad::toXmlElement()
	{
		Sptr sPtr = XmlProcessing::makeTaggedElement("payload");
		XmlProcessing::XmlDocument doc(makeDocElement(sPtr));
		Sptr sPtrVal = XmlProcessing::makeTaggedElement("filePath", filepath_);
		sPtr->addChild(sPtrVal);
		Sptr sPtrCats = XmlProcessing::makeTaggedElement("categories");
		sPtr->addChild(sPtrCats);
		for (auto cat : categories_)
		{
			Sptr sPtrCat = XmlProcessing::makeTaggedElement("category", cat);
			sPtrCats->addChild(sPtrCat);
		}
		return sPtr;
	}
	//----< create PayLoad instance from XmlElement >--------------------
	/*
	* - Required by Persist<PayLoad>
	*/
	inline PayLoad PayLoad::fromXmlElement(Sptr pElem)
	{
		PayLoad pl;
		for (auto pChild : pElem->children())
		{
			std::string tag = pChild->tag();
			std::string val = pChild->children()[0]->value();
			if (tag == "filePath")
			{
				pl.filePath(val);
			}
			if (tag == "categories")
			{
				std::vector<Sptr> pCategories = pChild->children();
				for (auto pCat : pCategories)
				{
					pl.categories().push_back(pCat->children()[0]->value());
				}
			}
		}
		return pl;
	}
	/////////////////////////////////////////////////////////////////////
	// PayLoad display functions

	inline void PayLoad::showPayLoadHeaders(std::ostream& out)
	{
		out << "\n  ";
		out << std::setw(10) << std::left << "Name";
		out << std::setw(45) << std::left << "Payload filePath";
		out << std::setw(25) << std::left << "Categories";
		out << "\n  ";
		out << std::setw(10) << std::left << "--------";
		out << std::setw(40) << std::left << "--------------------------------------";
		out << std::setw(25) << std::left << "-----------------------";
	}


	inline void PayLoad::showElementPayLoad(NoSqlDb::DbElement<PayLoad>& elem, std::ostream& out)
	{
		out << "\n  ";
		out << std::setw(10) << std::left << elem.name().substr(0, 8);
		out << std::setw(40) << std::left << elem.payLoad().filePath().substr(0, 38);
		for (auto cat : elem.payLoad().categories())
		{
			out << cat << " ";
		}
	}

	inline void PayLoad::showDb(NoSqlDb::DbCore<PayLoad>& db, std::ostream& out)
	{
		showPayLoadHeaders(out);
		for (auto item : db)
		{
			NoSqlDb::DbElement<PayLoad> temp = item.second;
			PayLoad::showElementPayLoad(temp, out);
		}
	}
}
#endif
