﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.APIModels.RequestModels;
using SudokuCollective.Core.Interfaces.APIModels.ResultModels;

namespace SudokuCollective.Core.Interfaces.Services
{
    public interface IGamesService : IService
    {
        Task<IGameResult> Create(ICreateGameRequest createGameRequest);
        Task<IGameResult> Update(int id, IUpdateGameRequest updateGameRequest);
        Task<IBaseResult> Delete(int id);
        Task<IGameResult> GetGame(int id, int appId);
        Task<IGamesResult> GetGames(IGamesRequest getGamesRequest);
        Task<IGameResult> GetMyGame(int gameid, IGamesRequest getMyGameRequest);
        Task<IGamesResult> GetMyGames(IGamesRequest getMyGameRequest);
        Task<IBaseResult> DeleteMyGame(int gameid, IGamesRequest getMyGameRequest);
        Task<IGameResult> Check(int id, IUpdateGameRequest updateGameRequest);
        Task<IAnnonymousGameResult> CreateAnnonymous(DifficultyLevel difficultyLevel);
        Task<IBaseResult> CheckAnnonymous(List<int> intList);
    }
}
