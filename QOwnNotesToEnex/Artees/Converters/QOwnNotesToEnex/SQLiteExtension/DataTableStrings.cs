using System.Data;

namespace Artees.Converters.QOwnNotesToEnex.SQLiteExtension
{
    public class DataTableStrings
    {
        public readonly string[] Columns;
        public readonly string[,] Items;

        public DataTableStrings(DataTable dataTable)
        {
            var rows = dataTable.Rows;
            var columns = dataTable.Columns;
            var rowsCount = rows.Count;
            var columnsCount = columns.Count;
            Columns = new string[columnsCount];
            Items = new string[rowsCount, columnsCount];
            for (var iColumn = 0; iColumn < columnsCount; iColumn++)
            {
                var columnName = columns[iColumn].ToString();
                Columns[iColumn] = columnName;
                for (var iRow = 0; iRow < rowsCount; iRow++)
                {
                    var row = rows[iRow];
                    var item = row.ItemArray[iColumn];
                    var itemString = item.ToString();
                    Items[iRow, iColumn] = itemString;
                }
            }
        }
    }
}