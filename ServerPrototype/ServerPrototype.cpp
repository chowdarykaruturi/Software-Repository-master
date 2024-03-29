////////////////////////////////////////////////////////////////////////////////////////
// ServerPrototype.cpp - Console App that processes incoming messages                 //
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
* ---------------------
*  Package contains one class, Server, that contains a Message-Passing Communication
*  facility. It processes each message by invoking an installed callable object
*  defined by the message's command key.
*
*  Message handling runs on a child thread, so the Server main thread is free to do
*  any necessary background processing (none, so far).
*
*  public Interfaces:
*  ------------------
* getFiles() : returns files from specified path
* getDirs() : returns directories from specified path
* show() : displays message to the console
* echo() : returns an reply message for echo
* getFiles() : returns an reply message with file list in a directory
* getDirs() : returns an reply message with directories list
* getFilesforBrowse() : returns an reply message with file list in a directory
* getDirsforBrowse() : returns an reply message with directories list
* checkIn() : returns an reply message for check-in
* checkOut() : returns an reply message for check-out
* browse() : returns an reply message for browse
* fileTransfer() : returns an reply message for transferring file
* viewMetaData() : returns an reply message for viewing meta data

*  Required Files:
* -----------------
*  ServerPrototype.h, ServerPrototype.cpp
*  Comm.h, Comm.cpp, IComm.h
*  Message.h, Message.cpp
*  FileSystem.h, FileSystem.cpp
*  Utilities.h
*
*  Maintenance History:
* ----------------------
*  ver 1.0 : 4/10/2018
*  - first release
*/
#include "ServerPrototype.h"
#include "../FileSystem-Windows/FileSystemDemo/FileSystem.h"
#include <chrono>

namespace MsgPassComm = MsgPassingCommunication;

using namespace NoSqlDb;
using namespace FileSystem;
using Msg = MsgPassingCommunication::Message;


//----< returns files from specified path >------------------

Files Server::getFiles(const NoSqlDb::SearchPath& path)
{
  return Directory::getFiles(path);
}

//----< returns directories from specified path >------------------

Dirs Server::getDirs(const NoSqlDb::SearchPath& path)
{
  return Directory::getDirectories(path);
}

//----< forms metadata for the DbElement >------------------------------------------

DbElement<PayLoad> formMetaData(std::string name, std::string description, std::string child, std::string status, std::string categories, std::string path)
{
	DbElement<PayLoad> dbElem;
	dbElem.name(name);
	std::cout << "\n name is:" << name;
	dbElem.descrip(description);
	std::cout << "\n description is:" << description;
	Children children;
	std::string childr;
	std::istringstream ss(child);
	if (child != "") {
		while (std::getline(ss, childr, ':')) {
			if (childr != "") {
				children.push_back(childr);
				std::cout << "\n child is:" << childr << '\n';
			}
		}
	}

	dbElem.children(children);
	PayLoad p;

	std::string repoPath = RepoPath;
	if (path != "Root Directory")
		repoPath = repoPath + "\\" + path;
	p.filePath(repoPath);

	std::istringstream ss1(categories);
	std::string cat;
	if (categories != "") {
		while (std::getline(ss1, cat, ':')) {
			if (cat != "") {
				p.categories().push_back(cat);
				std::cout << "\ncategory is:" << cat << '\n';
			}
		}
	}

	p.status(status);
	std::cout << "\n status is:" << status;
	dbElem.payLoad(p);
	return dbElem;
}

//----< returns an reply message for viewing file text >------------------


void Server::viewFileText(Msg msg) {
	Msg reply;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command(msg.command());
	reply.name("viewfile");
	reply.setValueForAttribute("fileShown", msg.getValueForAttribute("fileShown"));
	reply.receivePath(msg.receivePath());
	DbElement<PayLoad> dbElem = repo_.ViewFileContent(msg.getValueForAttribute("filename"));
	PayLoad p = dbElem.payLoad();
	reply.file(msg.getValueForAttribute("filename"));
	reply.sendPath(p.filePath());
	std::cout << "\n Sending message :" + reply.command();
	postMessage(reply);

}

//----< returns an reply message for viewing meta data >------------------

void Server::viewMetaData(Msg msg) {
	Msg reply;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("viewmetadata");
	reply.name(msg.name());
	DbElement<PayLoad> dbElem = repo_.viewMetaData(msg.name());
	reply.setValueForAttribute("description", dbElem.descrip());
	reply.setValueForAttribute("datetime", dbElem.dateTime());
	Children children = dbElem.children();
	size_t count = 0;
	for (auto item : children) {
		std::string countStr = Utilities::Converter<size_t>::toString(++count);
		reply.attribute("child" + countStr, item);
	}

	PayLoad p = dbElem.payLoad();
	Categories categories = p.categories();
	size_t count1 = 0;
	for (auto item : categories) {
		std::string countStr = Utilities::Converter<size_t>::toString(++count1);
		reply.attribute("category" + countStr, item);
	}

	reply.setValueForAttribute("status", p.status());
	reply.setValueForAttribute("filepath", p.filePath());
	std::cout << "\n posting reply mesage";
	std::cout << "\n Sending message :" + reply.command();
	postMessage(reply);

}

//----< returns an reply message afetr doing checkout >------------------

void Server::doCheckout(Msg msg) {
	Msg reply;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("checkout");
	reply.name("checkout");
	reply.receivePath(msg.receivePath());
	Children files = repo_.checkingout(msg.getValueForAttribute("filename"));
	for (auto item : files) {
		reply.attribute("file", item);
		std::cout << "\n Sending message :" + reply.command();
		postMessage(reply);
	}
}

//----< returns an reply message afetr doing checkin >------------------

void  Server::docheckin(Msg msg) {
	Msg reply;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("checkin");
	DbElement<PayLoad> dbElem = formMetaData(msg.file(), msg.getValueForAttribute("description"),
		msg.getValueForAttribute("children"), msg.getValueForAttribute("status"),
		msg.getValueForAttribute("categories"),msg.getValueForAttribute("path"));
	std::string status = repo_.checkin(dbElem);
	reply.setValueForAttribute("status", status);
	std::cout << "\n Sending message :" + reply.command();
	postMessage(reply);
}

//----< returns an reply message with file list after querying on category >------------------

void Server::removefilesForCategory(Msg msg) {
	Msg reply = msg;
	std::string Category = reply.getValueForAttribute("category");
	Children  files = repo_.getfilesForcategory(Category);
	reply.to(msg.from());
	reply.from(msg.to());
	size_t count = 0;
	for (auto item : files)
	{
		std::string countStr = Utilities::Converter<size_t>::toString(++count);
		reply.attribute("file" + countStr, item);
	}
	std::cout << "\n Sending message :" + reply.command();
	postMessage(reply);
}

//----< returns an reply message with file list after querying on category >------------------

void Server::filesForCategory(Msg msg) {
	Msg reply = msg;
	std::string Category = reply.getValueForAttribute("category");
	Children  files = repo_.getfilesForcategory(Category);
	reply.to(msg.from());
	reply.from(msg.to());
	size_t count = 0;
	for (auto item : files)
	{
		std::string countStr = Utilities::Converter<size_t>::toString(++count);
		reply.attribute("file" + countStr, item);
	}
	std::cout << "\n Sending message :" + reply.command();
	postMessage(reply);
}

//----< returns an reply message with file list after querying on dependency >------------------

void Server::queryOnDependency(Msg msg) {
	Msg reply = msg;
	std::string filename = reply.getValueForAttribute("fname");
	Children  files = repo_.getfilesForDependency(filename);
	reply.to(msg.from());
	reply.from(msg.to());
	if (files.size() > 0)
		reply.setValueForAttribute("status", "Files Successfully fetched");
	else
		reply.setValueForAttribute("status", "There is no file with name in database");
	size_t count = 0;
	for (auto item : files)
	{
		std::string countStr = Utilities::Converter<size_t>::toString(++count);
		reply.attribute("file" + countStr, item);
	}
	std::cout << "\n Sending message :" + reply.command();
	postMessage(reply);
}

//----< returns an reply message with file list after querying on filename >------------------

void Server::queryOnFilename(Msg msg) {
	Msg reply = msg;
	std::string filename = reply.getValueForAttribute("fname");
	Children  files = repo_.getfilesForFilename(filename);
	reply.to(msg.from());
	reply.from(msg.to());
	if (files.size() > 0)
		reply.setValueForAttribute("status", "Files Successfully fetched");
	else
		reply.setValueForAttribute("status", "There is no file with name in database");
	size_t count = 0;
	for (auto item : files)
	{
		std::string countStr = Utilities::Converter<size_t>::toString(++count);
		reply.attribute("file" + countStr, item);
	}
	std::cout << "\n Sending message :" + reply.command();
	postMessage(reply);
}


//----< displays message to the console >------------------

template<typename T>
void show(const T& t, const std::string& msg)
{
  std::cout << "\n  " << msg.c_str();
  for (auto item : t)
  {
    std::cout << "\n    " << item.c_str();
  }
}

//----< returns an reply message for echo >------------------

std::function<Msg(Msg)> echo = [](Msg msg) {
  Msg reply = msg;
  reply.to(msg.from());
  reply.from(msg.to());
  return reply;
};

//----< returns an reply message with file list in a directory >------------------

std::function<Msg(Msg)> getFiles = [](Msg msg) {
  Msg reply;
  reply.to(msg.from());
  reply.from(msg.to());
  reply.command("getFiles");
  std::string path = msg.value("path");
  if (path != "")
  {
    std::string searchPath = storageRoot;
    if (path != ".")
      searchPath = searchPath + "\\" + path;
    Files files = Server::getFiles(searchPath);
    size_t count = 0;
    for (auto item : files)
    {
      std::string countStr = Utilities::Converter<size_t>::toString(++count);
      reply.attribute("file" + countStr, item);
    }
  }
  else
  {
    std::cout << "\n  getFiles message did not define a path attribute";
  }
  return reply;
};

//----< returns an reply message with file list in a directory >------------------

std::function<Msg(Msg)> getAllFiles = [](Msg msg) {
	Msg reply;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("getAllFiles");
	std::string path = msg.value("path");
	if (path != "")
	{
		std::string searchPath = storageRoot;
		if (path != ".")
			searchPath = searchPath ;
		Files files = Server::getFiles(searchPath);
		size_t count = 0;
		for (auto item : files)
		{
			std::string countStr = Utilities::Converter<size_t>::toString(++count);
			reply.attribute("file" + countStr, item);
		}
	}
	else
	{
		std::cout << "\n  getFiles message did not define a path attribute";
	}
	return reply;
};

//----< returns an reply message with directories list >------------------

std::function<Msg(Msg)> getDirs = [](Msg msg) {
  Msg reply;
  reply.to(msg.from());
  reply.from(msg.to());
  reply.command("getDirs");
  std::string path = msg.value("path");
  if (path != "")
  {
    std::string searchPath = storageRoot;
    if (path != ".")
      searchPath = searchPath + "\\" + path;
    Files dirs = Server::getDirs(searchPath);
    size_t count = 0;
    for (auto item : dirs)
    {
      if (item != ".." && item != ".")
      {
        std::string countStr = Utilities::Converter<size_t>::toString(++count);
        reply.attribute("dir" + countStr, item);
      }
    }
  }
  else
  {
    std::cout << "\n  getDirs message did not define a path attribute";
  }
  return reply;
};

//----< returns an reply message with file list in a directory >------------------

std::function<Msg(Msg)> getFilesforBrowse = [](Msg msg) {
	Msg reply;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("getFilesforBrowse");
	std::string path = msg.value("path");
	if (path != "")
	{
		std::string searchPath = storageRoot;
		if (path != ".")
			searchPath = searchPath + "\\" + path;
		Files files = Server::getFiles(searchPath);
		size_t count = 0;
		for (auto item : files)
		{
			std::string countStr = Utilities::Converter<size_t>::toString(++count);
			reply.attribute("file" + countStr, item);
		}
	}
	else
	{
		std::cout << "\n  getFiles message did not define a path attribute";
	}
	return reply;
};


//----< returns an reply message with directories list >------------------

std::function<Msg(Msg)> getDirsForChkin = [](Msg msg) {
	Msg reply;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("getDirsForCheckin");
	std::string path = msg.value("path");
	if (path != "")
	{
		std::string searchPath = storageRoot;
		if (path != ".")
			searchPath = searchPath + "\\" + path;
		Files dirs = Server::getDirs(searchPath);
		size_t count = 0;
		for (auto item : dirs)
		{
			if (item != ".." && item != ".")
			{
				std::string countStr = Utilities::Converter<size_t>::toString(++count);
				reply.attribute("dir" + countStr, item);
			}
		}
	}
	else
	{
		std::cout << "\n  getDirs message did not define a path attribute";
	}
	return reply;
};


//----< returns an reply message with directories list >------------------

std::function<Msg(Msg)> getDirsforBrowse = [](Msg msg) {
	Msg reply;
	reply.to(msg.from());
	reply.from(msg.to());
	reply.command("getDirsforBrowse");
	std::string path = msg.value("path");
	if (path != "")
	{
		std::string searchPath = storageRoot;
		if (path != ".")
			searchPath = searchPath + "\\" + path;
		Files dirs = Server::getDirs(searchPath);
		size_t count = 0;
		for (auto item : dirs)
		{
			if (item != ".." && item != ".")
			{
				std::string countStr = Utilities::Converter<size_t>::toString(++count);
				reply.attribute("dir" + countStr, item);
			}
		}
	}
	else
	{
		std::cout << "\n  getDirs message did not define a path attribute";
	}
	return reply;
};



//----< test stub >------------------

int main()
{
  std::cout << "\n Repository Server";
  std::cout << "\n ==========================";
  std::cout << "\n";

  Server server(serverEndPoint, "ServerPrototype");
  server.start();

  std::cout << "\nDemonstrating Requirement #1";
  std::cout << "\n----------------------------";
  std::cout << "\n  "
	  << typeid(std::function<bool()>).name()
	  << ", declared in this function, "
	  << "\n  is only valid for C++11 and later versions.";

  std::cout << "\nDemonstrating Requirement #4 & #5 & #6";
  std::cout << "\n---------------------------------------";
  std::cout << "\n The sockets have been used for message passing communication";
  std::cout << "\n All the communication used between the client and server are achieved using"
	  << "\n asynchronous message passing communication channel and the channel uses HTTP style messages"
	  << "\n and the file transfer between the Repository server and client is sent and received through blocks.";
	  

  std::cout << "\n   message processing";
  std::cout << "\n ----------------------------";
  server.addMsgProc("echo", echo);
  server.addMsgProc("getFiles", getFiles);
  server.addMsgProc("getAllFiles", getAllFiles);
  server.addMsgProc("getDirs", getDirs);
  server.addMsgProc("getFilesforBrowse", getFilesforBrowse);
  server.addMsgProc("getDirsforBrowse", getDirsforBrowse);
  server.addMsgProc("serverQuit", echo);
  server.addMsgProc("getDirsForCheckin", getDirsForChkin);
  server.processMessages();
  
  Msg msg(serverEndPoint, serverEndPoint);  // send to self
  msg.name("msgToSelf");
  msg.command("echo");
  msg.attribute("verbose", "show me");
  server.postMessage(msg);
  std::this_thread::sleep_for(std::chrono::milliseconds(1000));

  std::cout << "\n  press enter to exit";
  std::cin.get();
  std::cout << "\n";

  msg.command("serverQuit");
  server.postMessage(msg);
  server.stop();
  return 0;
}

