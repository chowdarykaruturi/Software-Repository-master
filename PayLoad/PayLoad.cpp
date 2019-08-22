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

#include "PayLoad.h"
#include "../CppCommWithFileXfer/Utilities/StringUtilities.h"

using namespace NoSqlDb;
using namespace XmlProcessing;

#ifdef TEST_PAYLOAD

int main()
{
	Utilities::Title("Demonstrating Application Specific PayLoad class");
	Utilities::putline();

	using Sptr = std::shared_ptr<AbstractXmlElement>;

	Utilities::title("creating xml string from payload instance");
	PayLoad pl;
	pl.filePath("demo payload value");
	pl.categories().push_back("cat1");
	pl.categories().push_back("cat2");
	Sptr sPpl = pl.toXmlElement();
	XmlDocument doc(sPpl);
	std::cout << doc.toString();
	Utilities::putline();

	Utilities::title("creating payload instance from an XmlElement");
	PayLoad newPl = pl.fromXmlElement(sPpl);
	std::cout << "\n  payload value = " << pl.filePath();
	std::cout << "\n  payload categories:\n    ";
	for (auto cat : newPl.categories())
	{
		std::cout << cat << " ";
	}
	std::cout << "\n\n";
}
#endif
