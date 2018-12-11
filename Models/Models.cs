using System.Collections.Generic;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using System.Threading.Tasks;
using Models.Interfaces;
using BGG;

namespace Models
{
    public class Game : IGameModel
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int NumPlays { get; set; }
        public bool Own { get; set; }
        public bool PrevOwned { get; set; }
        public decimal PricePaid { get; set; }
        public decimal CurrValue { get; set; }
        public string AcquisitionDate { get; set; }
    }
    internal static class GameCreator
    {
        internal static IGameModel Create(string[] array)
        {
            int.TryParse(array[3], out int nPlays); // default if fails
            decimal.TryParse(array[44], out decimal pricePaid); // default if fails 
            decimal.TryParse(array[46], out decimal currValue); // default if fails
            return new Game
            {
                Name = array[0],
                Id = int.Parse(array[1]), // must fail if id is broken
                NumPlays = nPlays,
                Own = array[5].Equals("1"),
                PrevOwned = array[10].Equals("1"),
                PricePaid = pricePaid,
                CurrValue = currValue,
                AcquisitionDate = array[48]
            };
        }
    }
    public class CSVFileReader : IFileReader
    {
        public string Extension { get; } = "CSV files (*.csv)|*.csv";
        public IEnumerable<IGameModel> ReadGames(string fileName)
        {
            // open the CSV file with headers
            using (CsvReader csv = new CsvReader(new StreamReader(fileName), true))
            {
                foreach(string[] line in csv)
                {
                    yield return GameCreator.Create(line);
                }
            }
        }
    }
    public class DataCollector : IDataProvider
    {
        public async Task<List<IPlay>> GetPlaysAsync(string user)
        {
            API bggAPI = new API(new APIConfig());
            return await bggAPI.GetPlaysAsync(user);
        }
    }
}
