using System.Buffers;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using SudokuCollective.Core.Models;
using SudokuCollective.Core.Utilities;
using SudokuCollective.Data.Models;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.TestCases.Utilities
{
    public class ConverterShould
    {
        private DatabaseContext context;
        IDomainEntityListConverter<User> sut;
        User user;
        string json;

        [SetUp]
        public async Task SetUp()
        {
            context = await TestDatabase.GetDatabaseContext();
            
            sut = new IDomainEntityListConverter<User>();
            user = context.Users.FirstOrDefault(u => u.Id == 1);
            json = user.ToJson();
        }

        [Test, Category("Utilities")]
        public void ReadJson()
        {
            // Arrange
            var utf8 = Encoding.UTF8;
            byte[] byteArray = utf8.GetBytes(json);

            var readOnlySequence = new ReadOnlySequence<byte>(byteArray);

            Utf8JsonReader reader = new Utf8JsonReader(readOnlySequence);

            // Act
            var result = sut.Read(
                ref reader, 
                typeof(User), 
                new JsonSerializerOptions() { AllowTrailingCommas = true });

            // Assert
            Assert.That(result, Is.TypeOf<User>());
        }

        [Test, Category("Utilities")]
        public void WriteJson()
        {
            try
            {
                // Arrange
                var buffer = new ArrayBufferWriter<byte>();
                Utf8JsonWriter writer = new Utf8JsonWriter(buffer);

                // Act
                sut.Write(
                    writer, 
                    user, 
                    new JsonSerializerOptions());

                // Assert
                Assert.That(true);
            }
            catch
            {
                // Catch error if fails and assert false
                Assert.That(false);
            }
        }
    }
}
