using System;

namespace SudokuCollective.Core.Interfaces.Models.DomainEntities
{
    public interface IGalleryApp : IDomainEntity
    {
        string Name { get; set; }
        string Url { get; set; }
        string SourceCodeUrl { get; set; }
        string CreatedBy { get; set; }
        int UserCount { get; set; }
        DateTime DateCreated { get; set; }
        DateTime DateUpdated { get; set; }
        void NullifySourceCodeUrl();
    }
}
