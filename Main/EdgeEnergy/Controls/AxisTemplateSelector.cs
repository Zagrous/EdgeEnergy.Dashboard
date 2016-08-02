//-------------------------------------------------------------------------
// <copyright file="AxisTemplateSelector.cs" company="OneSource">
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
	public class AxisTemplateSelector : DataTemplateSelector
	{
		#region Public Properties
		/// <summary>
		/// Gets or sets the single series template.
		/// </summary>
		/// <value>The single series template.</value>
		public DataTemplate SingleSeriesTemplate { get; set; }
		/// <summary>
		/// Gets or sets the multiple series template.
		/// </summary>
		/// <value>The multiple series template.</value>
		public DataTemplate MultipleSeriesTemplate { get; set; }
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
			StockViewModel vm = item as StockViewModel;

			if (vm == null) return null;

			return (vm.Tickers != null && 
						vm.Tickers.Count > 1 && 
						MultipleSeriesTemplate != null) ? MultipleSeriesTemplate : SingleSeriesTemplate;
		}
		#endregion Public Methods
	}
}
