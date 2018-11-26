using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Renderers.Html.Inlines;

namespace Artees.Converters.QOwnNotesToEnex.MarkdigExtension
{
    internal class EnexParagraphExtension : IMarkdownExtension
    {
        private readonly string _noteFolder;
        private EnexLinkInlineRenderer _linkInlineRenderer;

        public EnexParagraphExtension(string noteFolder)
        {
            _noteFolder = noteFolder;
        }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            renderer.ObjectRenderers.RemoveAll(x => x is ParagraphRenderer);
            renderer.ObjectRenderers.RemoveAll(x => x is LineBreakInlineRenderer);
            renderer.ObjectRenderers.RemoveAll(x => x is LinkInlineRenderer);
            renderer.ObjectRenderers.RemoveAll(x => x is ListRenderer);
            renderer.ObjectRenderers.Add(new EnexParagraphRenderer());
            renderer.ObjectRenderers.Add(new EnexLineBreakInlineRenderer());
            _linkInlineRenderer = new EnexLinkInlineRenderer(_noteFolder);
            renderer.ObjectRenderers.Add(_linkInlineRenderer);
            renderer.ObjectRenderers.Add(new EnexListRenderer());
        }

        public string FindHash(string fileName)
        {
            return _linkInlineRenderer.FindHash(fileName);
        }
    }
}