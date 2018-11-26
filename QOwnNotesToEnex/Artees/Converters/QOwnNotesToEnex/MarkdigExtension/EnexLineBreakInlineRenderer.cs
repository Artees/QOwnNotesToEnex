using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;

namespace Artees.Converters.QOwnNotesToEnex.MarkdigExtension
{
    internal class EnexLineBreakInlineRenderer : HtmlObjectRenderer<LineBreakInline>
    {
        protected override void Write(HtmlRenderer renderer, LineBreakInline obj)
        {
            if (renderer.EnableHtmlForInline)
            {
                if (obj.IsHard)
                {
                    renderer.WriteLine("</div><div>");
                }
                else
                {
                    renderer.WriteLine();
                }
            }
            else
            {
                renderer.Write(" ");
            }
        }
    }
}