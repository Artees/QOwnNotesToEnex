using System.Data;
using System.Data.SQLite;

namespace Artees.Converters.QOwnNotesToEnex.SQLiteExtension
{
    public static class SqLiteExtensions
    {
        public static DataTable GetDataTable(this SQLiteConnection sqlite, string commandText)
        {
            sqlite.Open();
            var sqlCommand = sqlite.CreateCommand();
            sqlCommand.CommandText = commandText;
            var adapter = new SQLiteDataAdapter(sqlCommand);
            var dataTable = new DataTable();
            adapter.Fill(dataTable);
            sqlite.Close();
            return dataTable;
        }
        
        public static string ToMarkdown(this DataTable dataTable)
        {
            var strings = new DataTableStrings(dataTable);
            return new MarkdownTableBuilder(strings.Columns, strings.Items).ToString();
        }
    }
}