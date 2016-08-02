//-------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="OneSource">
//
// Copyright (c) 2012 One Source Ltd.
// 
// </copyright>
//-------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EdgeEnergy.CutterDashboard.ViewModels;
using Infragistics.Controls;
using log4net;

namespace EdgeEnergy.CutterDashboard.Views
{

    //public class MyCommands
    //{
    //    public RoutedCommand DeleteItem = new RoutedCommand("DeleteItem", typeof(StockViewModel));

    //}


	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class StocksView : Page
	{
        public static RoutedCommand DeleteItem = new RoutedCommand("DeleteItem", typeof(StockViewModel));
        
		#region Private Variables
        private static readonly ILog Log = LogManager.GetLogger("root");

		private StockViewModel _vm;
		#endregion Private Variables

		#region Constructors
		/// <summary>
        /// Initializes a new instance of the <see cref="StocksView"/> class.
		/// </summary>
        public StocksView()
		{
			InitializeComponent();

		    CommandBindings.Add(new CommandBinding(DeleteItem, OnDeleteItem));

            ZoomOut.Click += OnZoomOutChecked;

		}

        private void OnZoomOutChecked(object sender, RoutedEventArgs e)
        {
            zoomBar.Range = new Range();

            stockChart.VerticalZoombar.Range = new Range();

            mainChart.VerticalZoombar.Range = new Range();  
        }

		#endregion Constructors

		#region Event Handlers

        private void ResetWarning()
        {
            _vm.ResetErrors();    
        }

        private void ShowWarning()
        {
            int errors = _vm.GetErrors();
            if (errors == 0) return;

            MessageBox.Show(string.Format("There were {0} error(s) while processing data", errors), "Warning",
                            MessageBoxButton.OK);
        }

		/// <summary>
		/// Handles the Loaded event of the Window control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
		private void WindowLoaded(object sender, RoutedEventArgs e)
		{
            SymbolSelector.SelectedIndex = 0;
		
			// Lets grab the data context
			_vm = DataContext as StockViewModel;

			if (_vm == null)
			{
				throw new NullReferenceException("DataContext must be of type CutterViewModel");
			}

		    ResetWarning();
            _vm.LoadList();
            ShowWarning();
		}

		/// <summary>
		/// Handles the DropDownClosed event of the txtbxSymbolNew control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="bool"/> instance containing the event data.</param>
        //private void TxtbxSymbolNewDropDownClosed(object sender, RoutedPropertyChangedEventArgs<bool> e)
        //{
        //    var symbol = ((AutoCompleteBox)sender).Text.ToUpper();

        //    if ((bool)vm.CheckForExistingStockBySymbolName(symbol))
        //    {
        //        ((AutoCompleteBox)sender).Text = "";

        //        if (vm.AddStockCommand.CanExecute(symbol))
        //        {
        //            vm.AddStockCommand.Execute(symbol);
        //        }
        //    }
        //}


        public void OnDeleteItem(Object sender, ExecutedRoutedEventArgs e)
        {
            //Log.InfoFormat("Sender = {0} {1}", sender, e.Parameter);

            var range = new Range
            {
                Maximum = zoomBar.Range.Maximum,
                Minimum = zoomBar.Range.Minimum
            };

            var symbol = e.Parameter;
            if (_vm.RemoveStockCommand.CanExecute(symbol))
            {
                _vm.RemoveStockCommand.Execute(symbol);

            }

            zoomBar.Range = range;
        }


	    private void OnAddItem(object sender, SelectionChangedEventArgs e)
	    {
	        var comboBox = sender as ComboBox;
            if (comboBox == null) return;

	        var viewModel = (StockSearchItemViewModel) ((sender as ComboBox).SelectedItem);
            if (viewModel == null) return;

            if (viewModel.Parent == null) return;

            var symbol = viewModel.Symbol;

            //Log.InfoFormat("mainChart Min={0} Max={1}",
            //mainChart.HorizontalZoombar.Range.Minimum,
            //mainChart.HorizontalZoombar.Range.Maximum);

            //Log.InfoFormat("ZoomBar1 Min={0} Max={1}",
            //zoomBar.Range.Minimum,
            //zoomBar.Range.Maximum);

            var range = new Range
                {
                    Maximum = zoomBar.Range.Maximum, 
                    Minimum = zoomBar.Range.Minimum
                };

            if (_vm.AddStockCommand.CanExecute(symbol))
            {
                ResetWarning();
                _vm.AddStockCommand.Execute(symbol);
                ShowWarning();

                SymbolSelector.SelectedIndex = 0;
            }

            zoomBar.Range = range;

            //Log.InfoFormat("ZoomBar2 Min={0} Max={1}",
            //zoomBar.Range.Minimum,
            //zoomBar.Range.Maximum);

        }


       //private void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    var win = new About { Owner = Application.Current.MainWindow };
        //    win.ShowDialog();
        //}
		#endregion Event Handlers
	}
}
