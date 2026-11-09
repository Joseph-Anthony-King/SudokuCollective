using System;
using System.Collections.Generic;
using SudokuCollective.Core.Interfaces.Models;

namespace SudokuCollective.Core.Interfaces.ServiceModels
{
    public interface IRepositoryResponse
    {
        bool IsSuccess { get; set; }
        Exception Exception { get; set; }
        IDomainEntity DomainObject { get; set; }
        List<IDomainEntity> DomainObjects { get; set; }
        object Object { get; set; }
        List<object> Objects { get; set; }
    }
}
