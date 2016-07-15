using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace Bevelop.VSClient.Options
{
    public class GeneralOptions : DialogPage
    {
        [Category("Basic Settings")]
        [DisplayName("Server Url")]
        [Description("Server Url")]
        public string ServerUrl { get; set; } = "http://localhost:8080";
    }
}