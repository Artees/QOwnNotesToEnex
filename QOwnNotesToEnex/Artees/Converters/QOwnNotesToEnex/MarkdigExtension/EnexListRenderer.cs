using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Artees.Converters.QOwnNotesToEnex.MarkdigExtension
{
    internal class EnexListRenderer : HtmlObjectRenderer<ListBlock>
    {
        protected override void Write(HtmlRenderer renderer, ListBlock listBlock)
        {
            renderer.EnsureLine();
            if (renderer.EnableHtmlForBlock)
            {
                if (listBlock.IsOrdered)
                {
                    renderer.Write("<ol");
                    if (listBlock.BulletType != '1')
                    {
                        renderer.Write(" type=\"").Write(listBlock.BulletType).Write("\"");
                    }

                    if (listBlock.OrderedStart != null && (listBlock.OrderedStart != "1"))
                    {
                        renderer.Write(" start=\"").Write(listBlock.OrderedStart).Write("\"");
                    }
                    renderer.WriteAttributes(listBlock);
                    renderer.WriteLine(">");
                }
                else
                {
                    renderer.Write("<ul");
                    renderer.WriteAttributes(listBlock);
                    renderer.WriteLine(">");
                }
            }

            foreach (var item in listBlock)
            {
                var listItem = (ListItemBlock)item;
                var previousImplicit = renderer.ImplicitParagraph;
                renderer.ImplicitParagraph = !listBlock.IsLoose;

                renderer.EnsureLine();
                if (renderer.EnableHtmlForBlock)
                {
                    renderer.Write("<li").WriteAttributes(listItem).Write("><div>");
                }

                renderer.WriteChildren(listItem);

                if (renderer.EnableHtmlForBlock)
                {
                    renderer.WriteLine("</div></li>");
                }

                renderer.ImplicitParagraph = previousImplicit;
            }

            if (renderer.EnableHtmlForBlock)
            {
                renderer.WriteLine(listBlock.IsOrdered ? "</ol>" : "</ul>");
            }
        }
    }
}