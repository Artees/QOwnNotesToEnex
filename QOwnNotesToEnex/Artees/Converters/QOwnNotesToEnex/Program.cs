using System.IO;

namespace Artees.Converters.QOwnNotesToEnex
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var enex = Enex.Create(args[0]);
            File.WriteAllText(args[1], enex.OuterXml);
        }
    }
}