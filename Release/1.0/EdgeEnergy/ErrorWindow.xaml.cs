//-------------------------------------------------------------------------
// <copyright file="ErrorWindow.xaml.cs" company="OneSource">
//
// Copyright (c) 2012 One Source Ltd.
// 
// </copyright>
//-------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace EdgeEnergy.CutterDashboard
{
	/// <summary>
	///     Controls when a stack trace should be displayed on the
	///     Error Window
	///     
	///     Defaults to <see cref="OnlyWhenDebuggingOrRunningLocally"/>
	/// </summary>
	public enum StackTracePolicy
	{
		/// <summary>
		///   Stack trace is showed only when running with a debugger attached
		///   or when running the app on the local machine. Use this to get
		///   additional debug information you don't want the end users to see
		/// </summary>
		OnlyWhenDebuggingOrRunningLocally,

		/// <summary>
		/// Always show the stack trace, even if debugging
		/// </summary>
		Always,

		/// <summary>
		/// Never show the stack trace, even when debugging
		/// </summary>
		Never
	}

	/// <summary>
	/// Interaction logic for ErrorWindow.xaml
	/// </summary>
	public partial class ErrorWindow : Window
	{
		private static ErrorWindow _current;

		internal ErrorWindow(string message, string errorDetails)
		{
			InitializeComponent();

			IntroductoryText.Text = message;
			ErrorTextBox.Text = errorDetails;

			//errorDetailsRegion.Visibility = string.IsNullOrEmpty(errorDetails) ? Visibility.Collapsed : Visibility.Visible;
		}

		#region Factory Shortcut Methods
		/// <summary>
		///     Creates a new Error Window given an error message.
		///     Current stack trace will be displayed if app is running under
		///     debug or on the local machine
		/// </summary>
		public static void CreateNew(string message)
		{
			CreateNew(message, StackTracePolicy.OnlyWhenDebuggingOrRunningLocally);
		}

		/// <summary>
		///     Creates a new Error Window given an exception.
		///     Current stack trace will be displayed if app is running under
		///     debug or on the local machine
		///     
		///     The exception is converted onto a message using
		///     <see cref="ConvertExceptionToMessage"/>
		/// </summary>
		public static void CreateNew(Exception exception)
		{
			CreateNew(exception, StackTracePolicy.OnlyWhenDebuggingOrRunningLocally);
		}

		/// <summary>
		///     Creates a new Error Window given an exception. The exception is converted onto a message using
		///     <see cref="ConvertExceptionToMessage"/>
		///     
		///     <param name="policy">When to display the stack trace, see <see cref="StackTracePolicy"/></param>
		/// </summary>
		public static void CreateNew(Exception exception, StackTracePolicy policy)
		{
			// Account for nested exceptions
			var message = exception.Message;
			var innerException = exception.InnerException;
			var sbFullStackTrace = new StringBuilder(exception.StackTrace);

			while (innerException != null)
			{
				message = innerException.Message;
				sbFullStackTrace.AppendFormat("\nCaused by: {0}\n\n{1}", exception.Message, exception.StackTrace);
				innerException = innerException.InnerException;
			}

			CreateNew(message, sbFullStackTrace.ToString(), policy);
		}

		/// <summary>
		///     Creates a new Error Window given an error message.
		///     
		///     <param name="policy">When to display the stack trace, see <see cref="StackTracePolicy"/></param>
		/// </summary>
		public static void CreateNew(string message, StackTracePolicy policy)
		{
			CreateNew(message, new StackTrace().ToString(), policy);
		}
		#endregion

		#region Factory Methods
		/// <summary>
		///     All other factory methods will result in a call to this one
		/// </summary>
		/// 
		/// <param name="message">Which message to display</param>
		/// <param name="stackTrace">The associated stack trace</param>
		/// <param name="policy">In which situations the stack trace should be appended to the message</param>
		private static void CreateNew(string message, string stackTrace, StackTracePolicy policy)
		{
			if (_current != null) return;

			string errorDetails = string.Empty;

			if (policy == StackTracePolicy.Always ||
				policy == StackTracePolicy.OnlyWhenDebuggingOrRunningLocally && IsRunningUnderDebugOrLocalhost)
			{
				errorDetails = stackTrace ?? string.Empty;
			}

			_current = new ErrorWindow(message, errorDetails);
			_current.ShowDialog();
		}
		#endregion

		#region Factory Helpers
		/// <summary>
		///     Returns whether running under a dev environment, i.e., with a debugger attached or
		///     with the server hosted on localhost
		/// </summary>
		private static bool IsRunningUnderDebugOrLocalhost
		{
			get { return Debugger.IsAttached; }
		}
		#endregion

		private void OKButton_Click(object sender, RoutedEventArgs e)
		{
			this.DialogResult = true;

            Environment.Exit(0);
		}
	}
}
