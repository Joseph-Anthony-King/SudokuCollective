using System;
using System.Collections.Generic;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models;

namespace SudokuCollective.Data.Models
{
    public class RepositoryResponse : IRepositoryResponse
    {
        public bool IsSuccess { get; set; }
        public Exception Exception { get; set; }
        public IDomainEntity Object { get; set; }
        public List<IDomainEntity> Objects { get; set; }

        public RepositoryResponse()
        {
            IsSuccess = false;
            Exception = null;
            Object = null;
            Objects = new List<IDomainEntity>();
        }
    }
}