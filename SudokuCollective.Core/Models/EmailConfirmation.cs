using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Messages;
using SudokuCollective.Core.Utilities;
using SudokuCollective.Core.Validation.Attributes;

namespace SudokuCollective.Core.Models
{
    public class EmailConfirmation : IEmailConfirmation
    {
        #region Fields
        private string _token = string.Empty;
        private string _oldEmailAddress = string.Empty;
        private string _newEmailAddress = string.Empty;
        private readonly EmailValidatedAttribute _emailValidatedAttribute = new();
        private readonly GuidValidatedAttribute _guidValidator = new();
        #endregion

        #region Properties
        [Required, JsonPropertyName("id")]
        public int Id { get; set; }
        [Required, JsonPropertyName("token"), GuidValidated(ErrorMessage = AttributeMessages.InvalidToken)]
        public string Token
        {
            get => _token;
            set => _token = CoreUtilities.SetField(
                value, 
                _guidValidator, 
                AttributeMessages.InvalidToken);
        }
        [Required, JsonPropertyName("confirmationType")]
        public EmailConfirmationType ConfirmationType { get; set; }
        [Required, JsonPropertyName("isUpdate")]
        public bool IsUpdate 
        { 
            get => !string.IsNullOrEmpty(OldEmailAddress) && !string.IsNullOrEmpty(NewEmailAddress);
        }
        [Required, JsonPropertyName("userId")]
        public int UserId { get; set; }
        [Required, JsonPropertyName("appId")]
        public int AppId { get; set; }
        [JsonPropertyName("oldEmailAddress"), EmailValidated(ErrorMessage = AttributeMessages.InvalidOldEmail)]
        public string OldEmailAddress
        {
            get => _oldEmailAddress;
            set => _oldEmailAddress = SetOldEmailAddressField(
                value,
                _emailValidatedAttribute,
                AttributeMessages.InvalidOldEmail);
        }
        [JsonPropertyName("newEmailAddress"), EmailValidated(ErrorMessage = AttributeMessages.InvalidNewEmail)]
        public string NewEmailAddress
        {
            get => _newEmailAddress;
            set => _newEmailAddress = CoreUtilities.SetField(
                value, 
                _emailValidatedAttribute, 
                AttributeMessages.InvalidEmail);
        }
        [JsonPropertyName("oldEmailAddress")]
        public bool? OldEmailAddressConfirmed { get; set; }
        [Required, JsonPropertyName("expirationDate")]
        public DateTime ExpirationDate { get; set; }
        #endregion

        #region Constructors
        public EmailConfirmation()
        {
            Id = 0;
            ConfirmationType = EmailConfirmationType.NULL;
            UserId = 0;
            AppId = 0;
            OldEmailAddressConfirmed = null;
            ExpirationDate = DateTime.UtcNow.AddHours(24);

            _token = null;
            _oldEmailAddress = null;
            _newEmailAddress = null;
        }

        public EmailConfirmation(
            EmailConfirmationType confirmationType,
            int userId, 
            int appId) : this()
        {
            Token = Guid.NewGuid().ToString();
            ConfirmationType = confirmationType;
            UserId = userId;
            AppId = appId;
        }

        public EmailConfirmation(
            EmailConfirmationType confirmationType,
            int userId, 
            int appId, 
            string oldEmailAddress, 
            string newEmailAddress) : this()
        {
            Token = Guid.NewGuid().ToString();
            ConfirmationType = confirmationType;
            UserId = userId;
            AppId = appId;
            if (!string.IsNullOrEmpty(oldEmailAddress))
            {
                OldEmailAddress = oldEmailAddress;
            }
            if (!string.IsNullOrEmpty(newEmailAddress))
            {
                NewEmailAddress = newEmailAddress;
            }
            OldEmailAddressConfirmed = false;
        }

        [JsonConstructor]
        public EmailConfirmation(
            int id,
            EmailConfirmationType confirmationType,
            string token,
            int userId,
            int appId,
            string oldEmailAddress,
            string newEmailAddress,
            bool oldEmailAddressConfirmed,
            DateTime expirationDate)
        {
            Id = id;
            ConfirmationType = confirmationType;
            if (!string.IsNullOrEmpty(token))
            {
                Token = token;
            }
            UserId = userId;
            AppId = appId;
            if (!string.IsNullOrEmpty(oldEmailAddress))
            {
                OldEmailAddress = oldEmailAddress;
            }
            if (!string.IsNullOrEmpty(newEmailAddress))
            {
                NewEmailAddress = newEmailAddress;
            }
            OldEmailAddressConfirmed = oldEmailAddressConfirmed;
            ExpirationDate = expirationDate;
        }
        #endregion

        #region Methods
        public override string ToString() => string.Format(base.ToString() + ".Id:{0}.AppId:{1}.UserId:{2}", Id, AppId, UserId);

        public string ToJson() => JsonSerializer.Serialize(
            this,
            new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            });

        public IDomainEntity Cast<T>() => throw new System.NotImplementedException();

        private string SetOldEmailAddressField(
            string value, 
            RegularExpressionAttribute validator, 
            string errorMessage)
        {
            if (!string.IsNullOrEmpty(value) && validator.IsValid(value))
            {
                OldEmailAddressConfirmed = false;
                return value;
            }
            else
            {
                throw new ArgumentException(errorMessage);
            }
        }
        #endregion
    }
}
