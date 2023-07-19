using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.ServiceModels;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Repositories;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;

namespace SudokuCollective.Test.Repositories
{
    public class MockedRolesRepository
    {
        private readonly DatabaseContext context;
        internal Mock<IRolesRepository<Role>> SuccessfulRequest { get; set; }
        internal Mock<IRolesRepository<Role>> FailedRequest { get; set; }

        public MockedRolesRepository(DatabaseContext ctxt)
        {
            context = ctxt;

            SuccessfulRequest = new Mock<IRolesRepository<Role>>();
            FailedRequest = new Mock<IRolesRepository<Role>>();

            #region SuccessfulRequest
            SuccessfulRequest.Setup(rolesRepo =>
                rolesRepo.AddAsync(It.IsAny<Role>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = new Role() { RoleLevel = RoleLevel.NULL }
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(rolesRepo =>
                rolesRepo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Roles.FirstOrDefault(r => r.Id == 3)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(rolesRepo =>
                rolesRepo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Objects = context.Roles
                            .Where(r => r.RoleLevel != RoleLevel.NULL && r.RoleLevel != RoleLevel.SUPERUSER)
                            .ToList()
                            .ConvertAll(r => (IDomainEntity)r)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(rolesRepo =>
                rolesRepo.UpdateAsync(It.IsAny<Role>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true,
                        Object = context.Roles.FirstOrDefault(r => r.Id == 3)
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(rolesRepo =>
                rolesRepo.UpdateRangeAsync(It.IsAny<List<Role>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(rolesRepo =>
                rolesRepo.DeleteAsync(It.IsAny<Role>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(rolesRepo =>
                rolesRepo.DeleteRangeAsync(It.IsAny<List<Role>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = true
                    } as IRepositoryResponse));

            SuccessfulRequest.Setup(rolesRepo =>
                rolesRepo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(rolesRepo =>
                rolesRepo.HasRoleLevelAsync(It.IsAny<RoleLevel>()))
                    .Returns(Task.FromResult(true));

            SuccessfulRequest.Setup(rolesRepo =>
                rolesRepo.IsListValidAsync(It.IsAny<List<int>>()))
                    .Returns(Task.FromResult(true));
            #endregion

            #region FailedRequest
            FailedRequest.Setup(rolesRepo =>
                rolesRepo.AddAsync(It.IsAny<Role>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(rolesRepo =>
                rolesRepo.GetAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(rolesRepo =>
                rolesRepo.GetAllAsync())
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(rolesRepo =>
                rolesRepo.UpdateAsync(It.IsAny<Role>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(rolesRepo =>
                rolesRepo.UpdateRangeAsync(It.IsAny<List<Role>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(rolesRepo =>
                rolesRepo.DeleteAsync(It.IsAny<Role>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(rolesRepo =>
                rolesRepo.DeleteRangeAsync(It.IsAny<List<Role>>()))
                    .Returns(Task.FromResult(new RepositoryResponse()
                    {
                        IsSuccess = false
                    } as IRepositoryResponse));

            FailedRequest.Setup(rolesRepo =>
                rolesRepo.HasEntityAsync(It.IsAny<int>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(rolesRepo =>
                rolesRepo.HasRoleLevelAsync(It.IsAny<RoleLevel>()))
                    .Returns(Task.FromResult(false));

            FailedRequest.Setup(rolesRepo =>
                rolesRepo.IsListValidAsync(It.IsAny<List<int>>()))
                    .Returns(Task.FromResult(false));
            #endregion
        }
    }
}
