﻿using System.Collections.Generic;
using System.Threading.Tasks;
using BGG;

namespace Models.Interfaces
{
    public interface IGameModel : IGame
    {
        string AcquisitionDate { get; set; }
        decimal CurrValue { get; set; }
        int NumPlays { get; set; }
        bool Own { get; set; }
        bool PrevOwned { get; set; }
        decimal PricePaid { get; set; }
    }
    public interface IFileReader
    {
        string Extension { get; }
        IEnumerable<IGameModel> ReadGames(string fileName);
    }
    public interface IDataProvider
    {
        Task<List<IPlay>> GetPlaysAsync(string user);
    }
}
