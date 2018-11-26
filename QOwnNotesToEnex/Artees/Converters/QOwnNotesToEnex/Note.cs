using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Artees.Converters.QOwnNotesToEnex.MarkdigExtension;
using HeyRed.Mime;
using Markdig;

namespace Artees.Converters.QOwnNotesToEnex
{
    public class Note
    {
        public static Dictionary<string, Note> Read(string noteFolder)
        {
            var files = Directory.GetFiles(noteFolder, "*.md");
            var mediaFld = $"{noteFolder}/media/";
            var attFld = $"{noteFolder}/attachments/";
            var media = Directory.Exists(mediaFld) ? Directory.GetFiles(mediaFld) : new string[0];
            var attachments = Directory.Exists(attFld) ? Directory.GetFiles(attFld) : new string[0];
            var dictionary = new Dictionary<string, Note>();
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                var note = 
                    new Note(noteFolder, file, name, File.ReadAllText(file), media, attachments);
                dictionary[name] = note;
            }

            return dictionary;
        }

        public readonly string FileName,
            Text;

        private readonly string _noteFolder,
            _path;

        private readonly string[] _media,
            _attachments;

        private Note(string noteFolder, string path, string fileName, string text,
            IEnumerable<string> media, IEnumerable<string> attachments)
        {
            FileName = fileName;
            Text = text;
            _noteFolder = noteFolder;
            _path = path;
            _media = media.Where(s => Text.Contains($"/media/{Path.GetFileName(s)}")).ToArray();
            _attachments = attachments.
                Where(s => Text.Contains($"/attachments/{Path.GetFileName(s)}")).ToArray();
        }

        public XmlNode GetEnexXmlNode(XmlDocument xmlDocument)
        {
            var strings = Text.Split("\n");
            var textStart = 2;
            for (var i = 0; i < strings.Length; i++)
            {
                strings[i] = strings[i].Replace("\n", string.Empty).Replace("\r", string.Empty);
                if (strings[i].StartsWith("=")) textStart = i + 1;
            }

            var note = xmlDocument.CreateElement("note");
            var title = xmlDocument.CreateElement("title");
            title.InnerText = strings[0];
            note.AppendChild(title);
            var noteDoc = new XmlDocument();
            noteDoc.AppendChild(noteDoc.CreateXmlDeclaration("1.0", "UTF-8", "no"));
            noteDoc.AppendChild(noteDoc.CreateDocumentType("en-note", null,
                "http://xml.evernote.com/pub/enml2.dtd", null));
            var enNote = noteDoc.CreateElement("en-note");
            var text = string.Join("\n", strings, textStart, strings.Length - textStart);
            text = ClearDtd(text);
            var paragraphExtension = new EnexParagraphExtension(_noteFolder);
            var markdownPipeline = new MarkdownPipelineBuilder().Use(paragraphExtension).Build();
            var html = Markdown.ToHtml(text, markdownPipeline);
            enNote.InnerXml = html;
            noteDoc.AppendChild(enNote);
            var content = xmlDocument.CreateElement("content");
            var cdata = xmlDocument.CreateCDataSection(noteDoc.OuterXml);
            content.AppendChild(cdata);
            note.AppendChild(content);
            var created = xmlDocument.CreateElement("created");
            created.InnerText = Enex.GetTimeString(File.GetCreationTime(_path));
            note.AppendChild(created);
            var updated = xmlDocument.CreateElement("updated");
            updated.InnerText = Enex.GetTimeString(File.GetLastWriteTime(_path));
            note.AppendChild(updated);
            ParseResources(xmlDocument, paragraphExtension, note);
            return note;
        }

        private static string ClearDtd(string text)
        {
            var iStart = text.IndexOf("<!DOCTYPE", StringComparison.OrdinalIgnoreCase);
            while (iStart >= 0)
            {
                var iEnd = text.IndexOf(">", iStart, StringComparison.OrdinalIgnoreCase);
                text = text.Substring(iEnd);
                iStart = text.IndexOf("<!DOCTYPE", StringComparison.OrdinalIgnoreCase);
            }

            return text;
        }

        private void ParseResources(XmlDocument xmlDocument,
            EnexParagraphExtension paragraphExtension, XmlNode note)
        {
            var resources = _media.Concat(_attachments);
            foreach (var fileName in resources)
            {
                var resource = xmlDocument.CreateElement("resource");
                var data = xmlDocument.CreateElement("data");
                data.SetAttribute("encoding", "base64");
                data.InnerText = Convert.ToBase64String(File.ReadAllBytes(fileName));
                resource.AppendChild(data);
                var mime = xmlDocument.CreateElement("mime");
                var mimeType = MimeTypesMap.GetMimeType(fileName);
                mime.InnerText = mimeType;
                resource.AppendChild(mime);
                var recognition = xmlDocument.CreateElement("recognition");
                var recognitionDoc = new XmlDocument();
                var recognitionDecl = recognitionDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                recognitionDoc.AppendChild(recognitionDecl);
                var recognitionDocType = recognitionDoc.CreateDocumentType("recoIndex", "SYSTEM",
                    "http://xml.evernote.com/pub/recoIndex.dtd", null);
                recognitionDoc.AppendChild(recognitionDocType);
                var recoIndex = recognitionDoc.CreateElement("recoIndex");
                recoIndex.SetAttribute("objType", mimeType);
                var hash = paragraphExtension.FindHash(fileName);
                recoIndex.SetAttribute("objID", hash);
                recognitionDoc.AppendChild(recoIndex);
                var recognitionCdata = xmlDocument.CreateCDataSection(recognitionDoc.OuterXml);
                recognition.AppendChild(recognitionCdata);
                resource.AppendChild(recognition);
                var resourceAttributes = xmlDocument.CreateElement("resource-attributes");
                var resourceFileName = xmlDocument.CreateElement("file-name");
                resourceFileName.InnerText = Path.GetFileName(fileName);
                resourceAttributes.AppendChild(resourceFileName);
                resource.AppendChild(resourceAttributes);
                note.AppendChild(resource);
            }
        }
    }
}