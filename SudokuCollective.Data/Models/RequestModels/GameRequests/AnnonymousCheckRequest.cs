﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SudokuCollective.Core.Interfaces.APIModels.RequestModels.GameRequests;
using SudokuCollective.Data.Validation.Attributes;

namespace SudokuCollective.Data.Models.RequestModels
{
    public class AnnonymousCheckRequest : IAnnonymousCheckRequest
    {
        [Required, RowValidated]
        public List<int> FirstRow { get; set; }
        [Required, RowValidated]
        public List<int> SecondRow { get; set; }
        [Required, RowValidated]
        public List<int> ThirdRow { get; set; }
        [Required, RowValidated]
        public List<int> FourthRow { get; set; }
        [Required, RowValidated]
        public List<int> FifthRow { get; set; }
        [Required, RowValidated]
        public List<int> SixthRow { get; set; }
        [Required, RowValidated]
        public List<int> SeventhRow { get; set; }
        [Required, RowValidated]
        public List<int> EighthRow { get; set; }
        [Required, RowValidated]
        public List<int> NinthRow { get; set; }

        public AnnonymousCheckRequest()
        {
            FirstRow = new List<int>();
            SecondRow = new List<int>();
            ThirdRow = new List<int>();
            FourthRow = new List<int>();
            FifthRow = new List<int>();
            SixthRow = new List<int>();
            SeventhRow = new List<int>();
            EighthRow = new List<int>();
            NinthRow = new List<int>();
        }
    }
}
