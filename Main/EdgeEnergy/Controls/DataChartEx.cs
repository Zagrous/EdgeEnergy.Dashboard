//-------------------------------------------------------------------------
// <copyright file="DataChartEx.cs" company="OneSource">
//
// Copyright (c) 2012 One Source Ltd.
// 
// </copyright>
//-------------------------------------------------------------------------


using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Infragistics.Controls.Charts;
using EdgeEnergy.CutterDashboard.ViewModels;

namespace EdgeEnergy.CutterDashboard.Controls
{
	/// <summary>
	/// 
	/// </summary>
    public class DataChartEx : XamDataChart
	{
		#region SelectedChartType (DependencyProperty)
		/// <summary>
		/// Gets or sets the type of the selected chart.
		/// </summary>
		/// <value>The type of the selected chart.</value>
		public int SelectedChartType
		{
			get { return (int)GetValue(SelectedChartTypeProperty); }
			set { SetValue(SelectedChartTypeProperty, value); }
		}

		public static readonly DependencyProperty SelectedChartTypeProperty =
			DependencyProperty.Register(
				"SelectedChartType",
				typeof(int),
				typeof(DataChartEx),
				new PropertyMetadata(OnSelectedChartTypeChanged));

		/// <summary>
		/// Called when [selected chart type changed].
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
		private static void OnSelectedChartTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DataChartEx source = (DataChartEx)d;

			source.OnSeriesSourceChanged(null, source.SeriesSource);
		}
		#endregion SelectedChartType (DependencyProperty)

		#region SeriesSource (DependencyProperty)
		/// <summary>
		/// Gets or sets the series source.
		/// </summary>
		/// <value>The series source.</value>
        public IEnumerable SeriesSource
        {
            get { return (IEnumerable)GetValue(SeriesSourceProperty); }
            set { SetValue(SeriesSourceProperty, value); }
        }

		public static readonly DependencyProperty SeriesSourceProperty = 
			DependencyProperty.Register(
				"SeriesSource", 
				typeof(IEnumerable), 
				typeof(DataChartEx), 
				new PropertyMetadata(OnSeriesSourceChanged));

		/// <summary>
		/// Called when [series source changed].
		/// </summary>
		/// <param name="d">The d.</param>
		/// <param name="e">The <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnSeriesSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            IEnumerable oldValue	= (IEnumerable)e.OldValue;
            IEnumerable newValue	= (IEnumerable)e.NewValue;
            DataChartEx source		= (DataChartEx)d;

			//TODO: Change over to using a Weak Event pattern
			INotifyCollectionChanged ncc;

			if (oldValue != null)
			{
				ncc = oldValue as INotifyCollectionChanged;

				if (ncc != null)
				{
					ncc.CollectionChanged -= source.Series_CollectionChanged;
				}
			}

			if (newValue != null)
			{
				ncc = newValue as INotifyCollectionChanged;

				if (ncc != null)
				{
					ncc.CollectionChanged += source.Series_CollectionChanged;
				}
			}

            source.OnSeriesSourceChanged(oldValue, newValue);
        }

		/// <summary>
		/// Called when [series source changed].
		/// </summary>
		/// <param name="oldValue">The old value.</param>
		/// <param name="newValue">The new value.</param>
        protected virtual void OnSeriesSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
			this.Series.Clear();
			this.Axes.Clear();

            if (newValue != null)
            {
				DataTemplate axisCategoryXTemplate = GetCurentAxisCategoryXTemplate(null);
				DataTemplate axisCategoryYTemplate = GetCurentAxisCategoryYTemplate(null);
                
				// Y Axis
				if (axisCategoryYTemplate != null)
				{
					// load data template content
					Axis axisCategoryY = axisCategoryYTemplate.LoadContent() as Axis;

					if (axisCategoryY != null)
					{
						axisCategoryY.DataContext = DataContext;

						this.Axes.Add(axisCategoryY);
					}
				}

                foreach (object item in newValue)
                {
                    DataTemplate dataTemplate = GetCurentSeriesTemplate(item);

                    if (dataTemplate != null)
                    {
                        // load data template content
                        Series series = dataTemplate.LoadContent() as Series;

                        // load data template for the axis
                        var axis = axisCategoryXTemplate.LoadContent() as Axis;

                        if (series != null)
                        {
                            // set data context
                            series.DataContext = item;
                            axis.DataContext = item;

                            this.Series.Add(series);
                            this.Axes.Add(axis);

                            series.RefreshXAxis<CategoryDateTimeXAxis>(this);
                            series.RefreshYAxis<NumericYAxis>(this);
                        }
                    }
                }

                //Make only one axis labels pane visible.
                if (Axes.Where(axis => axis is CategoryDateTimeXAxis).Count() > 0)
                {
                    Axes.Last().LabelSettings.Visibility = Visibility.Visible;
                }
            }

            // TODO Listen for INotifyCollectionChanged with a weak event pattern
        }

		/// <summary>
		/// Handles the CollectionChanged event of the Series control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
		internal void Series_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			OnSeriesSourceChanged(null, SeriesSource);
		}

		/// <summary>
		/// Gets the curent series template.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		private DataTemplate GetCurentSeriesTemplate(object item)
		{
			DataTemplate dataTemplate = null;

			// get data template
			if (this.SeriesTemplateSelector != null)
			{
				dataTemplate = this.SeriesTemplateSelector.SelectTemplate(item, this);
			}
			if (dataTemplate == null && this.SeriesTemplate != null)
			{
				dataTemplate = this.SeriesTemplate;
			}

			return dataTemplate;
		}

		/// <summary>
		/// Gets the curent axis category X template.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		private DataTemplate GetCurentAxisCategoryXTemplate(object item)
		{
			DataTemplate dataTemplate = null;

			if (AxisCategoryXTemplateSelector != null)
			{
				dataTemplate = AxisCategoryXTemplateSelector.SelectTemplate(DataContext, this);
			}
			if (dataTemplate == null && AxisCategoryXTemplate != null)
			{
				dataTemplate = AxisCategoryXTemplate;
			}

			return dataTemplate;
		}

		/// <summary>
		/// Gets the curent axis category Y template.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <returns></returns>
		private DataTemplate GetCurentAxisCategoryYTemplate(DependencyObject item)
		{
			DataTemplate dataTemplate = null;

			if (AxisCategoryYTemplateSelector != null)
			{
				dataTemplate = AxisCategoryYTemplateSelector.SelectTemplate(DataContext, this);
			}
			if (dataTemplate == null && AxisCategoryYTemplate != null)
			{
				dataTemplate = AxisCategoryYTemplate;
			}

			return dataTemplate;
		}
		#endregion

		#region SeriesTemplate (DependencyProperty)

		/// <summary>
		/// Gets or sets the series template.
		/// </summary>
		/// <value>The series template.</value>
        public DataTemplate SeriesTemplate
        {
            get { return (DataTemplate)GetValue(SeriesTemplateProperty); }
            set { SetValue(SeriesTemplateProperty, value); }
        }

		public static readonly DependencyProperty SeriesTemplateProperty = 
			DependencyProperty.Register(
				"SeriesTemplate", 
				typeof(DataTemplate), 
				typeof(DataChartEx), 
				new PropertyMetadata(default(DataTemplate)));

		#endregion SeriesTemplate (DependencyProperty)

		#region SeriesTemplateSelector (DependencyProperty)

		/// <summary>
		/// Gets or sets the series template selector.
		/// </summary>
		/// <value>The series template selector.</value>
        public DataTemplateSelector SeriesTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(SeriesTemplateSelectorProperty); }
            set { SetValue(SeriesTemplateSelectorProperty, value); }
        }
		public static readonly DependencyProperty SeriesTemplateSelectorProperty = 
			DependencyProperty.Register(
				"SeriesTemplateSelector", 
				typeof(DataTemplateSelector), 
				typeof(DataChartEx), 
				new PropertyMetadata(default(DataTemplateSelector)));

		#endregion SeriesTemplateSelector (DependencyProperty)

		#region AxisCategoryXTemplate (DependencyProperty)

		/// <summary>
		/// Gets or sets the axis template.
		/// </summary>
		/// <value>The series template.</value>
		public DataTemplate AxisCategoryXTemplate
		{
			get { return (DataTemplate)GetValue(AxisCategoryXTemplateProperty); }
			set { SetValue(AxisCategoryXTemplateProperty, value); }
		}

		public static readonly DependencyProperty AxisCategoryXTemplateProperty =
			DependencyProperty.Register(
				"AxisCategoryXTemplate",
				typeof(DataTemplate),
				typeof(DataChartEx),
				new PropertyMetadata(default(DataTemplate)));

		#endregion AxisCategoryXTemplate (DependencyProperty)

		#region AxisCategoryXTemplateSelector (DependencyProperty)

		/// <summary>
		/// Gets or sets the axis template selector.
		/// </summary>
		/// <value>The series template selector.</value>
		public DataTemplateSelector AxisCategoryXTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(AxisCategoryXTemplateSelectorProperty); }
			set { SetValue(AxisCategoryXTemplateSelectorProperty, value); }
		}

		public static readonly DependencyProperty AxisCategoryXTemplateSelectorProperty =
			DependencyProperty.Register(
				"AxisCategoryXTemplateSelector",
				typeof(DataTemplateSelector),
				typeof(DataChartEx),
				new PropertyMetadata(default(DataTemplateSelector)));

		#endregion AxisCategoryXTemplateSelector (DependencyProperty)

		#region AxisCategoryYTemplate (DependencyProperty)

		/// <summary>
		/// Gets or sets the axis template.
		/// </summary>
		/// <value>The series template.</value>
		public DataTemplate AxisCategoryYTemplate
		{
			get { return (DataTemplate)GetValue(AxisCategoryYTemplateProperty); }
			set { SetValue(AxisCategoryYTemplateProperty, value); }
		}

		public static readonly DependencyProperty AxisCategoryYTemplateProperty =
			DependencyProperty.Register(
				"AxisCategoryYTemplate",
				typeof(DataTemplate),
				typeof(DataChartEx),
				new PropertyMetadata(default(DataTemplate)));

		#endregion AxisCategoryXTemplate (DependencyProperty)

		#region AxisCategoryYTemplateSelector (DependencyProperty)

		/// <summary>
		/// Gets or sets the axis template selector.
		/// </summary>
		/// <value>The series template selector.</value>
		public DataTemplateSelector AxisCategoryYTemplateSelector
		{
			get { return (DataTemplateSelector)GetValue(AxisCategoryYTemplateSelectorProperty); }
			set { SetValue(AxisCategoryYTemplateSelectorProperty, value); }
		}

		public static readonly DependencyProperty AxisCategoryYTemplateSelectorProperty =
			DependencyProperty.Register(
				"AxisCategoryYTemplateSelector",
				typeof(DataTemplateSelector),
				typeof(DataChartEx),
				new PropertyMetadata(default(DataTemplateSelector)));

		#endregion AxisCategoryYTemplateSelector (DependencyProperty)
	}

	internal static class DataChartExtensions
	{
		/// <summary>
		/// Refreshes the X axis.
		/// </summary>
		/// <typeparam name="TAxisType">The type of the axis type.</typeparam>
		/// <param name="series">The series.</param>
		/// <param name="source">The source.</param>
		internal static void RefreshXAxis<TAxisType>(this Series series, XamDataChart source) where TAxisType : CategoryAxisBase
		{
            string symbol = ((StockInfoViewModel)series.DataContext).Symbol;

            if (series is HorizontalAnchoredCategorySeries)
            {
                var at = (TAxisType)source.Axes
                    .Where(axes => axes.DataContext is StockInfoViewModel)
                    .Single(axis => ((StockInfoViewModel)axis.DataContext).Symbol == symbol);

                ((HorizontalAnchoredCategorySeries)series).XAxis = at;
            }

            else if (series is FinancialSeries)
            {
                var at = (TAxisType)source.Axes
                    .Where(axes => axes.DataContext is StockInfoViewModel)
                    .Single(axis => ((StockInfoViewModel)axis.DataContext).Symbol == symbol);

                ((FinancialSeries)series).XAxis = at;
            }
		}

		/// <summary>
		/// Refreshes the Y axis.
		/// </summary>
		/// <typeparam name="TAxisType">The type of the axis type.</typeparam>
		/// <param name="series">The series.</param>
		/// <param name="source">The source.</param>
		internal static void RefreshYAxis<TAxisType>(this Series series, XamDataChart source) where TAxisType : NumericYAxis
		{
			BindingExpression	b			= null;
			string				yAxisName	= string.Empty;

            if (series is HorizontalAnchoredCategorySeries)
			{
                b = series.GetBindingExpression(HorizontalAnchoredCategorySeries.YAxisProperty);
				if (b != null) yAxisName = b.ParentBinding.ElementName;

				TAxisType at = (TAxisType)source.Axes.FirstOrDefault((a) => a != null && a.Name == yAxisName);

				if (at == null)
				{
					at = (TAxisType) source.Axes.FirstOrDefault((a) => a is TAxisType);
				}

                ((HorizontalAnchoredCategorySeries)series).YAxis = at;
			}
			else if (series is FinancialSeries)
			{
				b = series.GetBindingExpression(FinancialSeries.YAxisProperty);
				if (b != null) yAxisName = b.ParentBinding.ElementName;

				TAxisType at = (TAxisType)source.Axes.FirstOrDefault((a) => a != null && a.Name == yAxisName);

				if (at == null)
				{
					at = (TAxisType) source.Axes.FirstOrDefault((a) => a is TAxisType);
				}

				((FinancialSeries)series).YAxis = at;
			}
		}
	}
}
