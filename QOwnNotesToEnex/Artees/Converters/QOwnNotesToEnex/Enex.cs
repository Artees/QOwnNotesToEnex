using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Globalization;
using System.Linq;
using System.Xml;

namespace Artees.Converters.QOwnNotesToEnex
{
    public static class Enex
    {
        public static XmlDocument Create(string noteFolder)
        {
            var xml = new XmlDocument();
            var declaration = xml.CreateXmlDeclaration("1.0", "UTF-8", null);
            xml.AppendChild(declaration);
            var documentType = xml.CreateDocumentType("en-export", null,
                "http://xml.evernote.com/pub/evernote-export2.dtd", null);
            xml.InsertBefore(documentType, xml.DocumentElement);
            var enExport = xml.CreateElement("en-export");
            var date = GetTimeString(DateTime.Now);
            enExport.SetAttribute("export-date", date);
            enExport.SetAttribute("application", "QOwnNotesToEnex");
            enExport.SetAttribute("version", "1.0");
            var notes = Note.Read(noteFolder);
            AddTags(noteFolder, notes, xml, enExport);
            xml.AppendChild(enExport);
            return xml;
        }

        private static void AddTags(string noteFolder, Dictionary<string, Note> notes,
            XmlDocument xml, XmlNode enExport)
        {
            Tag tags;
            using (var sqlite =
                new SQLiteConnection($"Data Source={noteFolder}/notes.sqlite;Version=3;"))
            {
                tags = new Tag(sqlite, notes);
            }

            var noteArray = notes.Values.ToArray();
            Console.Write("Converting notes... ");
            using (var progressBar = new ProgressBar())
            {
                for (var i = 0; i < noteArray.Length; i++)
                {
                    progressBar.Report((double) i / noteArray.Length);
                    var note = noteArray[i];
                    AddNote(xml, enExport, tags, note);
                }
            }

            Console.WriteLine("Done.");
        }

        private static void AddNote(XmlDocument xml, XmlNode enExport, Tag tags, Note note)
        {
            var noteEnex = note.GetEnexXmlNode(xml);
            foreach (var tag in tags.GetEnexXmlNodes(xml, note))
            {
                noteEnex.AppendChild(tag);
            }

            enExport.AppendChild(noteEnex);
        }

        public static string GetTimeString(DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMddTHHmmssZ", CultureInfo.InvariantCulture);
        }
    }
}