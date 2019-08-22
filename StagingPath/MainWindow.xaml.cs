////////////////////////////////////////////////////////////////////////////////////////
// MainWindow.xaml.cs - GUI for the Client                                            //
// ver 1.0                                                                            //
//                                                                                    //
//Language     : C++                                                                  //
// Platform    : Lenovo 510S Ideapad, Win Pro 10, Visual Studio 2017                  //
// Application : CSE-687 OOD Project 3                                                //
// Author      : Adarsh Venkatesh Bodineni,Syracuse University                        //
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
      Console.Write("\n Demonstrating Requirement #3");
      Console.Write("\n------------------------------");
            Console.Write("\n The GUI have tabs for connecting to server," +
                "\n check-in,check-out,browse specified package descriptions" +
                "\n double clicking on file in browse tab displays text of the file in pop up window" +
                "\n and also the browse tab have functionality for viewing file metadata\n\n");

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

    private void deleteFile(string file)
    {
       leftChildrenBox.Items.Remove(file);
    }


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
          Console.Write("\nCkoooooooooooooooooooooooooooooooooooooooooooooooooooooooooooooo");
          rcvMsg.show();
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



        private void DispatcherFilesOfCategory()
        {
            Action<CsMessage> getFiles = (CsMessage rcvMsg) =>
            {
                Action clrFiles = () =>
                {
                    clearAllFiles();
                };
             //   Dispatcher.Invoke(clrFiles, new Object[] { });
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


        private void DispatcherRemoveFilesOfCategory()
        {
            Action<CsMessage> getFiles = (CsMessage rcvMsg) =>
            {
                Action clrFiles = () =>
                {
                    clearAllFiles();
                };
                 //  Dispatcher.Invoke(clrFiles, new Object[] { });
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
                Console.Write("\nBrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrrr" );
                    rcvMsg.show();
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
                rcvMsg.show();
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
                 msg.show();
                 string metadata = formMetaDataString(msg);
                 Console.Write("\n " + metadata);
                 Dispatcher.Invoke((Action)delegate
                 {
                    // formMetaDataString(msg);
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
    }

    //----< start Comm, and demonstrates all requirements >------

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      Console.Write("\n This is client");
      Console.Write("\n----------------");
      string[] args = System.Environment.GetCommandLineArgs();
      Console.Write("\n The port of this client is " + args[1]);
      endPoint_ = new CsEndPoint();
      endPoint_.machineAddress = "localhost";
      endPoint_.port = int.Parse(args[1]);
      translater = new Translater();
      translater.listen(endPoint_);
      processMessages();
      loadDispatcher();

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
    //  checkin_and_checkout_and_viewmetadata();
    }

        //----< Requests server for check-in, check-out and viewing meta data  >------

        private void checkin_and_checkout_and_viewmetadata()
        {
            //checkout
            CsEndPoint serverEndPoint = new CsEndPoint();
            serverEndPoint.machineAddress = "localhost";
            serverEndPoint.port = 8080;
            CsMessage msg = new CsMessage();
            msg.add("to", CsEndPoint.toString(serverEndPoint));
            msg.add("from", CsEndPoint.toString(endPoint_));
            msg.add("command", "checkout");
            msg.add("filename", "Comm.h");
            msg.add("receivePath", "../../../../ClientStorage");
            Console.Write("\n\n Requesting the server with Command :" + msg.value("command")+ " for the file "+msg.value("filename"));
            translater.postMessage(msg);
        

            //checkin
            CsMessage msg1 = new CsMessage();
            msg1.add("to", CsEndPoint.toString(serverEndPoint));
            msg1.add("from", CsEndPoint.toString(endPoint_));
            msg1.add("command", "checkin");
            msg1.add("file", "IComm.h");
            msg1.add("name", "checkin");
            msg1.add("sendPath", "../../../../ClientStorage");
            Console.Write("\n\n Requesting the server with Command :" + msg1.value("command") + " for the file " + msg1.value("file"));
            translater.postMessage(msg1);

            //viewmetadata
            CsMessage msg2 = new CsMessage();
            msg2.add("to", CsEndPoint.toString(serverEndPoint));
            msg2.add("from", CsEndPoint.toString(endPoint_));
            msg2.add("command", "viewmetadata");
            msg2.add("name", "DbCore.h");
            msg2.add("description", "Implements NoSql database");
            msg2.add("dateTime", DateTime.Now.ToString());
            msg2.add("Children", "PayLoad.h , PayLoad.cpp");
            msg2.add("Categories", "NoSqlDb , DbCOre");
            Console.Write("\n\n Requesting the server with Command :" + msg2.value("command") + " for the file " + msg2.value("name"));
            translater.postMessage(msg2);
            String  file = "../../../../ClientStorage/Comm.h";

            System.Threading.Thread.Sleep(1000);

           viewSelectedFile(file);
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
            msg2.add("description", "Dummy metadata for the file "+ fileList.SelectedItem.ToString());
            msg2.add("dateTime", DateTime.Now.ToString());
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


        private void Remove_Click(object sender, RoutedEventArgs e)
        {

            for (int i = 0; i < rightChildrenBox.SelectedItems.Count; i++)
            {
                rightChildrenBox.Items.Remove(rightChildrenBox.SelectedItems[i]);
            }

            rightChildrenBox.Items.Refresh();
        }

        
        private void opencheckbox_clicked(object sender, RoutedEventArgs e)
        {
            if(opencheckbox.IsChecked == true)
            {
                closecheckbox.IsChecked = false;
            }
        }


        private void closecheckbox_clicked(object sender, RoutedEventArgs e)
        {
            if (closecheckbox.IsChecked == true)
            {
                opencheckbox.IsChecked = false;
            }
        }



    }
}
