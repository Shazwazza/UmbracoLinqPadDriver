using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using LINQPad.Extensibility.DataContext;
using Microsoft.Win32;

namespace UmbracoLinqPad
{
	/// <summary>
	/// Interaction logic for ConnectionDialog.xaml
	/// </summary>
	public partial class ConnectionDialog : Window
	{
	    readonly IConnectionInfo _cxInfo;
	    
	    public ConnectionDialog (IConnectionInfo cxInfo)
		{
			_cxInfo = cxInfo;
	        DataContext = cxInfo.CustomTypeInfo;
			InitializeComponent ();
		}

		void btnOK_Click (object sender, RoutedEventArgs e)
		{   
			DialogResult = true;
		}

		void BrowseUmbracoFolder (object sender, RoutedEventArgs e)
		{
            var dialog = new OpenFileDialog
		    {
		        FileName = "Filename will be ignored",
		        CheckPathExists = true,
		        ShowReadOnly = false, 
                ReadOnlyChecked = true, 
                CheckFileExists = false, 
                ValidateNames = false
		    };

            if (dialog.ShowDialog() == true)
            {
                _cxInfo.AppConfigPath = System.IO.Path.GetDirectoryName(dialog.FileName);
            }

		}

	
	}
}
