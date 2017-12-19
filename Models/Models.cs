﻿using System.Collections.Generic;
using LumenWorks.Framework.IO.Csv;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Net.Http;
using System.Net;
using System.Linq;
using System;
using Models.Interfaces;

namespace Models
{
    public class Play : IPlay
    {
        public string GameId { get; set; }
        public int Minutes { get; set; }
    }
    public class Game : IGame
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public int NumPlays { get; set; }
        public bool Own { get; set; }
        public bool PrevOwned { get; set; }
        public decimal PricePaid { get; set; }
        public decimal CurrValue { get; set; }
        public string AcquisitionDate { get; set; }
    }
    internal static class GameCreator
    {
        internal static IGame Create(string[] array)
        {
            int.TryParse(array[3], out int nPlays);
            decimal.TryParse(array[43], out decimal pricePaid);
            decimal.TryParse(array[45], out decimal currValue);
            return new Game
            {
                Name = array[0],
                Id = array[1],
                NumPlays = nPlays,
                Own = array[5].Equals("1"),
                PrevOwned = array[10].Equals("1"),
                PricePaid = pricePaid,
                CurrValue = currValue,
                AcquisitionDate = array[47]
            };
        }
    }
    public class CSVFileReader : IFileReader
    {
        public string Extension { get; } = "CSV files (*.csv)|*.csv";
        public IEnumerable<IGame> ReadGames(string fileName)
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
            List<IPlay> plays = new List<IPlay>();
            // Keep looking for pages with plays
            for (int page = 1; ; page++)
            {
                XDocument doc = await BGGAPI.GetPlays(user, page);
                List<Play> playsOnPage = doc.FilterPlays();
                if (playsOnPage.Count == 0) break;
                plays.AddRange(playsOnPage);
            }
            return plays;
        }
    }
    internal static class FilterExtensions
    {
        public static List<Play> FilterPlays(this XDocument doc)
        {
            try
            {
                var plays = from node in doc.Root.Elements("play")
                            select new Play
                            {
                                GameId = node.Descendants("item").First().Attribute("objectid").Value,
                                Minutes = int.Parse(node.Attribute("length").Value)
                            };
                return plays.ToList();
            }
            catch (Exception)
            {
                return new List<Play>();
            }
        }
    }
    internal static class BGGAPI
    {
        private const int DEFAULT_DELAY = 1000;
        private static async Task<XDocument> GetXMLFrom(string uri)
        {
            // use separate instance every time instead one static instance
            // better spam prevention
            using (HttpClient client = new HttpClient())
            {
                do
                {
                    using (HttpResponseMessage response = await client.GetAsync(uri))
                    {
                        // wait before asking for result again
                        await Task.Delay(DEFAULT_DELAY);
                        // throw on errors
                        response.EnsureSuccessStatusCode();

                        if (response.StatusCode != HttpStatusCode.OK) continue;

                        // Gather results
                        using (HttpContent content = response.Content)
                        {
                            byte[] bytes = await response.Content.ReadAsByteArrayAsync();
                            XDocument doc;
                            using (MemoryStream ms = new MemoryStream(bytes))
                            {
                                doc = XDocument.Load(ms);
                            }
                            return doc;
                        }
                    }
                } while (true);
            }
        }

        internal static async Task<XDocument> GetPlays(string userName, int pageNumber)
        {
            string baseURI = "https://www.boardgamegeek.com/xmlapi2/plays?username={0}&page={1}";
            string URI = string.Format(baseURI, userName, pageNumber);
            return await GetXMLFrom(URI);
        }
    }
}
