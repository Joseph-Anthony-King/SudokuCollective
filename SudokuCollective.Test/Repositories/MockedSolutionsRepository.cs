using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;

namespace SudokuCollective.Test.Repositories
{
    public class MockedSolutionsRepository
    {
        private readonly DatabaseContext context;
        internal Mock<ISolutionsRepository<SudokuSolution>> SuccessfulRequest { get; set; }
        internal Mock<ISolutionsRepository<SudokuSolution>> FailedRequest { get; set; }

        public MockedSolutionsRepository(DatabaseContext ctxt)
        {
            context = ctxt;

            SuccessfulRequest = new Mock<ISolutionsRepository<SudokuSolution>>();
            FailedRequest = new Mock<ISolutionsRepository<SudokuSolution>>();

            #region SuccessfulRequest
            SuccessfulRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<SudokuSolution>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = new SudokuSolution()
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.SudokuSolutions.FirstOrDefault(s => s.Id == 1)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.SudokuSolutions.ToList().ConvertAll(s => (IDomainEntity)s)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.AddSolutionsAsync(It.IsAny<List<ISudokuSolution>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(repo =>
                repo.GetSolvedSolutionsAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.SudokuSolutions
                            .Where(s => s.DateSolved > DateTime.MinValue)
                            .ToList()
                            .ConvertAll(s => (IDomainEntity)s)
                    } as IRepositoryResponse));
            #endregion

            #region FailedRequest
            FailedRequest.Setup(repo =>
                repo.AddAsync(It.IsAny<SudokuSolution>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.AddSolutionsAsync(It.IsAny<List<ISudokuSolution>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(repo =>
                repo.GetSolvedSolutionsAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));
            #endregion
        }
    }
}
