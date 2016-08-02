//-------------------------------------------------------------------------
// <copyright file="SeriesTemplateSelector.cs" company="OneSource">
//
// Copyright (c) 2012 One Source Ltd.
// 
// </copyright>
//-------------------------------------------------------------------------


using System.Windows;
using System.Windows.Controls;
using EdgeEnergy.CutterDashboard.ViewModels;

namespace EdgeEnergy.CutterDashboard.Controls
{
    public class SeriesTemplateSelector : DataTemplateSelector
	{
		#region Public Properties
		public DataTemplate LineChartTemplate { get; set; }
		public DataTemplate OHLCChartTemplate { get; set; }
		public DataTemplate AreaChartTemplate { get; set; }
		public DataTemplate CandelStickTemplate { get; set; }
		#endregion Public Properties

		#region Public Methods
		/// <summary>
		/// Selects the template.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="container">The container.</param>
		/// <returns></returns>
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
			StockInfoViewModel vm = item as StockInfoViewModel;

			if (vm == null) return null;

			switch (vm.Parent.SelectedChartType)
			{
				case 0: return LineChartTemplate;
				case 1: return OHLCChartTemplate;
				case 2: return AreaChartTemplate;
				case 3: return CandelStickTemplate;
			}

			return null;
        }
		#endregion Public Methods
	}
}
