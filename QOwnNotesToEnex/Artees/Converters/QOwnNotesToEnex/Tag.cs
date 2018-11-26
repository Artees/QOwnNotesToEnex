using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Xml;
using Artees.Converters.QOwnNotesToEnex.SQLiteExtension;

namespace Artees.Converters.QOwnNotesToEnex
{
    public class Tag
    {
        public readonly Dictionary<string, Tag> Children;
        public readonly Note[] Notes;

        private readonly string _name;

        public Tag(SQLiteConnection sqlite, Dictionary<string, Note> notes) :
            this("all", "0", sqlite, notes)
        {
        }

        private Tag(string name, string id, SQLiteConnection sqlite, Dictionary<string, Note> notes)
        {
            _name = name;
            Notes = LinkNotes(id, sqlite, notes);
            Children = CreateChildren(id, sqlite, notes);
        }

        private static Note[] LinkNotes(string id, SQLiteConnection sqlite,
            Dictionary<string, Note> notes)
        {
            var dataTable = sqlite.GetDataTable("SELECT * FROM noteTagLink WHERE tag_id = " + id);
            var names = new List<string>();
            foreach (DataRow row in dataTable.Rows)
            {
                var noteName = row.ItemArray[dataTable.Columns.IndexOf("note_file_name")];
                names.Add(noteName.ToString());
            }

            return notes.Where(kv => names.Contains(kv.Key)).Select(kv => kv.Value).ToArray();
        }

        private static Dictionary<string, Tag> CreateChildren(string id, SQLiteConnection sqlite,
            Dictionary<string, Note> notes)
        {
            var dataTable = sqlite.GetDataTable("SELECT * FROM tag WHERE parent_id = " + id);
            var rows = dataTable.Rows;
            var rowsCount = rows.Count;
            var nameColumn = dataTable.Columns.IndexOf("name");
            var idColumn = dataTable.Columns.IndexOf("id");
            var children = new Dictionary<string, Tag>();
            for (var i = 0; i < rowsCount; i++)
            {
                var items = rows[i].ItemArray;
                var tagName = items[nameColumn].ToString();
                children[tagName] = new Tag(tagName, items[idColumn].ToString(), sqlite, notes);
            }

            return children;
        }

        public override string ToString()
        {
            return _name;
        }

        public IEnumerable<XmlNode> GetEnexXmlNodes(XmlDocument xml, Note note)
        {
            foreach (var child in Children)
            {
                foreach (var node in child.Value.GetEnexXmlNodes(xml, note)) yield return node;
                if (!child.Value.Notes.Contains(note)) continue;
                var tag = xml.CreateElement("tag");
                tag.InnerText = child.Key;
                yield return tag;
            }
        }
    }
}