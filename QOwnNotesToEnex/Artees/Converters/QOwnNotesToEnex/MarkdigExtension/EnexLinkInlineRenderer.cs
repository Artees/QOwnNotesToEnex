using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using HeyRed.Mime;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax.Inlines;

namespace Artees.Converters.QOwnNotesToEnex.MarkdigExtension
{
    internal class EnexLinkInlineRenderer : HtmlObjectRenderer<LinkInline>
    {
        private readonly Dictionary<string, string> _hashes = new Dictionary<string, string>();

        private readonly string _noteFolder;

        public EnexLinkInlineRenderer(string noteFolder)
        {
            _noteFolder = noteFolder;
        }

        protected override void Write(HtmlRenderer renderer, LinkInline link)
        {
            var isFile = false;
            if (renderer.EnableHtmlForInline)
            {
                var url = link.GetDynamicUrl != null
                    ? link.GetDynamicUrl() ?? link.Url
                    : link.Url;
                isFile = url.Contains("file://");
                if (isFile)
                {
                    var mimeType = MimeTypesMap.GetMimeType(url);
                    renderer.Write("<en-media type=\"" + mimeType + "\" hash=\"");
                    var path = url.Replace("file:/", _noteFolder);
                    var hash = CalculateFileHashTotal(path);
                    _hashes[Path.GetFileName(path)] = hash;
                    renderer.Write(hash);
                    renderer.Write("\"");
                    renderer.WriteAttributes(link);
                }
                else
                {
                    renderer.Write(link.IsImage ? "<img src=\"" : "<a href=\"");
                    renderer.WriteEscapeUrl(url);
                    renderer.Write("\"");
                    renderer.WriteAttributes(link);
                }
            }

            if (link.IsImage)
            {
                if (renderer.EnableHtmlForInline)
                {
                    renderer.Write(" alt=\"");
                }

                var wasEnableHtmlForInline = renderer.EnableHtmlForInline;
                renderer.EnableHtmlForInline = false;
                renderer.WriteChildren(link);
                renderer.EnableHtmlForInline = wasEnableHtmlForInline;
                if (renderer.EnableHtmlForInline)
                {
                    renderer.Write("\"");
                }
            }

            if (renderer.EnableHtmlForInline && !string.IsNullOrEmpty(link.Title))
            {
                renderer.Write(" title=\"");
                renderer.WriteEscape(link.Title);
                renderer.Write("\"");
            }

            if (link.IsImage || isFile)
            {
                if (renderer.EnableHtmlForInline)
                {
                    renderer.Write(" />");
                }
            }
            else
            {
                if (renderer.EnableHtmlForInline)
                {
                    renderer.Write(">");
                }
                renderer.WriteChildren(link);
                if (renderer.EnableHtmlForInline)
                {
                    renderer.Write("</a>");
                }
            }
        }

        private static string CalculateFileHashTotal(string fileLocation)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fileLocation))
                {
                    var b = md5.ComputeHash(stream);
                    stream.Close();
                    return BitConverter.ToString(b).Replace("-", "").ToLower();
                }
            }
        }

        public string FindHash(string fileName)
        {
            return _hashes[Path.GetFileName(fileName)];
        }
    }
}