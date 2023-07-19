using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using SudokuCollective.Core.Interfaces.Models.DomainObjects.Results;

namespace SudokuCollective.Data.Models.Results
{
    public class AnnonymousGameResult : IAnnonymousGameResult
    {
        [JsonPropertyName("rows")]
        public List<List<int>> SudokuMatrix { get; set; }

        public AnnonymousGameResult()
        {
            SudokuMatrix = new List<List<int>>();
        }

        public AnnonymousGameResult(int[][] sudokuMatrix)
        {
            var result = new List<List<int>>();

            foreach (var array in sudokuMatrix) {
                
                result.Add(array.ToList());
            }
            
            SudokuMatrix = result;
        }

        public AnnonymousGameResult(List<List<int>> sudokuMatrix)
        {
            SudokuMatrix = sudokuMatrix;
        }
    }
}
