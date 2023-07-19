using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models;
using SudokuCollective.Core.Interfaces.Models.DomainEntities;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Values;
using SudokuCollective.Core.Models;
using SudokuCollective.Core.Utilities;

namespace SudokuCollective.Data.Models.Values
{
    public class Values : IValues
    {
        [JsonIgnore]
        ICollection<IDifficulty> IValues.Difficulties
        {
            get => Difficulties.ConvertAll(d => (IDifficulty)d);
            set => Difficulties = value.ToList().ConvertAll(d => (Difficulty)d);
        }
        [JsonPropertyName("difficulties"), JsonConverter(typeof(IDomainEntityListConverter<List<Difficulty>>))]
        public List<Difficulty> Difficulties { get; set; }
        [JsonIgnore]
        ICollection<IEnumListItem> IValues.ReleaseEnvironments
        {
            get => ReleaseEnvironments.ConvertAll(r => (IEnumListItem)r);
            set => ReleaseEnvironments = value.ToList().ConvertAll(r => (EnumListItem)r);
        }
        [JsonPropertyName("releaseEnvironments")]
        public List<EnumListItem> ReleaseEnvironments { get; set; }
        [JsonIgnore]
        ICollection<IEnumListItem> IValues.SortValues
        {
            get => SortValues.ConvertAll(r => (IEnumListItem)r);
            set => SortValues = value.ToList().ConvertAll(r => (EnumListItem)r);
        }
        [JsonPropertyName("sortValues")]
        public List<EnumListItem> SortValues { get; set; }
        [JsonIgnore]
        ICollection<IEnumListItem> IValues.TimeFrames
        {
            get => TimeFrames.ConvertAll(t => (IEnumListItem)t);
            set => TimeFrames = value.ToList().ConvertAll(t => (EnumListItem)t);
        }
        [JsonPropertyName("timeFrames")]
        public List<EnumListItem> TimeFrames { get; set; }
        [JsonIgnore]
        ICollection<IGalleryApp> IValues.Gallery 
        { 
            get => Gallery.ConvertAll(a => (IGalleryApp)a);
            set => Gallery = value.ToList().ConvertAll(a => (GalleryApp)a);
        }
        [JsonPropertyName("gallery"), JsonConverter(typeof(IDomainEntityListConverter<List<GalleryApp>>))]
        public List<GalleryApp> Gallery { get; set; }

        public Values()
        {
            Difficulties = new List<Difficulty>();
            ReleaseEnvironments = new List<EnumListItem>();
            SortValues = new List<EnumListItem>();
            TimeFrames = new List<EnumListItem>();
            Gallery = new List<GalleryApp>();
        }
    }
}
