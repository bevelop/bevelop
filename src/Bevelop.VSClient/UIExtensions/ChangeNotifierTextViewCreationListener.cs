using System.ComponentModel.Composition;
using Bevelop.VSClient.Services;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Bevelop.VSClient.UIExtensions
{
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
        readonly IVsCommandWindow _commandWindow;
        readonly IZipper _zipper;
        readonly IGitService _gitService;
        readonly IChangeServer _changeServer;

        [ImportingConstructor]
        public ChangeNotifierTextViewCreationListener(ITextDocumentFactoryService textDocumentFactoryService)
        {
            _textDocumentFactoryService = textDocumentFactoryService;
            _commandWindow = (IVsCommandWindow)ServiceProvider.GlobalProvider.GetService(typeof(SVsCommandWindow));

            _zipper = new Zipper();
            _gitService = new GitService(_zipper);
            _changeServer = new ChangeServer();
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
                new ChangeNotifier(textView, document, _commandWindow, _gitService, _changeServer, _zipper);
            }
        }
    }
}
