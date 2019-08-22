#pragma once
///////////////////////////////////////////////////////////////////////
// IPayLoad.h - declare language for serializing a PayLoad instance  //
// ver 1.0                                                                            //
//                                                                                    //
//Language     : C++                                                                  //
// Platform    : Lenovo 510S Ideapad, Win Pro 10, Visual Studio 2017                  //
// Application : CSE-687 OOD Project 4                                                //
// Author      : Akhil Karuturi,Syracuse University                        //
// Source      : Dr. Jim Fawcett, EECS, SU                                            //
////////////////////////////////////////////////////////////////////////////////////////
/*
*  Note:
*  - It is common practice to define C++ interfaces as structs with
*    pure virtual methods, including a virtual destructor.
*  - If a class derives from IPayLoad and fails to implement the
*    pure virtual method toXmlElement, the class will be abstract
*    and compilation will fail for statements that declare the PayLoad.
*  - static methods cannot be virtual, and we need fromXmlElement to
*    be static, because we may not have a PayLoad instance to invoke
*    it when deserializing.
*  - IPayLoad defers implementation to the PayLoad class.  If that
*    class fails to implement the method, builds will fail to link.
*/
namespace NoSqlDb
{
#include "../XmlDocument/XmlElement.h"

	using Xml = std::string;
	using Sptr = std::shared_ptr<XmlProcessing::AbstractXmlElement>;

	template<typename P>
	struct IPayLoad
	{
		virtual Sptr toXmlElement() = 0;
		static P fromXmlElement(Sptr elem);
		virtual ~IPayLoad() {};
	};
}