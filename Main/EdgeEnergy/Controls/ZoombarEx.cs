//-------------------------------------------------------------------------
// <copyright file="ZoombarEx.cs" company="OneSource">
//
// Copyright (c) 2012 One Source Ltd.
// 
// </copyright>
//-------------------------------------------------------------------------


using System;
using System.Windows;

using Infragistics.Controls;
using log4net;

namespace EdgeEnergy.CutterDashboard.Controls
{
	public class ZoombarEx : XamZoombar
	{
        private static readonly ILog Log = LogManager.GetLogger("root");


		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="ZoombarEx"/> class.
		/// </summary>
		public ZoombarEx()
        {
            ZoomChanging += SelfZoomChanging;
            ZoomChanged += SelfZoomChanged;
        }
		#endregion Constructors

		#region Target (Dependency Property)
		public readonly static DependencyProperty TargetProperty = 
            DependencyProperty.Register("Target", typeof(XamZoombar), typeof(ZoombarEx),
				new PropertyMetadata(null, 
					(o, e) => {
						(o as ZoombarEx).ChangeTargetZoombar(
							e.OldValue as XamZoombar,
							e.NewValue as XamZoombar);
					}));

		/// <summary>
		/// Gets or sets the target.
		/// </summary>
		/// <value>The target.</value>
        public XamZoombar Target
        {
            get { return (XamZoombar)GetValue(TargetProperty);}
            set { SetValue(TargetProperty, value); }
        }

		/// <summary>
		/// Changes the target zoombar.
		/// </summary>
		/// <param name="oldValue">The old value.</param>
		/// <param name="newValue">The new value.</param>
        private void ChangeTargetZoombar(XamZoombar oldValue, XamZoombar newValue)
        {
            if (oldValue != null)
            {
                oldValue.ZoomChanging -= TargetZoomChanging;
                oldValue.ZoomChanged -= TargetZoomChanged;
            }

            if (newValue != null)
            {
                newValue.ZoomChanging += TargetZoomChanging;
                newValue.ZoomChanged += TargetZoomChanged;
            }
        }
		#endregion Target (Dependency Property)

		#region Private Properties
		/// <summary>
		/// Gets or sets a value indicating whether [dont recurse].
		/// </summary>
		/// <value><c>true</c> if [dont recurse]; otherwise, <c>false</c>.</value>
		private bool DontRecurse { get; set; }
		#endregion Private Properties

		#region Private Event Handlers
		/// <summary>
		/// Selfs the zoom changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="ZoomChangedEventArgs"/> instance containing the event data.</param>
		void SelfZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            UpdateRange(Target, this);					
        }

		/// <summary>
		/// Selfs the zoom changing.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="ZoomChangeEventArgs"/> instance containing the event data.</param>
        void SelfZoomChanging(object sender, ZoomChangeEventArgs e)
        {
            UpdateRange(Target, this);
        }

		/// <summary>
		/// Targets the zoom changed.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="ZoomChangedEventArgs"/> instance containing the event data.</param>
        void TargetZoomChanged(object sender, ZoomChangedEventArgs e)
        {
            UpdateRange(this, Target);
		}

		/// <summary>
		/// Targets the zoom changing.
		/// </summary>
		/// <param name="sender">The sender.</param>
		/// <param name="e">The <see cref="ZoomChangeEventArgs"/> instance containing the event data.</param>
        void TargetZoomChanging(object sender, ZoomChangeEventArgs e)
        {
            UpdateRange(this, Target);
        }
		#endregion Private Event Handlers

		#region Private Methods
        private void UpdateRange(XamZoombar target, XamZoombar source)
		{
			if (Target == null || DontRecurse)
			{
				return;
			}

			DontRecurse = true;
			try
			{
                target.Range.Minimum = source.Range.Minimum;
                target.Range.Maximum = source.Range.Maximum;

				//Log.InfoFormat("Changed: {0} {1}, {2} {3}" , target.Name, source.Name,   Range.Minimum , Range.Maximum);
			}
			catch (Exception ex)
			{
				Log.InfoFormat(ex.ToString());
			}
			DontRecurse = false;
		}
		#endregion Private Methods
	}
}
