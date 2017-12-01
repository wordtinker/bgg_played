﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModelInterfaces
{
    public interface IPlay
    {
        string GameId { get; set; }
        int Minutes { get; set; }
    }
    public interface IGame
    {
        string AcquisitionDate { get; set; }
        decimal CurrValue { get; set; }
        string Id { get; set; }
        string Name { get; set; }
        int NumPlays { get; set; }
        bool Own { get; set; }
        bool PrevOwned { get; set; }
        decimal PricePaid { get; set; }
    }
    public interface IFileReader
    {
        string Extension { get; }
        IEnumerable<IGame> ReadGames(string fileName);
    }
    public interface IDataProvider
    {
        Task<List<IPlay>> GetPlaysAsync(string user);
    }
}