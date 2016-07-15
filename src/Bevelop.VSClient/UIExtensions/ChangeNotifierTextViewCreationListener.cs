using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Bevelop.VSClient.UIExtensions
{
    /// <summary>
    /// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
    /// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    sealed class ChangeNotifierTextViewCreationListener : IWpfTextViewCreationListener
    {
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("ChangeNotifier")]
        [Order(After = PredefinedAdornmentLayers.Caret)]
        AdornmentLayerDefinition _editorAdornmentLayer;

        readonly ITextDocumentFactoryService _textDocumentFactoryService;
        IVsCommandWindow _commandWindow;

        [ImportingConstructor]
        public ChangeNotifierTextViewCreationListener(ITextDocumentFactoryService textDocumentFactoryService)
        {
            _textDocumentFactoryService = textDocumentFactoryService;
            _commandWindow = (IVsCommandWindow)ServiceProvider.GlobalProvider.GetService(typeof(SVsCommandWindow));
        }

        public AdornmentLayerDefinition EditorAdornmentLayer
        {
            get
            {
                return _editorAdornmentLayer;
            }

            set
            {
                _editorAdornmentLayer = value;
            }
        }

        public void TextViewCreated(IWpfTextView textView)
        {
            ITextDocument document;
            if (_textDocumentFactoryService.TryGetTextDocument(textView.TextBuffer, out document))
            {
                // The adorment will get wired to the text view events
                new ChangeNotifier(textView, document, _commandWindow);
            }
        }
    }
}
