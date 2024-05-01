using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Payloads;

namespace SudokuCollective.Data.Models.Payloads
{
    public class UpdateUserRolePayload : IUpdateUserRolePayload
    {
        [Required, JsonPropertyName("roleIds")]
        public List<int> RoleIds { get; set; }

        public UpdateUserRolePayload()
        {
            RoleIds = [];
        }

        public UpdateUserRolePayload(int[] roleIds)
        {
            RoleIds = [.. roleIds];
        }

        public UpdateUserRolePayload(List<int> roleIds)
        {
            RoleIds = roleIds;
        }

        public static implicit operator JsonElement(UpdateUserRolePayload v)
        {
            return JsonSerializer.SerializeToElement(v, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        }
    }
}
