////////////////////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - GUI for the Client                                            //
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
 * This package provides a WPF-based GUI for client.  It's 
 * responsibilities are to:
 * - Provide a display of directory contents of a remote ServerPrototype.
 * - It provides a subdirectory list and a filelist for the selected directory.
 * - You can navigate into subdirectories by double-clicking on subdirectory
 *   or the parent directory, indicated by the name "..".
 *   Requesting Repository server for check-in,check-out and browse
 *   
 * Required Files:
 * ---------------
 * Mainwindow.xaml, MainWindow.xaml.cs
 * Translater.dll
 * 
 *  public Interfaces:
 * ------------------
 * processMessages() : process incoming messages on child thread
 * clearDirs() : clears directory list
 * clearDirsBr() : clears directory list
 * addDir() : adds directory list to the GUI 
 * addDirBr() : adds directory list to the GUI 
 * insertParent() : function dispatched by child thread to main thread
 * insertParentBr() : function dispatched by child thread to main thread
 * clearFiles() : clears file list
 * clearFilesBr() : clears the file list
 * addFile() : adds the file to the list of files
 * addFileBr() : adds the file to the list of files in browse tab
 * addClientProc() : add client processing for message with key into the dictionary
 * DispatcherLoadGetDirs() : load getDirs processing into dispatcher dictionary
 * DispatcherLoadGetFiles() : load getFiles processing into dispatcher dictionary 
 * DispatcherLoadGetDirsforBrowse() : load getDirs processing into dispatcher dictionary
 * DispatcherLoadGetFilesforBrowse() : load getFiles processing into dispatcher dictionary 
 * DispatcherCheckIn() : Dispatches check-in status to status bar
 * DispatcherCheckOut() : Dispatches check-out status to status bar
 * DispatcherBrowse() : Dispatches browse status to status bar and displays the file in pop-up window
 * DispatcherViewMetadata() : Dispatches view meta data status to status bar
 * loadDispatcher() : load all dispatcher processing
 * Window_Loaded() : start Comm, and demonstrates all requirements
 * checkin_and_checkout_and_viewmetadata() : Requests server for check-in, check-out and viewing meta data
 * view_Content_of_file_Click() : Requests server for the content of the specified file
 * removeFirstDir() : strip off name of first part of path
 * DirList_MouseDoubleClick() : respond to mouse double-click on dir name
 * dirList_MouseDoubleClick() : Requests server for files in that directory
 * Check_for_null_Checkin() : Checks whether client have given enough input in GUI for check-in
 * Check_in_click() : Requests Repository server for check-in
 * check_for_null_for_Checkout() : Checks whether client have given enough input in GUI for check-out
 * check_out_click() : Requests Repository server for check-out
 * SelectFile_Click() : Provides capability to select a file by opening a dialog box
 * get_Check_out_path() : Provides capability to select a directory by opening a dialog box
 * viewSelectedFile() : Opens a new pop up window to view the content of file
 * view_Meta_Data() : Requests Repository server for viewing meta data for a file in server
 * connect_to_server_click() : Requests Repository server for connecting 
 * 
 * Maintenance History:
 * --------------------
 * ver 1.0 : 9 Apr 2018
 * - first release
 * 
 */

// Translater has to be statically linked with CommLibWrapper
// - loader can't find Translater.dll dependent CommLibWrapper.dll
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using MsgPassingCommunication;
using System.Windows.Forms;
using System.IO;

namespace WpfApp1
{
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private Stack<string> pathStack_ = new Stack<string>();
    private Stack<string> PathStack_ = new Stack<string>();
    private Translater translater;
    private CsEndPoint endPoint_;
    private Thread rcvThrd = null;
    private Dictionary<string, Action<CsMessage>> dispatcher_ 
      = new Dictionary<string, Action<CsMessage>>();
    private bool fileShown = false;

    //----< process incoming messages on child thread >----------------

    private void processMessages()
    {
            Console.Write("\n Starting Receiving Thread for Client\n");
  

      ThreadStart thrdProc = () => {
        while (true)
        {
          CsMessage msg = translater.getMessage();
              try
              {
                  string msgId = msg.value("command");

                  Console.Write("\n The received msg with command is " + msgId);
                  if (dispatcher_.ContainsKey(msgId))
                  {
                      dispatcher_[msgId].Invoke(msg);
                  }
              }catch(Exception e)
              {
                  Console.WriteLine("\n The exception is" + e);
              }
        }
      };
      rcvThrd = new Thread(thrdProc);
      rcvThrd.IsBackground = true;
      rcvThrd.Start();
    }

    //----< function dispatched by child thread to main thread >-------

    private void clearDirs()
    {
      DirList.Items.Clear();
    }

    //----< function dispatched by child thread to main thread >-------

    private void clearDirsBr()
    {
            directoriesList.Items.Clear();
    }

    private void clearDirsCkin()
    {
      directoriesBox.Items.Clear();
    }

    //----< function dispatched by child thread to main thread >-------

    private void addDir(string dir)
    {
      DirList.Items.Add(dir);
    }

     //----< function dispatched by child thread to main thread >-------

    private void addDirBr(string dir)
    {
       directoriesList.Items.Add(dir);
    }

        //----< function dispatched by child thread to main thread >-------

    private void addDirChkin(string dir)
    {
      directoriesBox.Items.Add(dir);
    }

    //----< function dispatched by child thread to main thread >-------

    private void insertParent()
    {
      DirList.Items.Insert(0, "..");
    }

    //----< function dispatched by child thread to main thread >-------

    private void insertParentBr()
    {
     directoriesList.Items.Insert(0, "..");
    }

     //----< function dispatched by child thread to main thread >-------

     private void clearFiles()
    {
      FileList.Items.Clear();
    }

        //----< function dispatched by child thread to main thread >-------

        private void clearFilesBrowse()
        {
            queryFiles.Items.Clear();
        }

        //----< function dispatched by child thread to main thread >-------

        private void clearAllFiles()
        {
            leftChildrenBox.Items.Clear();
        }

        //----< function dispatched by child thread to main thread >-------

     private void clearFilesBr()
     {
        fileList.Items.Clear();
     }

     //----< function dispatched by child thread to main thread >-------

    private void addFile(string file)
    {
      FileList.Items.Add(file);
    }

        //----< function dispatched by child thread to main thread >-------

   private void addFileBrowse(string file)
   {
      queryFiles.Items.Add(file);
   }


        //----< function dispatched by child thread to main thread >-------

    private void deleteFile(string file)
    {
       leftChildrenBox.Items.Remove(file);
    }

        //----< function dispatched by child thread to main thread >-------

    private void addAllFile(string file)
    {
       leftChildrenBox.Items.Add(file);
    }

        //----< function dispatched by child thread to main thread >-------

    private void addFileBr(string file)
    {
        fileList.Items.Add(file);
    }

    //----< add client processing for message with key >---------------

    private void addClientProc(string key, Action<CsMessage> clientProc)
    {
      dispatcher_[key] = clientProc;
    }

    //----< load getDirs processing into dispatcher dictionary >-------

    private void DispatcherLoadGetDirs()
    {
      Action<CsMessage> getDirs = (CsMessage rcvMsg) =>
      {
          Action clrDirs = () =>
        {
          clearDirs();
        };
        Dispatcher.Invoke(clrDirs, new Object[] { });
        var enumer = rcvMsg.attributes.GetEnumerator();
        while (enumer.MoveNext())
        {
          string key = enumer.Current.Key;
          if (key.Contains("dir"))
          {
            Action<string> doDir = (string dir) =>
            {
              addDir(dir);
            };
            Dispatcher.Invoke(doDir, new Object[] { enumer.Current.Value });
          }
        }
        Action insertUp = () =>
        {
          insertParent();
        };
        Dispatcher.Invoke(insertUp, new Object[] { });
      };
      addClientProc("getDirs", getDirs);
    }

        //----< load files of a category processing into dispatcher dictionary >-------

        private void DispatcherFilesOfCategory()
        {
            Action<CsMessage> getFiles = (CsMessage rcvMsg) =>
            {
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("file"))
                    {
                        Action<string> doFile = (string file) =>
                        {
                            addAllFile(file);
                        };
                        Dispatcher.Invoke(doFile, new Object[] { enumer.Current.Value });
                    }
                }
            };
            addClientProc("filesofcategory", getFiles);
        }

        //----< load files of a  query on filename processing into dispatcher dictionary >-------

        private void DispatcherFilesOfFilename()
        {
            Action<CsMessage> getFiles = (CsMessage rcvMsg) =>
            {
                rcvMsg.show();
                Console.Write("\n " + rcvMsg.value("status"));
                Action clrFiles = () =>
                {
                    clearFilesBrowse();
                };
                Dispatcher.Invoke(clrFiles, new Object[] { });
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("file"))
                    {
                        Action<string> doFile = (string file) =>
                        {
                            addFileBrowse(file);
                        };
                        Dispatcher.Invoke(doFile, new Object[] { enumer.Current.Value });
                    }
                }
                Dispatcher.Invoke((Action)delegate
                {
                    statusbar.Text = rcvMsg.value("status");
                });
            };
            addClientProc("queryonfilename", getFiles);
            addClientProc("queryondependency", getFiles);  
        }

        //----< remove files of a category processing into dispatcher dictionary >-------

        private void DispatcherRemoveFilesOfCategory()
        {
            Action<CsMessage> getFiles = (CsMessage rcvMsg) =>
            {
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("file"))
                    {
                        Action<string> doFile = (string file) =>
                        {
                            deleteFile(file);
                        };
                        Dispatcher.Invoke(doFile, new Object[] { enumer.Current.Value });
                    }
                }
            };
            addClientProc("removefilesofcategory", getFiles);
        }


        //----< load getFiles processing into dispatcher dictionary >------

        private void DispatcherLoadGetFiles()
    {
      Action<CsMessage> getFiles = (CsMessage rcvMsg) =>
      {
        Action clrFiles = () =>
        {
          clearFiles();
        };
        Dispatcher.Invoke(clrFiles, new Object[] { });
        var enumer = rcvMsg.attributes.GetEnumerator();
        while (enumer.MoveNext())
        {
          string key = enumer.Current.Key;
          if (key.Contains("file"))
          {
            Action<string> doFile = (string file) =>
            {
              addFile(file);
            };
            Dispatcher.Invoke(doFile, new Object[] { enumer.Current.Value });
          }
        }
      };
      addClientProc("getFiles", getFiles);
    }

        //----< load getDirs processing into dispatcher dictionary >-------

        private void DispatcherLoadGetDirsforBrowse()
        {
            
            Action<CsMessage> getDirsBr = (CsMessage rcvMsg) =>
            {
                Action clrDirsBr = () =>
                {
                    clearDirsBr();
                };
                Dispatcher.Invoke(clrDirsBr, new Object[] { });
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("dir"))
                    {
                        Action<string> doDirBr = (string dir) =>
                        {
                            addDirBr(dir);
                        };
                        Dispatcher.Invoke(doDirBr, new Object[] { enumer.Current.Value });
                    }
                }
                Action insertUp = () =>
                {
                    insertParentBr();
                };
                Dispatcher.Invoke(insertUp, new Object[] { });
            };
            addClientProc("getDirsforBrowse", getDirsBr);
        }

        //----< load getDirs processing into dispatcher dictionary >-------

        private void DispatcherLoadGetDirsforCheckin()
        {
            Action<CsMessage> getDirsChkin = (CsMessage rcvMsg) =>
            {
                Action clrDirsBr = () =>
                {
                    clearDirsCkin();
                };
                Dispatcher.Invoke(clrDirsBr, new Object[] { });
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("dir"))
                    {
                        Action<string> doDirCkin = (string dir) =>
                        {
                            addDirChkin(dir);
                        };
                        Dispatcher.Invoke(doDirCkin, new Object[] { enumer.Current.Value });
                    }

                }
                Dispatcher.Invoke((Action)delegate
                {
                    directoriesBox.Items.Insert(0, "Root Directory");
                });
            };
            addClientProc("getDirsForCheckin", getDirsChkin);
        }

        //----< load getFiles processing into dispatcher dictionary >------

        private void DispatcherLoadGetFilesforBrowse()
        {

            Action<CsMessage> getFilesBr = (CsMessage rcvMsg) =>
            {
                Action clrFilesBr = () =>
                {
                    clearFilesBr();
                };
                Dispatcher.Invoke(clrFilesBr, new Object[] { });
                var enumer = rcvMsg.attributes.GetEnumerator();
                while (enumer.MoveNext())
                {
                    string key = enumer.Current.Key;
                    if (key.Contains("file"))
                    {
                        Action<string> doFileBr = (string file) =>
                        {
                            addFileBr(file);
                        };
                        Dispatcher.Invoke(doFileBr, new Object[] { enumer.Current.Value });
                    }
                }
            };
            addClientProc("getFilesforBrowse", getFilesBr);
        }

        //----< Dispatches check-in status to status bar  >------

        private void DispatcherCheckIn()
        {

            Action<CsMessage> checkin = (CsMessage rcvMsg) =>
            {
                string status = rcvMsg.value("status");
                Console.Write("\n " + status);
                Dispatcher.Invoke((Action)delegate
                        {
                            statusbar.Text = status;
                        });
 
            };
            addClientProc("checkin", checkin);
        }

        //----< Dispatches check-out status to status bar  >------

        private void DispatcherCheckOut()
        {
           
            Action<CsMessage> checkout = (CsMessage rcvMsg) =>
            {
                Console.Write("\nFile Checked-out successfully");
                Dispatcher.Invoke((Action)delegate
                {
                    statusbar.Text = "File Checked-out successfully";
                });
            };
            addClientProc("checkout", checkout);
        }

        //----< Dispatches browse status to status bar and displays the file in pop-up window >------
        
        private void DispatcherBrowse()
        {
            Action<CsMessage> browse = (CsMessage rcvMsg) =>
            {
                String path = rcvMsg.value("receivePath");
                String file = rcvMsg.value("file");
                String fullspec = System.IO.Path.GetFullPath(path+"\\"+file);

                if (!fileShown)
                {
                    viewSelectedFile(path + "\\" + file);
                }

                Dispatcher.Invoke((Action)delegate
                    {
                        statusbar.Text = "Viewed File Text successfully";
                    });
              
                if (rcvMsg.value("fileShown") == "false")
                {
                    fileShown = true;
                }
            };
            addClientProc("viewfile", browse);
        }

        //----< Forms meta data string to display it on the GUI >------

        string formMetaDataString(CsMessage msg)
        {

            string metadata = "Name: " + msg.value("name") + "\n" +
                           "Description: " + msg.value("description") + "\n" +
                           "Date Time: " + msg.value("datetime") + "\nChildren: ";
                           

            var enumer = msg.attributes.GetEnumerator();
            while (enumer.MoveNext())
            {
                string key = enumer.Current.Key;
                if (key.Contains("child"))
                {
                    metadata = metadata + enumer.Current.Value + "  ";
                }
            }

            metadata = metadata + "\nStatus: " + msg.value("status") + "\n"+
                                 "Filepath: " + msg.value("filepath") + "\n"+"Categories: ";

         var enumer1 = msg.attributes.GetEnumerator();
            while (enumer1.MoveNext())
            {
                string key = enumer1.Current.Key;
                if (key.Contains("category"))
                {
                    metadata = metadata + enumer1.Current.Value + "  ";
                }
            }
            return metadata;
        }

        //----< Dispatches view meta data status to status bar  >------

        private void DispatcherViewMetadata()
        {
            Action<CsMessage> viewmetadata = (CsMessage msg) =>
             {   
                 string metadata = formMetaDataString(msg);
                 Console.Write("\n " + metadata);
                 Dispatcher.Invoke((Action)delegate
                 {
                     main.Inlines.Clear();
                     main.Inlines.Add(new Run(
                        metadata

                        ));
                     statusbar.Text = "Viewed Meta data successfully";
                 });
             };
            addClientProc("viewmetadata", viewmetadata);
        }

        //----< load all dispatcher processing >---------------------------

    private void loadDispatcher()
    {
      DispatcherLoadGetDirs();
      DispatcherLoadGetFiles();
      DispatcherLoadGetDirsforBrowse();
      DispatcherLoadGetFilesforBrowse();
      DispatcherCheckIn();
      DispatcherCheckOut();
      DispatcherViewMetadata();
      DispatcherBrowse();
      DispatcherFilesOfCategory();
      DispatcherRemoveFilesOfCategory();
      DispatcherLoadGetDirsforCheckin();
      DispatcherFilesOfFilename();
    }

    //----< start Comm, and demonstrates all requirements >------

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      Console.Write("\n This is client");
      Console.Write("\n----------------");
      string[] args = System.Environment.GetCommandLineArgs();
      Console.Write("\n The port of this client is " + args[1]);
      Console.Write("\n Demonstrating Req #1");
      Console.Write("\n-------------------");
      Console.Write("\n The GUI is developed using WPF and the .net framework is used");
      Console.Write("\n Demonstrating Req #2 & #3");
      Console.Write("\n-------------------");
      Console.Write("\n The GUI have tabs for connecting to server," +
          "\n check-in,check-out,browse specified package descriptions" +
          "\n double clicking on file in browse tab displays text of the file in pop up window" +
          "\n and also the browse tab have functionality of viewing repository contents " +
          "\n There is one more browse tab in GUI where the client can query on database based on " +
          "\n filenames or dependency names or versions.\n\n");
     Console.Write("\n In the check-in tab, the categories selection drop down, if selected " +
                "\n a category  then files of that category are queried from database and displayed " +
                "\n on the list box, hence providing user to select dependencies of only those categories");
      Console.Write("\n\n In check-in tab, the list of directories in the server are displayed" +
                "\n he can choose the directory he wants to check-in to");
      Console.Write("\nIam demonstrating all the requirements on only one client inorder to" +
                "\n escape the same demonstartion twice");

      endPoint_ = new CsEndPoint();
      endPoint_.machineAddress = "localhost";
      int arg = int.Parse(args[1]);
      endPoint_.port = arg;
      translater = new Translater();
      translater.listen(endPoint_);
      processMessages();
      loadDispatcher();

      if (arg == 8081)
      {
       Console.Write("\n Demonstrating Req#7");
       Console.Write("\n---------------------");
        Console.Write("\n The below series of tests demonstrates all the requirements");
       Console.Write("\n Demonstrating Check-in");
         Console.Write("\n------------------------");
         demo_checkin();
         Console.Write("\n Demonstrating Check-out");
         Console.Write("\n------------------------");
         demo_checkout();
         Console.Write("\n Demonstrating Browse");
         Console.Write("\n------------------------");
         demo_browse();
        }
      getDirs_getFiles();
    }

        private void getDirs_getFiles()
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;

            PathTextBlock.Text = "Storage";
            pathTextBlock.Text = "Storage";
            pathStack_.Push("../ServerStorage");
            PathStack_.Push("../ServerStorage");
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getDirs");
            msg.add("path", pathStack_.Peek());
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);
            msg.remove("command");

            msg.add("command", "getDirsForCheckin");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);
            msg.remove("command");
            msg.add("command", "getFiles");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);
            msg.remove("command");
            msg.add("command", "getDirsforBrowse");
            msg.remove("path");
            msg.add("path", PathStack_.Peek());
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);
            msg.remove("command");
            msg.add("command", "getFilesforBrowse");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);
        }


        //----< Demonstrates check-in functionality >------

        private void demo_checkin()
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "checkin");
            msg.add("file", "Message.cpp");
            msg.add("name", "checkin");
            msg.add("sendPath", "../../../../ClientStorage");
            msg.add("description", "Messaging structure");
            msg.add("categories", "Messaging:");
            msg.add("status", "Open");
            msg.add("path", "Root Directory");
            Console.Write("\n Trying to check-in a file \"Message.cpp\" with open status");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);

            msg.remove("status");
            msg.add("status", "Closed");
            Console.Write("\n Trying to check-in a file \"Message.cpp\" again, the old file will be overridden, " +
                "\n now checking-in with closed status");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);

            Console.Write("\n Trying to check-in a file \"Message.cpp\" again,the file  is now in closed state, " +
                "\n the new version file will be created");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);

            Console.Write("\n Trying to check-in a file \"Message.cpp\" again,version number increases ");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);

            demo_checkin2();
        }

        //----< Demonstrates check-in functionality >------

        private void demo_checkin2()
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "checkin");
            msg.add("file", "Message.h");
            msg.add("name", "checkin");
            msg.add("sendPath", "../../../../ClientStorage");
            msg.add("description", "Messaging structure");
            msg.add("categories", "Messaging:");
            msg.add("status", "Open");
            msg.add("children", "Message.cpp:");
            msg.add("path", "Root Directory");
            Console.Write("\n Trying to check-in a file \"Message.h\" with open status,that have children \"Message.cpp\"");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);

            CsMessage msg1 = new CsMessage();
            msg1.add("to", CsEndPoint.toString(serverEndPoint));
            msg1.add("from", CsEndPoint.toString(endPoint_));
            msg1.add("command", "checkin");
            msg1.add("file", "Comm.h");
            msg1.add("name", "checkin");
            msg1.add("sendPath", "../../../../ClientStorage");
            msg1.add("description", "Communication capability");
            msg1.add("categories", "Comm:Messaging:");
            msg1.add("status", "Open");
            msg1.add("children", "Message.h:");
            msg1.add("path", "Root Directory");
            Console.Write("\n Trying to check-in a file \"Comm.h\" that has dependency file \"Message.h\" which is in open status " +
                "\n so the check-in will fail");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg1);

            msg.remove("status");
            msg.add("status", "Closed");
            Console.Write("\n Trying to check-in a file \"Message.h\" again with closed status, as the file was in open status, same file will be replaced");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);

            Console.Write("\n Trying to check-in a file \"Comm.h\" that has dependency file \"Message.h\" which is now in closed status " +
                "\n so the check-in will success");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg1);
            demo_checkin3();
        }

        //----< Demonstrates check-in functionality >------

        private void demo_checkin3(){
            Console.Write("Demonstrating check-in files when there is mutual dependency");
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "checkin");
            msg.add("file", "App.xaml.cs");
            msg.add("name", "checkin");
            msg.add("sendPath", "../../../../ClientStorage");
            msg.add("description", "GUI for client");
            msg.add("categories", "Display:");
            msg.add("status", "Open");
            msg.add("path", "Test1");
            Console.Write("\n Trying to check-in a file \"App.xaml.cs\" into repository storage with open status");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);

           CsMessage msg1 = new CsMessage();
            msg1.add("to", CsEndPoint.toString(serverEndPoint));
            msg1.add("from", CsEndPoint.toString(endPoint_));
            msg1.add("command", "checkin");
            msg1.add("file", "ViewFile.xaml.cs");
            msg1.add("name", "checkin");
            msg1.add("sendPath", "../../../../ClientStorage");
            msg1.add("description", "Communication capability");
            msg1.add("categories", "Display:Messaging:");
            msg1.add("status", "Open");
            msg1.add("path", "Test1");
            Console.Write("\n  Trying to check-in a file \"ViewFile.xaml.cs\" into repository storage with open status ");              
            translater.postMessage(msg1);
            msg.remove("status");
            msg.add("children", "ViewFile.xaml.cs:");
            msg.add("status", "PartialClosed");
            Console.Write("\n Now the dependency file ViewFile.xaml.cs is added to App.xaml.cs and trying to check-in");
            translater.postMessage(msg);

            msg1.remove("status");
            msg1.add("status", "Closed");
            msg1.add("children","App.xaml.cs");
            Console.Write("\n Now the dependency file \"App.xaml.cs\" is added to \"ViewFile.xaml.cs\" and trying to check-in "); 
            translater.postMessage(msg1);
            msg.remove("status");
            msg.add("status", "Closed");
            Console.Write("\n   Again trying to check-in App.xaml.cs now, it will success");
            translater.postMessage(msg);
            demo_checkin4();
        }

        //----< Demonstrates check-in functionality >------

        private void demo_checkin4()
        {

            Console.Write("Demonstrating check-in files when there is mutual dependency");
            Console.Write("\n----------------------------------------------------------");
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "checkin");
            msg.add("file", "Version.h");
            msg.add("name", "checkin");
            msg.add("sendPath", "../../../../ClientStorage");
            msg.add("description", "Versioning and deversioning");
            msg.add("categories", "DbCore:");
            msg.add("status", "Open");
            msg.add("path", "Root Directory");
            Console.Write("\n Demonstrating Req#6");
            Console.Write("\n---------------------");
            Console.Write("\n The file \"Version.h\" is sent to repository server in blocks");
            Console.Write("\n Trying to check-in a file \"Version.h\" into repository storage with open status");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);
   
        }

        //----< Requests Repository server for check-out >------

        private void demo_checkout()
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "checkout");
            msg.add("filename", "Version.h");
            msg.add("receivePath", "../../../../ClientStorage");
            Console.Write("\n Trying to checkout file \"Version.h\"");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);

            msg.remove("filename");
            msg.add("filename", "App.xaml.cs");
            Console.Write("\n Trying to checkout file \"App.xaml.cs\" which has dependency, so both the file" +
                "\n and its dependencies are checked out");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);

            
        }

        //----< Demonstrates Browse functionality >------

        private void demo_browse()
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg2 = new CsMessage();
            msg2.add("to", CsEndPoint.toString(serverEndPoint));
            msg2.add("from", CsEndPoint.toString(endPoint_));
            msg2.add("command", "viewmetadata");
            msg2.add("name", "Comm.h");
            Console.Write("\n Demonstrating Req#5");
            Console.Write("\n---------------------");
            Console.Write("\n The message is sent asynchronously and the clinet won't wait for the reply and proceeds with next thing.");
            Console.Write("\n Trying to view meta data for the  file \"Comm.h\"");
            Console.Write("\n\n Requesting the server with Command :" + msg2.value("command"));
            translater.postMessage(msg2);


            fileShown = false;
            CsMessage msg1 = new CsMessage();
            msg1.add("to", CsEndPoint.toString(serverEndPoint));
            msg1.add("from", CsEndPoint.toString(endPoint_));
            msg1.add("command", "viewfile");
            msg1.add("filename", "App.xaml.cs");
            msg1.add("receivePath", "../../../../ClientStorage");
            msg1.add("fileShown", "false");
            Console.Write("\n\n Requesting the server with Command :" + msg1.value("command"));
            translater.postMessage(msg1);

            demo_browse1();
        }

        //----< Demonstrates Browse functionality >------

        private void demo_browse1()
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "queryonfilename");
            msg.add("fname", "Message.cpp");
            Console.Write("\n Requesting server to send filelist that matches with name \"Message.cpp\"");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);


            CsMessage msg1 = new CsMessage();
            msg1.add("to", CsEndPoint.toString(serverEndPoint));
            msg1.add("from", CsEndPoint.toString(endPoint_));
            msg1.add("command", "queryondependency");
            msg1.add("fname", "Message.cpp");
            Console.Write("\n Requesting server to send filelist that has dependency \"Message.cpp\"");
            Console.Write("\n\n Requesting the server with Command :" + msg1.value("command"));
            translater.postMessage(msg1);


            CsMessage msg2 = new CsMessage();
            msg2.add("to", CsEndPoint.toString(serverEndPoint));
            msg2.add("from", CsEndPoint.toString(endPoint_));
            msg2.add("command", "queryonfilename");
            msg2.add("fname", "1");
            Console.Write("\n Requesting server to send filelist that has dependency \"1\"");
            Console.Write("\n\n Requesting the server with Command :" + msg2.value("command"));
            translater.postMessage(msg2);
        }

        //----< Requests server for the content of the specified file  >------

        private void view_Content_of_file_Click(object sender, MouseButtonEventArgs e)
        {
            fileShown = false;
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "viewfile");
            msg.add("filename", fileList.SelectedItem.ToString());
            msg.add("receivePath", "../../../../ClientStorage");
            msg.add("fileShown", "false");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);
        }


        //----< strip off name of first part of path >---------------------

     private string removeFirstDir(string path)
    {
      string modifiedPath = path;
      int pos = path.IndexOf("/");
      modifiedPath = path.Substring(pos + 1, path.Length - pos - 1);
      return modifiedPath;
    }

    //----< respond to mouse double-click on dir name >----------------

    private void DirList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      string selectedDir = (string)DirList.SelectedItem;
      string path;
      if(selectedDir == "..")
      {
        if (pathStack_.Count > 1)  // don't pop off "Storage"
          pathStack_.Pop();
        else
          return;
      }
      else
      {
        path = pathStack_.Peek() + "/" + selectedDir;
        pathStack_.Push(path);
      }
      PathTextBlock.Text = removeFirstDir(pathStack_.Peek());
      
      CsEndPoint serverEndPoint = new CsEndPoint();
      serverEndPoint.machineAddress = "localhost";
      serverEndPoint.port = 8080;
      CsMessage msg = new CsMessage();
      msg.add("to", CsEndPoint.toString(serverEndPoint));
      msg.add("from", CsEndPoint.toString(endPoint_));
      msg.add("command", "getDirs");
      msg.add("path", pathStack_.Peek());
      translater.postMessage(msg);
      
      msg.remove("command");
      msg.add("command", "getFiles");
      translater.postMessage(msg);
    }

        //----< Requests server for files in that directory  >------

        private void dirList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string selectedDir = (string)directoriesList.SelectedItem;
            string path;
            if (selectedDir == "..")
            {
                if (PathStack_.Count > 1)  // don't pop off "Storage"
                    PathStack_.Pop();
                else
                    return;
            }
            else
            {
                path = PathStack_.Peek() + "/" + selectedDir;
                PathStack_.Push(path);
            }
            pathTextBlock.Text = removeFirstDir(PathStack_.Peek());

            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "getDirsforBrowse");
            msg.add("path", PathStack_.Peek());
            translater.postMessage(msg);

            msg.remove("command");
            msg.add("command", "getFilesforBrowse");
            translater.postMessage(msg);
        }

        //----< Checks whether client have given enough input in GUI for check-in >------

        private bool Check_for_null_Checkin()
        {
            String fileWithpath = SelectedFile.Text;
            String description = EnteredDescriptionIs.Text;
            if (fileWithpath == "")
            {
                System.Windows.MessageBox.Show("Select the File");
                return false;
            }

            if (description == "")
            {
                System.Windows.MessageBox.Show("Please write description of the file");
                return false;
            }

            if (Display.IsChecked == false && DbCore.IsChecked == false && Comm.IsChecked == false && Messaging.IsChecked == false)
            {
                System.Windows.MessageBox.Show("Please select atleast one category");
                return false;
            }


            if(opencheckbox.IsChecked == false && closecheckbox.IsChecked == false)
            {
                System.Windows.MessageBox.Show("Please select status ");
                return false;
            }

            if(directoriesBox.SelectedItems.Count <= 0)
            {
                System.Windows.MessageBox.Show("Please select the directory you want to check-in to ");
                return false;
            }

            return true;
        }

        //----< Requests Repository server for check-in >------

        private void Check_in_click(object sender, RoutedEventArgs e)
        {
            String filePath="", fileName = "",categories = "",children = "", status = "";
            String fileWithpath = SelectedFile.Text;
            String description = EnteredDescriptionIs.Text;
            if (!Check_for_null_Checkin())
                return;
            if (fileWithpath != ""){
                int pos = fileWithpath.LastIndexOf("\\");
                 filePath = fileWithpath.Substring(0, pos);
                 fileName = fileWithpath.Substring(pos + 1);}

            if (Display.IsChecked == true  )
                categories = categories + "Display:";
            
            if(DbCore.IsChecked == true)            
                categories = categories + "DbCore:";

            if(Comm.IsChecked == true)
                categories = categories + "Comm:";

            if(Messaging.IsChecked == true)
                categories = categories + "Messaging:";

            for (int i = 0; i < rightChildrenBox.Items.Count; i++)
                children = children + rightChildrenBox.Items[i]+":";

            if (opencheckbox.IsChecked == true)
                status = "Open";
            if (closecheckbox.IsChecked == true)
                status = "Closed";

            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "checkin");
            msg.add("file", fileName);
            msg.add("name", "checkin");
            msg.add("sendPath", filePath);
            msg.add("description", EnteredDescriptionIs.Text);
            msg.add("children",children );
            msg.add("categories", categories);
            msg.add("status", status);
            msg.add("path", directoriesBox.SelectedItem.ToString());
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);
        }

        //----< Requests server to query on given filename to return filelist >------

        private void fetchForFilename(object sender, RoutedEventArgs e)
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "queryonfilename");
            msg.add("fname", EnteredFilenameIs.Text);
           Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
           translater.postMessage(msg);
          }

        //----< Requests server to query on given dependency to return filelist >------

        private void fetchforDependency(object sender, RoutedEventArgs e)
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "queryondependency");
            msg.add("fname", EnteredDependencyFilenameIs.Text);
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);
        }

        //----< Requests server to query on given version to return filelist >------

        private void fetchforVersion(object sender, RoutedEventArgs e)
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "queryonfilename");
            msg.add("fname", EnteredVersionIs.Text);
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);
        }

        //----< Requests server to query on given category to return filelist >------

        private void Display_checkbox_clicked(object sender, RoutedEventArgs e)
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            if (Display.IsChecked == true)
            {
                msg.add("command", "filesofcategory");
                msg.add("category", "Display");

                Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
                translater.postMessage(msg);
            }
            if(Display.IsChecked == false)
            {
                msg.add("command", "removefilesofcategory");
                msg.add("category", "Display");

                Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
                translater.postMessage(msg);
            }
        }

        //----< Requests server to query on given category to return filelist >------

        private void Dbcore_checkbox_clicked(object sender, RoutedEventArgs e)
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            if (DbCore.IsChecked == true)
            {
                msg.add("command", "filesofcategory");
                msg.add("category", "DbCore");

                Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
                translater.postMessage(msg);
            }
            if (DbCore.IsChecked == false)
            {
                msg.add("command", "removefilesofcategory");
                msg.add("category", "DbCore");

                Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
                translater.postMessage(msg);
            }
        }

        //----< Requests server to query on given category to return filelist >------


        private void Comm_checkbox_clicked(object sender, RoutedEventArgs e)
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            if (Comm.IsChecked == true)
            {

                msg.add("command", "filesofcategory");
                msg.add("category", "Comm");

                Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
                translater.postMessage(msg);
            }
            if (Comm.IsChecked == false)
            {
                msg.add("command", "removefilesofcategory");
                msg.add("category", "Comm");

                Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
                translater.postMessage(msg);
            }

        }

        //----< Requests server to query on given category to return filelist >------

        private void Messaging_checkbox_clicked(object sender, RoutedEventArgs e)
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            if (Messaging.IsChecked == true)
            {

                msg.add("command", "filesofcategory");
                msg.add("category", "Messaging");

                Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
                translater.postMessage(msg);
            }
            if (Messaging.IsChecked == false)
            {
                msg.add("command", "removefilesofcategory");
                msg.add("category", "Messaging");

                Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
                translater.postMessage(msg);
            }
        }



        //----< Checks whether client have given enough input in GUI for check-out >------

        private bool check_for_null_for_Checkout()
        {
            if(CheckOutPath.Text == "")
            {
                System.Windows.MessageBox.Show("Select the Path you want to check-out to");
                return false;
            }

            if (FileList.SelectedItems.Count == 0)
            {
                System.Windows.MessageBox.Show("Select the file you want to check-out");
                return false;
            }
            return true;

        }

        //----< Requests Repository server for check-out >------

        private void check_out_click(object sender, RoutedEventArgs e)
        {

            if (!check_for_null_for_Checkout())
                return;

            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "checkout");
            msg.add("filename", FileList.SelectedItem.ToString());
            msg.add("receivePath", CheckOutPath.Text);
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);
        }

        //----< Provides capability to select a file by opening a dialog box >------

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog();
            var result = ofd.ShowDialog();
            if (result == false) return;
            SelectedFile.Foreground = new SolidColorBrush(Colors.Black);
            SelectedFile.Text = ofd.FileName;
        }

        //----< Provides capability to select a directory by opening a dialog box >------

        private void get_Check_out_path(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.ShowDialog();
            CheckOutPath.Text = dialog.SelectedPath;
        }


        //----< Opens a new pop up window to view the content of file >------

        private void viewSelectedFile(string file)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)delegate {
                ViewFile viewFile = new ViewFile();
            String path = System.IO.Path.GetFullPath(file);
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run(File.ReadAllText(path)));
            viewFile.codeview.Blocks.Add(paragraph) ;
            viewFile.Show();
            });
        }

        //----< Requests Repository server for viewing meta data for a file in server >------

        private void view_Meta_Data(object sender, RoutedEventArgs e)
        {

            if (fileList.SelectedItems.Count == 0)
            {
                System.Windows.MessageBox.Show("Select the File to view meta data");
                return;
            }
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg2 = new CsMessage();
            msg2.add("to", CsEndPoint.toString(serverEndPoint));
            msg2.add("from", CsEndPoint.toString(endPoint_));
            msg2.add("command", "viewmetadata");
            msg2.add("name", fileList.SelectedItem.ToString());
            Console.Write("\n\n Requesting the server with Command :" + msg2.value("command"));
            translater.postMessage(msg2);
        }


        //----< Requests Repository server for connecting >------

        private void connect_to_server_click(object sender, RoutedEventArgs e)
        {
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "echo");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command"));
            translater.postMessage(msg);
        }

        //----< Adds dependency files into list of children >------

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (leftChildrenBox.SelectedItems.Count == 0)
            {
                System.Windows.MessageBox.Show("Select children");
                return;
            }

            foreach (var x in leftChildrenBox.SelectedItems)
            {
                Console.Write("\n The selected children is:" + x);
                rightChildrenBox.Items.Insert(0, x);
            }
        }

        //----< Removes dependency files from list of children >------

        private void Remove_Click(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < rightChildrenBox.SelectedItems.Count; i++)
            {
                rightChildrenBox.Items.Remove(rightChildrenBox.SelectedItems[i]);
            }

            rightChildrenBox.Items.Refresh();
        }

        //----< Sets the status checkbox  >------

        private void opencheckbox_clicked(object sender, RoutedEventArgs e)
        {
            if(opencheckbox.IsChecked == true)
            {
                closecheckbox.IsChecked = false;
            }
        }

        //----< Sets the status checkbox  >------

        private void closecheckbox_clicked(object sender, RoutedEventArgs e)
        {
            if (closecheckbox.IsChecked == true)
            {
                opencheckbox.IsChecked = false;
            }
        }

    }
}
