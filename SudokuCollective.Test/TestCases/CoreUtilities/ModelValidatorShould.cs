using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SudokuCollective.Core.Models;
using SudokuCollective.Core.Utilities;
using SudokuCollective.Data.Models;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Utilities
{
    public class ModelValidatorShould
    {
        DatabaseContext context;
        User user;
        ICollection<ValidationResult> validationResults;

        [SetUp]
        public async Task SetUp()
        {
            context = await TestDatabase.GetDatabaseContext();
            user = context.Users.FirstOrDefault(u => u.Id == 1);
            
            validationResults = new List<ValidationResult>();
        }

        [Test, Category("Utilities")]
        public void ValidateModels()
        {
            // Arrange

            // Act
            var result = ModelValidator.Validate<User>(
                user, 
                out validationResults);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test, Category("Utilities")]
        public void ReturnsFalseIfModelInvalid()
        {
            // Arrange
            user = new User();

            // Act
            var result = ModelValidator.Validate<User>(
                user, 
                out validationResults);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}