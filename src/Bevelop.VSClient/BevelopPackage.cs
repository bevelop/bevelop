using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace Bevelop.VSClient
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideOptionPage(typeof(OptionPageGrid), "My Category", "My Grid Page", 0, 0, true)]
    [Guid(BevelopPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class BevelopPackage : Package
    {
        public const string PackageGuidString = "f49f9eaa-ce04-41cd-b46a-f847b2a85f33";

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
        [Category("My Category")]
        [DisplayName("Server Url")]
        [Description("Server Url")]
        public string ServerUrl { get; set; } = "http://localhost:8080";
    }
}
