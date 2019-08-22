#pragma once
////////////////////////////////////////////////////////////////////////////////////////
// ServerPrototype.h - Console App that processes incoming messages                   //
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

*  public Interfaces:
*  ------------------
* Server() : initialize server endpoint and give server a name
* start() : start server's instance of Comm
* stop() : stop Comm instance
* postMessage() : pass message to Comm for sending
* getMessage() : get message from Comm
* addMsgProc() : add ServerProc callable object to server's dispatcher
* processMessages() : start processing messages on child thread

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
#include <vector>
#include <string>
#include <unordered_map>
#include <functional>
#include <thread>
#include "../CppCommWithFileXfer/Message/Message.h"
#include "../CppCommWithFileXfer/MsgPassingComm/Comm.h"
#include <windows.h>
#include <tchar.h>
#include "../RepositoryCore/RepositoryCore.h"
#include "../PayLoad/PayLoad.h"
#include "../Check-In/CheckIn.h"
#include "../DbCore/Definitions.h"

namespace NoSqlDb
{
  using File = std::string;

  using Files = std::vector<File>;
  using Dir = std::string;
  using Dirs = std::vector<Dir>;
  using SearchPath = std::string;
  using Key = std::string;
  using Msg = MsgPassingCommunication::Message;
  using ServerProc = std::function<Msg(Msg)>;
  using MsgDispatcher = std::unordered_map<Key,ServerProc>;
  
  const SearchPath storageRoot = "../ServerStorage";  // root for all server file storage
  const MsgPassingCommunication::EndPoint serverEndPoint("localhost", 8080);  // listening endpoint


  class Server
  {
  public:
	 Server();
    Server(MsgPassingCommunication::EndPoint ep, const std::string& name);
    void start();
    void stop();
    void addMsgProc(Key key, ServerProc proc);
    void processMessages();
    void postMessage(MsgPassingCommunication::Message msg);
	void sendFile(MsgPassingCommunication::Message msg);
    MsgPassingCommunication::Message getMessage();
    static Dirs getDirs(const SearchPath& path = storageRoot);
    static Files getFiles(const SearchPath& path = storageRoot);
	void filesForCategory( Msg msg);
	void queryOnFilename(Msg msg);
	void queryOnDependency(Msg msg);
	void removefilesForCategory(Msg msg);
	void docheckin(Msg msg);
	void doCheckout(Msg msg);
	void viewMetaData(Msg msg);
	void viewFileText(Msg msg);
  private:
    MsgPassingCommunication::Comm comm_;
    MsgDispatcher dispatcher_;
    std::thread msgProcThrd_;
	NoSqlDb::RepositoryCore<PayLoad> repo_;
  };

  //----< initialize server endpoint and give server a name >----------

  inline Server::Server(MsgPassingCommunication::EndPoint ep, const std::string& name)
    : comm_(ep, name) {}

  //----< start server's instance of Comm >----------------------------

  inline void Server::start()
  {
    comm_.start();
  }
  //----< stop Comm instance >-----------------------------------------

  inline void Server::stop()
  {
    if(msgProcThrd_.joinable())
      msgProcThrd_.join();
    comm_.stop();
  }
  //----< pass message to Comm for sending >---------------------------

  inline void Server::postMessage(MsgPassingCommunication::Message msg)
  {
    comm_.postMessage(msg);
  }
  //----< get message from Comm >--------------------------------------

  inline MsgPassingCommunication::Message Server::getMessage()
  {
    Msg msg = comm_.getMessage();
    return msg;
  }
  //----< add ServerProc callable object to server's dispatcher >------

  inline void Server::addMsgProc(Key key, ServerProc proc)
  {
    dispatcher_[key] = proc;
  }

  //----< start processing messages on child thread >------------------

  inline void Server::processMessages()
  {
    auto proc = [&]()
    {
      if (dispatcher_.size() == 0)
      {
        std::cout << "\n  no server procs to call";
        return;
      }
      while (true)
      {
        Msg msg = getMessage();
        std::cout << "\nReceived message: " << msg.command() << " from " << msg.from().toString();
        if (msg.command() == "serverQuit")
          break;

		if (msg.command() == "checkin" && msg.to().port != msg.from().port) 
			docheckin(msg);	
		if (msg.command() == "filesofcategory" && msg.to().port != msg.from().port) 
			 filesForCategory(msg);		
		if (msg.command() == "queryonfilename" && msg.to().port != msg.from().port)
			queryOnFilename(msg);
		if (msg.command() == "queryondependency" && msg.to().port != msg.from().port)
			queryOnDependency(msg);
		if (msg.command() == "removefilesofcategory" && msg.to().port != msg.from().port) 
			 removefilesForCategory(msg);
		if (msg.command() == "checkout" && msg.to().port != msg.from().port) 
			 doCheckout(msg);
		if (msg.command() == "viewmetadata" && msg.to().port != msg.from().port) 
			viewMetaData(msg);
		if (msg.command() == "viewfile" && msg.to().port != msg.from().port) 
			viewFileText(msg);
		
		if (msg.to().port != msg.from().port && (msg.command() =="echo" || msg.command() =="getDirsForCheckin" || msg.command() == "getFiles" || msg.command() == "getAllFiles" || msg.command() == "getDirs" || msg.command() == "getFilesforBrowse" || msg.command() == "getDirsforBrowse" || msg.command() == "serverQuit" ))  // avoid infinite message loop
		{
          Msg reply = dispatcher_[msg.command()](msg);
		  std::cout << "\n Sending message :" + reply.command();
		  msg.show();
		  postMessage(reply);
		  reply.show();
        }
        else  std::cout << "\n  server attempting to post to self";
      }
      std::cout << "\n  server message processing thread is shutting down";
    };
    std::thread t(proc);
    std::cout << "\n  starting server thread to process messages";
    msgProcThrd_ = std::move(t);
  }
}