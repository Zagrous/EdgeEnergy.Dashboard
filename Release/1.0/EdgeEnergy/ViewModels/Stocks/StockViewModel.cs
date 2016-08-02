//-------------------------------------------------------------------------
// <copyright file="StockViewModel.cs" company="OneSource">
//
// Copyright (c) 2012 One Source Ltd.
// 
// </copyright>
//-------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using EdgeEnergy.Services;
using EdgeEnergy.CutterDashboard.Commands;
using EdgeEnergy.CutterDashboard.Models;
using System.Diagnostics;
using log4net;
using ColorConverter = System.Windows.Media.ColorConverter;

namespace EdgeEnergy.CutterDashboard.ViewModels
{

    public class StockViewModel : INotifyPropertyChanged
	{
		#region Private Member Variables

        private static readonly ILog Log = LogManager.GetLogger("root");

		private readonly IDataService _service;

		private readonly ObservableCollection<StockInfoViewModel> _stocks = new ObservableCollection<StockInfoViewModel>();
		private readonly ObservableCollection<StockInfoViewModel> _stocks1 = new ObservableCollection<StockInfoViewModel>();
		private readonly ObservableCollection<StockInfoViewModel> _stocks2 = new ObservableCollection<StockInfoViewModel>();
		
		private StockInfoViewModel selectedStock ;

        private ICommand    _commandLoadStock;
        private ICommand    _commandAddStock;
        private ICommand    _commandRemoveStock;
        private ICommand    _commandSelectStock;
        private ICommand    _commandStockDisplayRange;
		private ICommand 	_commandMoveToNextStock;
		private ICommand	_commandChartType;

		//private DispatcherTimer		timer;

		private int					_selectedChartType;

        private int                 _stockDisplayRange;

		private string 									_stockSearchFilter;
		private static IList<StockSearchItemViewModel> 	_stockSearchList;
        #endregion Private Member Variables

		#region Constructors

        public StockViewModel(IDataService service)
		{
            IsInitialDataLoading = true;
            _service = service;



            //SynchronizationContext.Current.Post(x => OnLoadStockSearchData(), null);
            OnLoadStockSearchData();


            //$$$
            IsInitialDataLoading = false;
            RaisePropertyChanged("IsInitialDataLoading");


            RefreshDetails();

            //timer = new DispatcherTimer {Interval = new TimeSpan(0, 0, 10)};
            //timer.Tick += (s,e) => RefreshDetails();
            //timer.Start();
		}
		#endregion Constructors

		#region Public Properties
        /// <summary>
        /// Gets or sets the MinimumValue property for the chart axes.
        /// </summary>
        public DateTime MinimumValue { get; private set; }

        /// <summary>
        /// Gets the MaximumValue property for the chart axes.
        /// </summary>
        //public DateTime MaximumValue { get { return DateTime.Now.AddDays(-1); } }
        public DateTime MaximumValue { get; private set; }

		/// <summary>
		/// Gets the tickers.
		/// </summary>
		/// <value>The tickers.</value>
		public ObservableCollection<StockInfoViewModel> Tickers
		{
			get { return _stocks; }
		}

        public ObservableCollection<StockInfoViewModel> Tickers1
        {
            get { return _stocks1; }
        }
        public ObservableCollection<StockInfoViewModel> Tickers2
        {
            get { return _stocks2; }
        }


		/// <summary>
		/// Gets or sets the selected stock.
		/// </summary>
		/// <value>The selected stock.</value>
		public StockInfoViewModel SelectedStock
		{
			get { return selectedStock; }
			set 
			{ 
				if (value == selectedStock) return; 
				
				selectedStock = (value == null && _stocks.Count > 0) ? _stocks.First() : value; 
				
				RaisePropertyChanged("SelectedStock");
				RaisePropertyChanged("IsSelectedStockValid");
			}
		}

		/// <summary>
		/// Gets or sets the type of the selected chart.
		/// </summary>
		/// <value>The type of the selected chart.</value>
		public int SelectedChartType
		{
			get { return _selectedChartType; }
			set
			{
			    if (value == _selectedChartType) return; 
                _selectedChartType = value; 
                RaisePropertyChanged("SelectedChartType"); 
                //RaisePropertyChanged("Tickers");
                RaiseTickerChanged(null);
                RaisePropertyChanged("SelectedStock");
			}
		}

		/// <summary>
		/// Gets the stocks.
		/// </summary>
		/// <value>The stocks.</value>
		public IEnumerable<StockSearchItemViewModel> StockSearchList
		{
			get
			{
				if (string.IsNullOrEmpty(_stockSearchFilter)) return _stockSearchList;

				var filterLower = _stockSearchFilter.ToLower();

				return _stockSearchList.Where(x => { var symbolLower = x.Symbol.ToLower(); return (symbolLower.StartsWith(filterLower) || symbolLower.StartsWith(filterLower)); });
			}
		}

		/// <summary>
		/// Gets or sets the stock search filter.
		/// </summary>
		/// <value>The stock search filter.</value>
		public string StockSearchFilter
		{
			get { return _stockSearchFilter; }
			set { _stockSearchFilter = value; RaisePropertyChanged("StockSearchFilter"); RaisePropertyChanged("StockSearchList"); }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is retrieving data.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is retrieving data; otherwise, <c>false</c>.
		/// </value>
		public bool IsInitialDataLoading { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this instance is selected stock value.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is selected stock value; otherwise, <c>false</c>.
		/// </value>
		public bool IsSelectedStockValid { get { return selectedStock != null && selectedStock.Data != null; } }

		/// <summary>
		/// Gets a value indicating whether the selected stock instances details have been populated.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is details populated; otherwise, <c>false</c>.
		/// </value>
		public bool IsDetailsPopulated { get { return SelectedStock != null && SelectedStock.Details != null && !string.IsNullOrEmpty(SelectedStock.Details.Symbol); } }


		#endregion Public Properties

		#region Public Commands

        /// <summary>
        /// Gets the add stock command.
        /// </summary>
        /// <value>The add stock command.</value>
        public ICommand LoadStockCommand
        {
            get
            {
                if (_commandLoadStock == null)
                {
                    _commandLoadStock = new RelayCommand<string>(
                      p => { LoadList(); },
                      p => { return true; });

                    RaisePropertyChanged("LoadStockCommand");
                }
                return (_commandLoadStock);
            }
        }



		/// <summary>
		/// Gets the add stock command.
		/// </summary>
		/// <value>The add stock command.</value>
		public ICommand AddStockCommand
		{
			get
			{
				if (_commandAddStock == null)
				{
					_commandAddStock = new RelayCommand<string>(
					  p => { AddSymbolToWatchList(p); },
					  p => { return true; });

					RaisePropertyChanged("AddStockCommand");
				}
				return (_commandAddStock);
			}
		}

		/// <summary>
		/// Gets the remove stock command.
		/// </summary>
		/// <value>The remove stock command.</value>
		public ICommand RemoveStockCommand
		{
			get
			{
				if (_commandRemoveStock == null)
				{
					_commandRemoveStock = new RelayCommand<string>(
					  p => { RemoveSymbolFromWatchList(p); },
					  p => { return true; }
					  );

					RaisePropertyChanged("RemoveStockCommand");
				}
				return (_commandRemoveStock);
			}
		}

		/// <summary>
		/// Gets the select stock command.
		/// </summary>
		/// <value>The select stock command.</value>
		public ICommand SelectStockCommand
		{
			get
			{
				if (_commandSelectStock == null)
				{
					_commandSelectStock = new RelayCommand<string>(
						p => { SetSelectedStock(p); },
						p => { return true; }
						);

					RaisePropertyChanged("SelectStockCommand");
				}

				return (_commandSelectStock);
			}
		}

		/// <summary>
		/// Gets the move to next stock.
		/// </summary>
		/// <value>The move to next stock.</value>
		public ICommand MoveToNextStock
		{
			get
			{
				if (_commandMoveToNextStock == null)
				{
					_commandMoveToNextStock = new RelayCommand<int>(
						p => { MoveToStock(p); },
						p => { return true; }
						);
				}

				return (_commandMoveToNextStock);
			}
		}

        /// <summary>
        /// Gets the stock display range command
        /// </summary>
        public ICommand StockDisplayRangeCommand
        {
            get
            {
                if (_commandStockDisplayRange == null)
                {
                    _commandStockDisplayRange = new RelayCommand<int>(
                        p => { SetStockDisplayRange(p); },
                        p => { return true; }
                    );

                    RaisePropertyChanged("StockDisplayRangeCommand");
                }

                return (_commandStockDisplayRange);
            }
        }

		/// <summary>
		/// Gets the selected chart type command.
		/// </summary>
		/// <value>The selected chart type command.</value>
		public ICommand SelectedChartTypeCommand
		{
			get
			{
				if (_commandChartType == null)
				{
					_commandChartType = new RelayCommand<int>(
						p => { SetChartType(p); },
						p => { return true; }
					);

					RaisePropertyChanged("SelectedChartTypeCommand");
				}

				return (_commandChartType);
			}
		}
		#endregion Public Commands
      
        #region Public Methods

        /// <summary>
        /// Goes through the StockSearchList and checks
        /// if the entered Stock symbol is actually an
        /// existing stock.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns></returns>
        public bool? CheckForExistingStockBySymbolName(string symbol)
        {
            if (_stockSearchList == null)
            {
                //This means that the search list is not yet initialized
                //but the two default stock "MSFT" and "ALCO" have to be added.
                return null;
            }

            foreach (var searchItem in _stockSearchList)
            {
                if (searchItem.Symbol == symbol)
                {
                    //Found!
                    return true;
                }
            }

            //The Stock doesn't exist.
            return false;
        }

        public void LoadList()
        {
            var defaultItems = _service.GetDefaultLegends().ToList();

            foreach (var item in defaultItems)
            {
                AddSymbolToWatchList(item);
            }

            // Select the First 
            SelectStockCommand.Execute(defaultItems.First());

        }

        public void ResetErrors()
        {
            _service.Error = 0;
        }

        public int GetErrors()
        {
            return _service.Error;
        }

        public void RaiseTickerChanged(string symbol)
        {
            RaisePropertyChanged("Tickers");
        }

	    /// <summary>
		/// Adds the tick to watch list.
		/// </summary>
		/// <param name="symbol">The symbol.</param>
		public void AddSymbolToWatchList(string symbol)
		{
			if (string.IsNullOrEmpty(symbol)) return;


            //Get the stock info
			var sivm = GetStockInfoBySymbolName(symbol);

			if (sivm != null) return;

	        var brush = GetBrush(symbol);

	        var stock = new StockInfoViewModel(this, symbol, brush, _stocks.Count > 0 ? _stocks[0].RangeFilter : 1);

            //Add the stock to the other stocks.
            _stocks.Add(stock);
            _stocks1.Add(stock);


            //if (service.GetViewIndex(symbol) == 1)
            //    stocks1.Add(stock);
            //else
            //    stocks2.Add(stock);

	        //Get the data for the stock.
	        RefreshData(stock);

	        //RaisePropertyChanged("Tickers");
            RaiseTickerChanged(symbol);

			// If this is the first stock, lets automatically select it.
			if (_stocks.Count == 1 || SelectedStock == null)
			{
				SelectedStock = _stocks.First();
			}
		}

	    private Brush GetBrush(string symbol)
	    {
	        var colour = _service.GetColour(symbol);
	        var convertFromString = ColorConverter.ConvertFromString(colour);
            Brush brush = convertFromString == null ? new SolidColorBrush(Color.FromRgb(255, 255, 255)) : new SolidColorBrush((Color)convertFromString);

	        return brush;
	    }

	    /// <summary>
		/// Removes the ticker from watch list.
		/// </summary>
		/// <param name="symbol">The symbol.</param>
		public void RemoveSymbolFromWatchList(string symbol)
		{
            _service.Unsubscribe(symbol);

			var sivm = GetStockInfoBySymbolName(symbol);

			if (sivm == null) return;

			_stocks.Remove(sivm);
            _stocks1.Remove(sivm);


            //if (service.GetViewIndex(symbol) == 1)
            //    stocks1.Remove(sivm);
            //else
            //    stocks2.Remove(sivm);

            // If we just removed the active stock, we should reset it.
            if (_stocks.Count > 0 && (sivm == SelectedStock || SelectedStock == null))
            {
                SelectedStock = _stocks.First();
            }


            SetStockDisplayRange(_stockDisplayRange);

			//RaisePropertyChanged("Tickers");
            RaiseTickerChanged(symbol);
		}

		/// <summary>
		/// Moves to stock based on index.
		/// </summary>
		/// <param name="index">The index.</param>
		public void MoveToStock(int index)
		{
			int iCurrentIndex = _stocks.IndexOf(SelectedStock);

			iCurrentIndex += index;

			if (iCurrentIndex < 0 || iCurrentIndex >= _stocks.Count || _stocks.Count == 0) return;

			SelectedStock = _stocks[iCurrentIndex];
		}

		/// <summary>
		/// Sets the selected stock.
		/// </summary>
		/// <param name="symbol">The symbol.</param>
		public void SetSelectedStock(string symbol)
		{
			var sivm = GetStockInfoBySymbolName(symbol);

			if (sivm == null) return;

			SelectedStock = sivm;
		}

        /// <summary>
        /// Set the Range Filter for the currently selected stock
        /// </summary>
        /// <param name="numberOfMonths"></param>
        //public void SetStockDisplayRange(int numberOfMonths)
        //{
        //    stockDisplayRange = numberOfMonths;

        //    stocks.Each(x => x.RangeFilter = numberOfMonths);

        //    DateTime minimumValue = DateTime.Now;

        //    if (numberOfMonths == 0)
        //    {
        //        foreach (StockInfoViewModel stock in stocks)
        //        {
        //            if (stock.Data != null)
        //            {
        //                if (stock.Data.Count() > 0)
        //                {
        //                    DateTime date = stock.Data.First().Date;
        //                    if (date < minimumValue)
        //                    {
        //                        minimumValue = date;
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    else
        //    {
        //        minimumValue = DateTime.Now.AddMonths(-1 * numberOfMonths);
        //    }

        //    MinimumValue = minimumValue;

        //    RaisePropertyChanged("MinimumValue");
        //}

        public void SetStockDisplayRange(int numberOfMonths)
        {
            _stockDisplayRange = numberOfMonths;

            _stocks.Each(x => x.RangeFilter = numberOfMonths);

            DateTime minimumValue = DateTime.MaxValue;
            DateTime maximumValue = DateTime.MinValue;


            foreach (StockInfoViewModel stock in _stocks)
            {
                if (stock.Data == null) continue;

                if (!stock.Data.Any()) continue;

                DateTime minDate = stock.Data.First().Date;
                if (minDate < minimumValue)
                    minimumValue = minDate;

                DateTime maxDate = stock.Data.Last().Date;
                if (maxDate > maximumValue)
                    maximumValue = maxDate;                

            }

            MinimumValue = minimumValue;
            MaximumValue = maximumValue;

            //Log.InfoFormat("MinimumValue {0} {1}", MinimumValue, MaximumValue);

            RaisePropertyChanged("MinimumValue");
            RaisePropertyChanged("MaximumValue");
        }



		/// <summary>
		/// Sets the type of the chart.
		/// </summary>
		/// <param name="chartType">Type of the chart.</param>
		public void SetChartType(int chartType)
		{
			SelectedChartType = chartType;
		}

        /// <summary>
        /// Refreshes the details.
        /// </summary>
        public void RefreshDetails()
        {
            // @@@ service.RefreshDetails(stocks.Select(x => x.Symbol).ToList(), OnRefreshDetails);

            var result = new Dictionary<string, StockTickerDetails>();

            var symbols = _stocks.Select(x => x.Symbol).ToList();

            foreach (var symbol in symbols)
            {
                result.Add(symbol,
                    new StockTickerDetails
                    {
                        Symbol = symbol
                    }
                    );
            }

            OnRefreshDetails(result);
        }

		/// <summary>
		/// Refreshes the data.
		/// </summary>
		public void RefreshData(StockInfoViewModel stock)
		{
            //service.RefreshData(stock.Symbol, DateTime.Now.AddYears(-100), OnRefreshData);
            _service.Subscribe(stock.Symbol, OnSnapshotData, OnUpdateData);
		}
		#endregion Public Methods

		#region Private Methods
        /// <summary>
        /// Gets the name of the stock info by symbol.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <returns></returns>
		StockInfoViewModel GetStockInfoBySymbolName(string symbol)
		{
			var sivm = (from item in _stocks
						where item.Symbol == symbol
						select item).DefaultIfEmpty(null).FirstOrDefault();

			return sivm;
		}

		/// <summary>
		/// Called when [refresh details].
		/// </summary>
		/// <param name="data">The data.</param>
		private void OnRefreshDetails(IDictionary<string, StockTickerDetails> data)
		{
			foreach (var sivm in _stocks)
			{
				// It is possible a new stock was added while a query was in progress
				// If so, we can ignore it as it will be picked up on the next query
				if (data.ContainsKey(sivm.Symbol))
				{
					sivm.Details = data[sivm.Symbol];
				}
			}

			RaisePropertyChanged("SelectedStock");
            //RaisePropertyChanged("Tickers");
            RaiseTickerChanged(null);
			RaisePropertyChanged("IsDetailsPopulated");			
		}

        /// <summary>
        /// Called when [refresh data].
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="data">The data.</param>
        private void OnSnapshotData(string symbol, IEnumerable<CutterData> data)
        {
            IsInitialDataLoading = false;
            RaisePropertyChanged("IsInitialDataLoading");

            var sivm = GetStockInfoBySymbolName(symbol);

            if (sivm == null)
                return;


            sivm.AddRange(data);

            //foreach (var cutterData in data)
            //{
            //    sivm.Data.Add(cutterData);   
            //}


            //sivm.Data = data.ToList();

            SetStockDisplayRange(_stockDisplayRange);

            RaisePropertyChanged("SelectedStock");

            RaiseTickerChanged(symbol);
        }

        private void OnUpdateData(string symbol, CutterData data)
        {
            //Log.InfoFormat("OnUpdateData1: {0} - {1}", symbol, data );

            var sivm = GetStockInfoBySymbolName(symbol);

            if (sivm == null)
                return;

            sivm.AddData(data);

            SetStockDisplayRange(_stockDisplayRange);

            RaisePropertyChanged("SelectedStock");

            RaiseTickerChanged(symbol);

            //Log.InfoFormat("OnUpdateData2: {0} - {1}", symbol, data);

            // TODO To be implemented
        }


        ///// <summary>
        ///// Called when [refresh data].
        ///// </summary>
        ///// <param name="symbol">The symbol.</param>
        ///// <param name="data">The data.</param>
        //private void OnRefreshData(string symbol, IEnumerable<CutterData> data)
        //{
        //    IsInitialDataLoading = false;
        //    RaisePropertyChanged("IsInitialDataLoading");

        //    var sivm = GetStockInfoBySymbolName(symbol);

        //    if (sivm == null)
        //    {
        //        return;
        //    }

        //    sivm.Data = data;

        //    SetStockDisplayRange(stockDisplayRange);

        //    RaisePropertyChanged("SelectedStock");
            
        //    RaiseTickerChanged(symbol);
        //}


        public IEnumerable<StockSearchItemViewModel> GetSearchItems()
        {
            yield return new StockSearchItemViewModel
            {
                Symbol = "Add data field...",
                Parent = null
            };

            foreach (var symbol in _service.GetAvailableSymbols())
            {
                yield return new StockSearchItemViewModel
                {
                    Symbol = symbol,
                    Parent = this
                };
            }
        }


		/// <summary>
		/// Called when [load stock search data].
		/// </summary>
		private void OnLoadStockSearchData()
		{
			try
			{
                _stockSearchList = GetSearchItems().ToList();

				RaisePropertyChanged("StockSearchList");
			}
			catch 
			{
				// We are hiding this exception because it is most likely caused
				// but blend not being able to find the stocksymbols.xml file
				// This only happens until the project is built at least once inside 
				// blend
			}
		}
		#endregion Private Methods

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;
		/// <summary>
		/// Raises the property changed.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		internal void RaisePropertyChanged(string propertyName)
		{
			if (PropertyChanged == null) return;
            Debug.WriteLine(string.Format("PropertyChanged:{0}", propertyName));
			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
