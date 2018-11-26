using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Artees.Converters.QOwnNotesToEnex.MarkdigExtension
{
    internal class EnexParagraphRenderer : HtmlObjectRenderer<ParagraphBlock>
    {
        protected override void Write(HtmlRenderer renderer, ParagraphBlock obj)
        {
            if (!renderer.ImplicitParagraph && renderer.EnableHtmlForBlock)
            {
                if (!renderer.IsFirstInContainer)
                {
                    renderer.EnsureLine();
                }

                renderer.Write("<div").WriteAttributes(obj).Write(">");
            }

            renderer.WriteLeafInline(obj);
            if (!renderer.ImplicitParagraph && renderer.EnableHtmlForBlock)
            {
                renderer.WriteLine("</div>");
                if (!renderer.IsLastInContainer)
                {
                    renderer.WriteLine("<div><br/></div>");
                }
                else
                {
                    renderer.EnsureLine();
                }
            }
        }
    }
}