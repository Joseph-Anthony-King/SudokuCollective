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
        public IDomainEntity DomainObject { get; set; }
        public List<IDomainEntity> DomainObjects { get; set; }
        public object Object { get; set; }
        public List<object> Objects { get; set; }

        public RepositoryResponse()
        {
            IsSuccess = false;
            Exception = null;
            DomainObject = null;
            DomainObjects = [];
            Object = null;
            Objects = [];
        }
    }
}