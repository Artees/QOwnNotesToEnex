using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Xml;
using Artees.Converters.QOwnNotesToEnex;
using Artees.Converters.QOwnNotesToEnex.SQLiteExtension;
using Xunit;

namespace QOwnNotesToEnexTest
{
    public class UnitTest
    {
        private const string TestNoteFolder = "../../../../TestNoteFolder/";

        [Fact]
        public void TestMarkdownTableBuilder()
        {
            var dataTable = GetTestDataTable("SELECT * FROM tag");
            var markdown = dataTable.ToMarkdown();
            Assert.Equal(File.ReadAllText("../../../TestMarkdown.md"), markdown);
        }

        private static DataTable GetTestDataTable(string commandText)
        {
            DataTable dataTable;
            using (var sqlite = GetTestSqLite())
            {
                dataTable = sqlite.GetDataTable(commandText);
            }

            return dataTable;
        }

        private static SQLiteConnection GetTestSqLite()
        {
            var connectionString = $"Data Source={Environment.CurrentDirectory}/" +
                                   $"{TestNoteFolder}notes.sqlite;Version=3;";
            return new SQLiteConnection(connectionString);
        }

        [Fact]
        public void TestTableName()
        {
            var dataTable = GetTestDataTable("SELECT name FROM sqlite_master WHERE type='table'");
            var strings = new DataTableStrings(dataTable);
            Assert.Contains("tag", strings.Items.Cast<string>());
        }

        [Fact]
        public void TestTags()
        {
            Tag tags;
            using (var sqlite = GetTestSqLite())
            {
                tags = new Tag(sqlite, ReadTestNotes());
            }

            Assert.Equal("all", tags.ToString());
            var root = tags.Children.Values.Select(tag => tag.ToString()).ToList();
            root.Sort();
            Assert.Equal(root, new[] {"tag0", "tag1"});
            var children0 = 
                tags.Children["tag0"].Children.Values.Select(tag => tag.ToString()).ToList();
            children0.Sort();
            Assert.Equal(children0, new[] {"tag00", "tag01", "tag02", "tag03"});
            var children01 = tags.Children["tag0"].Children["tag01"].Children.Values
                .Select(tag => tag.ToString()).ToList();
            children01.Sort();
            Assert.Equal(children01, new[] {"tag010"});
        }

        private static Dictionary<string, Note> ReadTestNotes()
        {
            return Note.Read(TestNoteFolder);
        }

        [Fact]
        public void TestNotes()
        {
            var notes = ReadTestNotes();
            const string name = "Test Note 0";
            var note = notes[name];
            Assert.Equal(name, note.FileName);
            Assert.Equal(File.ReadAllText($"{TestNoteFolder}Test Note 0.md"), note.Text);
        }

        [Fact]
        public void TestNoteTagLink()
        {
            Tag tags;
            using (var sqlite = GetTestSqLite())
            {
                tags = new Tag(sqlite, ReadTestNotes());
            }

            Assert.Equal(new[] {"Test Note 0"},
                tags.Children["tag0"].Children["tag02"].Notes.Select(n => n.FileName).ToArray());
        }

        [Fact]
        public void TestEnex()
        {
            var expected = new XmlDocument();
            expected.Load("../../../TestEnex.enex");
            var actual = Enex.Create(TestNoteFolder);
            var expectedDate = 
                expected.GetElementsByTagName("en-export")[0].Attributes["export-date"].Value;
            actual.GetElementsByTagName("en-export")[0].Attributes["export-date"].Value = 
                expectedDate;
            Assert.Equal(File.ReadAllText("../../../TestEnex.enex"), actual.OuterXml);
        }
    }
}