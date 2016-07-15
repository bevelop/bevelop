﻿//------------------------------------------------------------------------------
// <copyright file="TestPackage.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Bevelop.VSClient
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideOptionPage(typeof(OptionPageGrid), "My Category", "My Grid Page", 0, 0, true)]
    [Guid(TestPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class TestPackage : Package
    {
        /// <summary>
        /// TestPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "f49f9eaa-ce04-41cd-b46a-f847b2a85f33";

        /// <summary>
        /// Initializes a new instance of the <see cref="TestPackage"/> class.
        /// </summary>
        public TestPackage()
        {
            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        #endregion
    }

    public class OptionPageGrid : DialogPage
    {
        string url = "http://localhost";

        public OptionPageGrid()
        {
            
        }

        [Category("My Category")]
        [DisplayName("Server Url")]
        [Description("Server Url")]
        public string ServerUrl
        {
            get { return url; }
            set { url = value; }
        }
    }
}
