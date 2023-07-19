using NUnit.Framework;

namespace SudokuCollective.Test.TestCases.Encryption
{
    public class EncryptionShould
    {
        private string key;

        [SetUp]
        public void SetUp()
        {
            key = "a7d74763578a4d77a05ed72094cd50eb";
        }

        [Test, Category("Encryption")]
        public void EncryptStrings()
        {
            // Arrange
            var value = "value";

            // Act
            var encryptedValue = Encrypt.Encryption.EncryptString(value, key);

            // Assert
            Assert.That(string.Equals(value, encryptedValue), Is.False);
        }

        [Test, Category("Encryption")]
        public void DecryptStrings()
        {
            // Arrange
            var value = "value";
            var encryptedValue = Encrypt.Encryption.EncryptString(value, key);

            // Act
            var decryptedValue = Encrypt.Encryption.DecryptString(encryptedValue, key);

            // Assert
            Assert.That(string.Equals(value, decryptedValue), Is.True);
        }
    }
}
