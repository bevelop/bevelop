using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Bevelop.VSClient.Options;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Bevelop.VSClient
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideOptionPage(typeof(GeneralOptions), "Bevelop", "General", 0, 0, true)]
    [Guid(BevelopPackage.PackageGuidString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    public sealed class BevelopPackage : Package
    {
        public const string PackageGuidString = "f49f9eaa-ce04-41cd-b46a-f847b2a85f33";

        #region Package Members

        internal static BevelopPackage Instance { get; private set; }

        internal GeneralOptions GeneralOptions => (GeneralOptions) GetDialogPage(typeof (GeneralOptions));

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();

            Instance = this;
        }

        #endregion
    }
}
