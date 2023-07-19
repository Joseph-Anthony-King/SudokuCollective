using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Params;
using SudokuCollective.Core.Interfaces.Services;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Values;
using SudokuCollective.Test.TestData;

namespace SudokuCollective.Test.Services
{
    public class MockedValuesService
    {
        private MockedDifficultiesService MockedDifficultiesService { get; set; }
        private Values _settings { get; set; }
        private List<EnumListItem> _releaseEnvironments { get; set; }
        private List<EnumListItem> _sortValues { get; set; }
        private List<EnumListItem> _timeFrames { get; set; }

        internal Mock<IValuesService> Request { get; set; }

        public MockedValuesService(DatabaseContext context)
        {
            Request = new Mock<IValuesService>();

            _settings = TestObjects.GetValues();
            _releaseEnvironments = _settings.ReleaseEnvironments;
            _sortValues = _settings.SortValues;
            _timeFrames = _settings.TimeFrames;

            Request.Setup(service =>
                service.GetAsync(It.IsAny<Paginator>()))
                .Returns(Task.FromResult(TestObjects.GetResult() as IResult));

            Request.Setup(service =>
                service.GetReleaseEnvironments())
                .Returns(TestObjects.GetResult() as IResult);

            Request.Setup(service =>
                service.GetSortValues())
                .Returns(TestObjects.GetResult() as IResult);

            Request.Setup(service =>
                service.GetTimeFrames())
                .Returns(TestObjects.GetResult() as IResult);
        }
    }
}
