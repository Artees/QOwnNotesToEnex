using System;
using System.Linq;
using System.Text;

namespace Artees.Converters.QOwnNotesToEnex.SQLiteExtension
{
    internal class MarkdownTableBuilder
    {
        private readonly string[] _columnNames;
        private readonly string[,] _itemNames;

        private readonly int _rowLength,
            _columnsCount,
            _rowsCount;

        private readonly StringBuilder _stringBuilder = new StringBuilder();

        private int _iRow;

        public MarkdownTableBuilder(string[] columnNames, string[,] itemNames)
        {
            _columnNames = columnNames;
            _itemNames = itemNames;
            _columnsCount = _itemNames.GetLength(1);
            _rowsCount = _itemNames.GetLength(0);
            var cl = columnNames.Max(name => name.Length);
            _rowLength = (from string n in itemNames select n.Length).Concat(new[] {cl}).Max();
        }

        public override string ToString()
        {
            RowToString(GetColumnName, " ");
            RowToString(GetEmptyString, "-");
            for (var iRow = 0; iRow < _rowsCount; iRow++)
            {
                _iRow = iRow;
                RowToString(GetItemName, " ");
            }

            return _stringBuilder.ToString();
        }

        private void RowToString(Func<int, string> getItemString, string space)
        {
            for (var iColumn = 0; iColumn < _columnsCount; iColumn++)
            {
                var itemString = getItemString(iColumn);
                _stringBuilder.AppendFormat("|{0}{1}", space, itemString);
                for (var i = 0; i < _rowLength - itemString.Length + 1; i++)
                {
                    _stringBuilder.Append(space);
                }
            }

            _stringBuilder.Append("|\n");
        }

        private string GetColumnName(int iColumn)
        {
            return _columnNames[iColumn];
        }

        private static string GetEmptyString(int iColumn)
        {
            return string.Empty;
        }

        private string GetItemName(int iColumn)
        {
            return _itemNames[_iRow, iColumn];
        }
    }
}