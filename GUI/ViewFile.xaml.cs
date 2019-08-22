////////////////////////////////////////////////////////////////////////////////////////
// ViewFile.xaml.cs - Pops up to show the content of file                             //
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
 * Pops up to show the contents of the file
 * Required Files:
 * ---------------
 * ViewFile.xaml, ViewFile.xaml.cs
 * 
 * public Interfaces:
 * ------------------
 *
 * Maintenance History:
 * --------------------
 * ver 1.0 : 9 Apr 2018
 * - first release
 *
 * */
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
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for ViewFile.xaml
    /// </summary>
    public partial class ViewFile : Window
    {
        public ViewFile()
        {
            InitializeComponent();
        }
    }
}
