////////////////////////////////////////////////////////////////////////////////////////
// Persist.h : store and retrieve NoSqlDb contents                                    //
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
* This package creates xml format of database and also helps to
* retrieve it back to key/value pair database
* public Interfaces:
* ------------------
* createXml() : creates an xml from the database
* saveToFile() : saves created xml in a file
* restore() : this is where restoration takes place
* restoreFromFile() : reads the xml file
*
* Required Files:
* ---------------
* DbCore.h, DbCore.cpp
* DateTime.h, DateTime.cpp
* Utilities.h, Utilities.cpp
* Persist.h, Persist.cpp
*
* Build Process:
* --------------
* devenv Cpp11 - NoSqlDb.sln /rebuild debug
*
* Maintenance History:
* --------------------
* ver 1.0 : 6th feb 2018
*/


#include "../CppCommWithFileXfer/Utilities/StringUtilities.h"
#include "../CppCommWithFileXfer/Utilities/TestUtilities.h"
#include "../DbCore/DbCore.h"
#include <string>
#include <iostream>
#include <vector>
#include <algorithm>
#include <regex>
#include <fstream>
#include "../DateTime/DateTime.h"
#include "../XmlDocument/XmlDocument.h"
#include "../XmlDocument/XmlElement.h"
#include "../PayLoad/PayLoad.h"
using namespace NoSqlDb;
using namespace XmlProcessing;
using sPtr = std::shared_ptr < AbstractXmlElement >;
namespace PersistDb
{
	/////////////////////////////////////////////////////////////////////
	// Persist class
	// -  provide persisting and retireving of db functionalities
	template<typename T>
	class Persist
	{
	public:
		using Xml = std::string;
		using SPtr = std::shared_ptr<AbstractXmlElement>;
		using SPtrs = std::vector<SPtr>;
		Persist(const DbCore<T>& dbCore) { db_ = dbCore; }
		Xml createXml();
		bool saveToFile(const std::string& fileSpec);
		void restore(const Xml& xml);
		bool restoreFromFile(const std::string& fileSpec);
		DbCore<T> returnDb();
	private:
		DbCore<T> db_;
	};

	template<typename T>
	DbCore<T> Persist<T>::returnDb() {
		return db_;
	}



	/*
	* This is where the restoration takes place.
	*/
	template<typename T>
	void Persist<T>::restore(const Xml& xml)
	{
		Utilities::title("\n  Building Database from XmlDocument");
		using Sptr = std::shared_ptr<AbstractXmlElement>;
		using Key = std::string;
		DbCore<T> newDb;
		XmlDocument newXDoc(xml, XmlDocument::str);
		std::vector<Sptr> records = newXDoc.descendents("dbRecord").select();
		for (auto pRecord : records)
		{
			Key key; DbElement<T> elem; T pl;
			std::vector<Sptr> pChildren = pRecord->children();
			for (auto pChild : pChildren)
			{
				if (pChild->tag() == "key")	key = pChild->children()[0]->value();
				else
				{
					std::vector<Sptr> pValueChildren = pChild->children();
					for (auto pValueChild : pValueChildren)
					{
						if (pValueChild->tag() == "name")
							elem.name(pValueChild->children()[0]->value());
						if (pValueChild->tag() == "description")
							elem.descrip(pValueChild->children()[0]->value());
						if (pValueChild->tag() == "date")
							elem.dateTime(pValueChild->children()[0]->value());
						if (pValueChild->tag() == "children")
						{
							std::vector<std::string> children;
							std::vector<Sptr> pChildren = pValueChild->children();
							for (auto pChild : pChildren)
							{
								children.push_back(pChild->children()[0]->value());
							}
							elem.children(children);
						}
						if (pValueChild->tag() == "payload")
						{
							pl = PayLoad::fromXmlElement(pValueChild);
							elem.payLoad(pl);
						}
					}
				}
			}
			newDb[key] = elem;
		}
		std::cout << "\n \nThe restored Database from xml is:";
		showDb(newDb);
		db_ = newDb;
	}

	//----< restores db contents from file, using restore >------------

	template<typename T>
	bool Persist<T>::restoreFromFile(const std::string& fileSpec)
	{
		std::ifstream in(fileSpec);
		if (!in.good())
		{
			std::cout << "\n  failed to open file";
			return false;
		}
		Xml xml;

		// build XML string from file

		while (in.good())
		{
			char ch = in.get();

			/////////////////////////////////////////////////////
			// This code removes whitespace from XML contents
			// but is unnecessart and adds a small CPU load.
			// I left it here because I intent to use it in
			// another package, for display.
			/////////////////////////////////////////////////////
			// if (ch == '\n' || ch == '\t' || ch == ' ')
			//   continue;
			/////////////////////////////////////////////////////

			if (!in.good())
				break;
			xml += ch;
		}
		in.close();

		restore(xml);
		return true;
	}

	/*
	* This is where the persistance of db takes place.
	*/

	template <typename T>
	typename Persist<T>::Xml Persist<T>::createXml()
	{
		std::cout << "\n  Creating XML representation of Database using XmlDocument";
		std::cout << "\n ----------------------------------------------------------------";
		using Sptr = std::shared_ptr<AbstractXmlElement>;
		Sptr pDb = makeTaggedElement("db");
		Sptr pDocElem = makeDocElement(pDb);
		XmlDocument xDoc(pDocElem);

		for (auto item : db_)
		{
			Sptr pRecord = makeTaggedElement("dbRecord");
			pDb->addChild(pRecord);
			Sptr pKey = makeTaggedElement("key", item.first);
			pRecord->addChild(pKey);

			Sptr pValue = makeTaggedElement("value");
			pRecord->addChild(pValue);
			Sptr pName = makeTaggedElement("name", item.second.name());
			pValue->addChild(pName);
			Sptr pDescrip = makeTaggedElement("description", item.second.descrip());
			pValue->addChild(pDescrip);
			Sptr pDate = makeTaggedElement("date", item.second.dateTime());
			pValue->addChild(pDate);

			SPtr pChildren = XmlProcessing::makeTaggedElement("children");
			Keys childKeys = item.second.children();
			for (Key child : childKeys)
			{
				SPtr pChild = XmlProcessing::makeTaggedElement("childKey", static_cast<std::string>(child));
				pChildren->addChild(pChild);
			}
			pValue->addChild(pChildren);
			Sptr pPayLoad = item.second.payLoad().toXmlElement();
			pValue->addChild(pPayLoad);
		}
		std::string Xml = xDoc.toString();
		return Xml;
	}


	//----< saves db contents to file, using save >--------------------

	template<typename T>
	bool Persist<T>::saveToFile(const std::string& fileSpec)
	{
		std::ofstream out(fileSpec);
		if (!out.good())
			return false;
		Persist<T> persist(db_);
		Xml xml = persist.createXml();
		out << xml;
		out.close();
		return true;
	}
}