//-------------------------------------------------------------------------
// <copyright file="LocalizedResources.cs" company="OneSource">
//
// Copyright (c) 2012 One Source Ltd.
// 
// </copyright>
//-------------------------------------------------------------------------

namespace EdgeEnergy.CutterDashboard.Assets.LocalizedResources
{
	public class LocalizedResources
	{
		#region Private Variables
		private static Strings strings = new Strings();
		private static Images images = new Images();
		#endregion Private Variables

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="LocalizedResources"/> class.
		/// </summary>
		public LocalizedResources()
		{
		}
		#endregion Constructors

		#region Public Properties
		/// <summary>
		/// Gets the strings.
		/// </summary>
		/// <value>The strings.</value>
		public Strings Strings
		{
			get { return strings; }
		}

		/// <summary>
		/// Gets the images.
		/// </summary>
		/// <value>The images.</value>
		public Images Images
		{
			get { return images; }
		}
		#endregion Public Properties
	}
}
