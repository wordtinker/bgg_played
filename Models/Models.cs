using System.Collections.Generic;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using System.Threading.Tasks;

namespace Models
{
    public class Game
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public int NumPlays { get; set; }
        public bool Own { get; set; }
        public bool PrevOwned { get; set; }
        public decimal PricePaid { get; set; }
        public decimal CurrValue { get; set; }
        public string AcquisitionDate { get; set; }

        public static Game Create(string[] array)
        {
            int.TryParse(array[3], out int nPlays);
            decimal.TryParse(array[42], out decimal pricePaid);
            decimal.TryParse(array[44], out decimal currValue);
            return new Game
            {
                Name = array[0],
                Id = array[1],
                NumPlays = nPlays,
                Own = array[5].Equals("1"),
                PrevOwned = array[10].Equals("1"),
                PricePaid = pricePaid,
                CurrValue = currValue,
                AcquisitionDate = array[46]
            };
        }
    }
    public static class CSVFileReader
    {
        public static string Extension { get; } = "CSV files (*.csv)|*.csv";
        public static IEnumerable<Game> ReadGames(string fileName)
        {
            // open the CSV file with headers
            using (CsvReader csv = new CsvReader(new StreamReader(fileName), true))
            {
                foreach(string[] line in csv)
                {
                    yield return Game.Create(line);
                }
            }
        }
    }
    public static class DataCollector
    {
        public static async Task<int> GetMinutesPlayedAsync(string user, string gameID)
        {
            await Task.Delay(200);
            return 120;
            // TODO STUB
        }
    }
}
