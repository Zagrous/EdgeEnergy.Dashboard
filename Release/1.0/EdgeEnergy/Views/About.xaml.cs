//-------------------------------------------------------------------------
// <copyright file="About.xaml.cs" company="OneSource">
//
// Copyright (c) 2012 One Source Ltd.
// 
// </copyright>
//-------------------------------------------------------------------------


using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace EdgeEnergy.CutterDashboard.Views
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class About : Window
	{
		public About()
		{
			InitializeComponent();
		}

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

	}


}
