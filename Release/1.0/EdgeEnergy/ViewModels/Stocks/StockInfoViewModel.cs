//-------------------------------------------------------------------------
// <copyright file="StockInfoViewModel.cs" company="OneSource">
//
// Copyright (c) 2012 One Source Ltd.
// 
// </copyright>
//-------------------------------------------------------------------------


using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using EdgeEnergy.CutterDashboard.Models;
using EdgeEnergy.Services;
using log4net;


namespace EdgeEnergy.CutterDashboard.ViewModels
{
	/// <summary>
	/// 
	/// </summary>
	public class StockInfoViewModel : INotifyPropertyChanged
	{
        private static readonly ILog Log = LogManager.GetLogger("root");

		#region Private Member Variables
		private static Dictionary<int, int> 	_rangeIntervalXLookup;
		private static Dictionary<int, int> 	_rangeIntervalYLookup;

		private StockTickerDetails				_details = new StockTickerDetails();
        //private ObservableCollection<CutterData> _data = new ObservableCollection<CutterData>();
        private ObservableCollection<CutterData> _data;
		private StockViewModel					_parent;

        private int 	_rangeFilter	= 0;
		private int 	_rangeIntervalX	= 4;
		private int 	_rangeIntervalY	= 1;
		private Brush	_brush			= new SolidColorBrush(Colors.Yellow);

		#endregion Private Member Variables

		#region Constructor
		/// <summary>
		/// Initializes the <see cref="StockInfoViewModel"/> class.
		/// </summary>
		static StockInfoViewModel()
		{
			_rangeIntervalXLookup = new Dictionary<int, int>();

			_rangeIntervalXLookup[120] 	= 60;	// 10 Years
			_rangeIntervalXLookup[60] 	= 60;	// 5 Years
			_rangeIntervalXLookup[48] 	= 60;	// 4 Years
			_rangeIntervalXLookup[36] 	= 60;	// 3 Years
			_rangeIntervalXLookup[24] 	= 60;	// 2 Years
			_rangeIntervalXLookup[12] 	= 60;	// 1 Year
			_rangeIntervalXLookup[6] 	= 30;	// 6 Months
			_rangeIntervalXLookup[3] 	= 21;	// 3 Months
			_rangeIntervalXLookup[2] 	= 14;	// 2 Months
			_rangeIntervalXLookup[1] 	= 7;	// 1 Month
			_rangeIntervalXLookup[0]	= 120; 	// unlimited

			_rangeIntervalYLookup = new Dictionary<int, int>();

			_rangeIntervalYLookup[12] 	= 60;	// 50 $ 
			_rangeIntervalYLookup[6] 	= 30;	// 25 $ 
			_rangeIntervalYLookup[3] 	= 21;	// 10 $ 
			_rangeIntervalYLookup[2] 	= 14;	// 5 $ 
			_rangeIntervalYLookup[1] 	= 7;	// 1 $
			_rangeIntervalYLookup[0]	= 120; 	// unlimited
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StockInfoViewModel"/> class.
		/// </summary>
		public StockInfoViewModel()
		{
			
		}

	    /// <summary>
	    /// Initializes a new instance of the <see cref="StockInfoViewModel"/> class.
	    /// </summary>
	    /// <param name="parent">The parent.</param>
	    /// <param name="symbol">The symbol.</param>
	    /// <param name="brush">The brush.</param>
	    /// <param name="range">The range.</param>
	    public StockInfoViewModel(StockViewModel parent, string symbol, Brush brush, int range)
        {
            Brush = brush;

            _parent = parent;
            _details.Symbol = symbol;

            RangeFilter = range == 0 ? 1 : range;
        }
		#endregion Constructor

		#region Public Properties
		/// <summary>
		/// Gets the symbol.
		/// </summary>
		/// <value>The symbol.</value>
		public string Symbol
		{
			get { return Details != null ? Details.Symbol : string.Empty; }
			set { Details.Symbol = value; }
		}

		/// <summary>
		/// Gets or sets the details.
		/// </summary>
		/// <value>The details.</value>
		public StockTickerDetails Details
		{
			get { return _details; }
			set { if (value == _details) return; _details = value; RaisePropertyChanged("Details"); RaisePropertyChanged("Symbol"); }
		}

		/// <summary>
		/// Gets or sets the data.
		/// </summary>
		/// <value>The data.</value>
        public ObservableCollection<CutterData> Data
		{
			get { return _data; }
			set
			{
			    if (value == _data) return; 
                _data = value;

			    RaisePropertyChanged("Data");
			    RaisePropertyChanged("FilteredData");
			    //Parent.RaisePropertyChanged("Tickers");
			    Parent.RaiseTickerChanged(null);
			    Parent.RaisePropertyChanged("SelectedStock");

			}
		}

        public void AddRange( IEnumerable<CutterData> data)
        {
            if (_data == null)
                _data = new ObservableCollection<CutterData>(data);

            RaisePropertyChanged("Data");
            RaisePropertyChanged("FilteredData");

            Parent.RaiseTickerChanged(null);
            Parent.RaisePropertyChanged("SelectedStock");
        }


        public void AddData(CutterData data)
        {
            if( _data == null )
                _data = new ObservableCollection<CutterData>();

            _data.Add(data);
            RaisePropertyChanged("Data");
            RaisePropertyChanged("FilteredData");

            Parent.RaiseTickerChanged(null);
            Parent.RaisePropertyChanged("SelectedStock");
        }

        /// <summary>
        /// Returns Stock Data based on the current Range Filter (if range filter = 0 than all data is returned)
        /// </summary>
        public ObservableCollection<CutterData> FilteredData
        {
            get
            {
                //if (_data == null) return _data;
                //if (_rangeFilter == 0) return _data;

                //Log.InfoFormat("FilteredData");
                return _data;


                //DateTime dt = DateTime.Now.AddMonths(-1 * RangeFilter);

                //return _data.Where(x => x.Date > dt);
            }
        }

        /// <summary>
        /// Sets a filter for the data (based in number of months)
        /// </summary>
        public int RangeFilter
        {
            get { return _rangeFilter; }
            set 
			{
				if (value == _rangeFilter) return;

				// This indicates a number in months
				_rangeFilter 	= value;

				// What is the X Axises interval going to look like?
				bool bFoundInterval = false;
				int i = _rangeFilter;

				// Set the default value (the unlimited value)
				_rangeIntervalX = _rangeIntervalXLookup[0];
				_rangeIntervalY = _rangeIntervalYLookup[0];

				// Find the one we are looking for or the next closest
				while (!bFoundInterval && i > 0)
				{
					if (_rangeIntervalXLookup.ContainsKey(i))
					{
						_rangeIntervalX = _rangeIntervalXLookup[i];
						bFoundInterval = true;
					} else i--;
				}

				i = _rangeFilter;
				while (!bFoundInterval && i > 0)
				{
					if (_rangeIntervalYLookup.ContainsKey(i))
					{
						_rangeIntervalY = _rangeIntervalYLookup[i];
						bFoundInterval = true;
					}
					else i--;
				}
			
				RaisePropertyChanged("RangeFilter"); 
				RaisePropertyChanged("FilteredData");
				RaisePropertyChanged("RangeIntervalX");
				RaisePropertyChanged("RangeIntervalY");
			}

        }

		/// <summary>
		/// Gets or sets the range interval.
		/// </summary>
		/// <value>The range interval.</value>
		public int RangeIntervalX
		{
			get { return _rangeIntervalX; }
			set { if (value == _rangeIntervalX) return; _rangeIntervalX = value; RaisePropertyChanged("RangeIntervalX"); RaisePropertyChanged("FilteredData"); }
		}

		/// <summary>
		/// Gets or sets the range interval.
		/// </summary>
		/// <value>The range interval.</value>
		public int RangeIntervalY
		{
			get { return _rangeIntervalY; }
			set { if (value == _rangeIntervalY) return; _rangeIntervalY = value; RaisePropertyChanged("RangeIntervalY"); RaisePropertyChanged("FilteredData"); }
		}

		/// <summary>
		/// Gets the parent.
		/// </summary>
		/// <value>The parent.</value>
		public StockViewModel Parent
		{
			get { return _parent; }
		}

        /// <summary>
        /// Gets or sets the brush for this stock.
        /// </summary>
        /// <value>The brush.</value>
        public Brush Brush
        {
            get;
            private set;
        }
		#endregion Public Properties

		#region Public Commands
		#endregion Public Commands

		#region Public Methods



		#endregion Public Methods

		#region INotifyPropertyChanged Members
		public event PropertyChangedEventHandler PropertyChanged;
		/// <summary>
		/// Raises the property changed.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		private void RaisePropertyChanged(string propertyName)
		{
			if (PropertyChanged == null) return;

			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}
}
