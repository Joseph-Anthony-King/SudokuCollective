using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Moq;
using SudokuCollective.Core.Enums;
using SudokuCollective.Core.Models;
using SudokuCollective.Data.Models;
using SudokuCollective.Data.Models.Params;
using SudokuCollective.Data.Models.Payloads;
using SudokuCollective.Data.Models.Requests;
using SudokuCollective.Data.Models.Results;
using SudokuCollective.Data.Models.Values;

namespace SudokuCollective.Test.TestData
{
    public static class TestObjects
    {
        public static string GetLicense() => "d17f0ed3-be9a-450a-a146-f6733db2bbdb";

        public static string GetInvalidLicense() => "a0fa1a7c-af21-433e-8e7f-f94f0086f45e";

        public static string GetSecondLicense() => "03c0d43f-3ad8-490a-a131-f73c81fe02c0";
        
        public static string GetThirdLicense() => "aaa6c3ec-ec85-46e7-9793-68a6e2bf4904";

        public static string GetToken() => "d17f0ed3-be9a-450a-a146-f6733db2bbdb";
        
        public static string GetEncryptionKey() => "0b13b6b05d7c498ea8d222f28b921f5f";

        public static Paginator GetPaginator() => new Paginator()
            {
                Page = 1,
                ItemsPerPage = 10,
                SortBy = SortValue.NULL,
                OrderByDescending = false,
                IncludeCompletedGames = false
            };

        public static AnnonymousCheckRequest GetValidAnnonymousCheckPayload() =>
            new AnnonymousCheckRequest
            {
                FirstRow = new List<int> { 7, 8, 5, 4, 1, 3, 2, 9, 6 },
                SecondRow = new List<int> { 1, 4, 2, 8, 6, 9, 5, 7, 3 },
                ThirdRow = new List<int> { 6, 9, 3, 2, 7, 5, 4, 1, 8 },
                FourthRow = new List<int> { 5, 1, 4, 3, 8, 2, 7, 6, 9 },
                FifthRow = new List<int> { 2, 6, 7, 9, 4, 1, 3, 8, 5 },
                SixthRow = new List<int> { 8, 3, 9, 7, 5, 6, 1, 2, 4 },
                SeventhRow = new List<int> { 4, 2, 1, 6, 3, 8, 9, 5, 7 },
                EighthRow = new List<int> { 3, 5, 8, 1, 9, 7, 6, 4, 2 },
                NinthRow = new List<int> { 9, 7, 6, 5, 2, 4, 8, 3, 1 }
            };

        public static AnnonymousCheckRequest GetInvalidAnnonymousCheckPayload() =>
            new AnnonymousCheckRequest
            {
                FirstRow = new List<int> { 7, 8, 5, 4, 1, 3, 2, 9, 6, 0 },
                SecondRow = new List<int> { 1, 4, 2, 8, 6, 9, 5, 7, 3 },
                ThirdRow = new List<int> { 6, 9, 3, 2, 7, 5, 4, 1, 8 },
                FourthRow = new List<int> { 5, 1, 4, 3, 8, 2, 7, 6, 9 },
                FifthRow = new List<int> { 2, 6, 7, 9, 4, 1, 3, 8, 5 },
                SixthRow = new List<int> { 8, 3, 9, 7, 5, 6, 1, 2, 4 },
                SeventhRow = new List<int> { 4, 2, 1, 6, 3, 8, 9, 5, 7 },
                EighthRow = new List<int> { 3, 5, 8, 1, 9, 7, 6, 4, 2 },
                NinthRow = new List<int> { 9, 7, 6, 5, 2, 4, 8, 3, 1 }
            };

        public static List<List<int>> GetAnnonymousGame() => new List<List<int>>()
            {
                new List<int> { 7, 8, 5, 4, 1, 3, 2, 9, 6 },
                new List<int> { 1, 4, 2, 8, 6, 9, 5, 7, 3 },
                new List<int> { 6, 9, 3, 2, 7, 5, 4, 1, 8 },
                new List<int> { 5, 1, 4, 3, 8, 2, 7, 6, 9 },
                new List<int> { 2, 6, 7, 9, 4, 1, 3, 8, 5 },
                new List<int> { 8, 3, 9, 7, 5, 6, 1, 2, 4 },
                new List<int> { 4, 2, 1, 6, 3, 8, 9, 5, 7 },
                new List<int> { 3, 5, 8, 1, 9, 7, 6, 4, 2 },
                new List<int> { 9, 7, 6, 5, 2, 4, 8, 3, 1 }
            };

        public static SolutionPayload GetValidSolutionPayload() =>
            new SolutionPayload
            {
                FirstRow = new List<int> { 7, 8, 5, 4, 1, 3, 2, 9, 6 },
                SecondRow = new List<int> { 1, 4, 2, 8, 6, 9, 5, 7, 3 },
                ThirdRow = new List<int> { 6, 9, 3, 2, 7, 5, 4, 1, 8 },
                FourthRow = new List<int> { 5, 1, 4, 3, 8, 2, 7, 6, 9 },
                FifthRow = new List<int> { 2, 6, 7, 9, 4, 1, 3, 8, 5 },
                SixthRow = new List<int> { 8, 3, 9, 7, 5, 6, 1, 2, 4 },
                SeventhRow = new List<int> { 4, 2, 1, 6, 3, 8, 9, 5, 7 },
                EighthRow = new List<int> { 3, 5, 8, 1, 9, 7, 6, 4, 2 },
                NinthRow = new List<int> { 9, 7, 6, 5, 2, 4, 8, 3, 1 }
            };

        public static SolutionPayload GetInvalidSolutionPayload() =>
            new SolutionPayload
            {
                FirstRow = new List<int> { 7, 8, 5, 4, 1, 3, 2, 9, 6, 0 },
                SecondRow = new List<int> { 1, 4, 2, 8, 6, 9, 5, 7, 3 },
                ThirdRow = new List<int> { 6, 9, 3, 2, 7, 5, 4, 1, 8 },
                FourthRow = new List<int> { 5, 1, 4, 3, 8, 2, 7, 6, 9 },
                FifthRow = new List<int> { 2, 6, 7, 9, 4, 1, 3, 8, 5 },
                SixthRow = new List<int> { 8, 3, 9, 7, 5, 6, 1, 2, 4 },
                SeventhRow = new List<int> { 4, 2, 1, 6, 3, 8, 9, 5, 7 },
                EighthRow = new List<int> { 3, 5, 8, 1, 9, 7, 6, 4, 2 },
                NinthRow = new List<int> { 9, 7, 6, 5, 2, 4, 8, 3, 1 }
            };

        public static List<SudokuCell> GetUpdateSudokuCells(int updatedValue)
        {
            var cells = new List<SudokuCell>()
            {
                new SudokuCell(81,1,1,1,1,4,4,false,1),
                new SudokuCell(58,2,2,1,1,2,updatedValue,true,1),
                new SudokuCell(57,3,3,1,1,8,8,false,1),
                new SudokuCell(56,4,4,2,1,6,0,true,1),
                new SudokuCell(55,5,5,2,1,1,0,true,1),
                new SudokuCell(54,6,6,2,1,5,0,true,1),
                new SudokuCell(53,7,7,3,1,9,0,true,1),
                new SudokuCell(52,8,8,3,1,3,0,true,1),
                new SudokuCell(51,9,9,3,1,7,0,true,1),
                new SudokuCell(50,10,1,1,2,3,3,false,1),
                new SudokuCell(49,11,2,1,2,9,0,true,1),
                new SudokuCell(48,12,3,1,2,6,6,false,1),
                new SudokuCell(47,13,4,2,2,2,0,true,1),
                new SudokuCell(46,14,5,2,2,8,0,true,1),
                new SudokuCell(45,15,6,2,2,7,0,true,1),
                new SudokuCell(44,16,7,3,2,1,1,false,1),
                new SudokuCell(43,17,8,3,2,4,4,false,1),
                new SudokuCell(42,18,9,3,2,5,0,false,1),
                new SudokuCell(59,19,1,1,3,1,0,true,1),
                new SudokuCell(61,20,2,1,3,5,0,true,1),
                new SudokuCell(80,21,3,1,3,7,7,false,1),
                new SudokuCell(62,22,4,2,3,3,0,true,1),
                new SudokuCell(79,23,5,2,3,4,0,true,1),
                new SudokuCell(78,24,6,2,3,9,9,false,1),
                new SudokuCell(77,25,7,3,3,6,0,true,1),
                new SudokuCell(76,26,8,3,3,8,0,true,1),
                new SudokuCell(75,27,9,3,3,2,2,false,1),
                new SudokuCell(74,28,1,4,4,5,0,true,1),
                new SudokuCell(73,29,2,4,4,8,8,false,1),
                new SudokuCell(72,30,3,4,4,4,4,false,1),
                new SudokuCell(71,31,4,5,4,9,0,true,1),
                new SudokuCell(70,32,5,5,4,3,0,true,1),
                new SudokuCell(69,33,6,5,4,6,0,true,1),
                new SudokuCell(68,34,7,6,4,2,0,true,1),
                new SudokuCell(67,35,8,6,4,7,0,true,1),
                new SudokuCell(66,36,9,6,4,1,0,true,1),
                new SudokuCell(65,37,1,4,5,7,7,false,1),
                new SudokuCell(64,38,2,4,5,6,0,true,1),
                new SudokuCell(63,39,3,4,5,1,0,true,1),
                new SudokuCell(41,40,4,5,5,8,8,false,1),
                new SudokuCell(40,41,5,5,5,2,2,false,1),
                new SudokuCell(39,42,6,5,5,4,4,false,1),
                new SudokuCell(38,43,7,6,5,5,5,false,1),
                new SudokuCell(16,44,8,6,5,9,0,true,1),
                new SudokuCell(15,45,9,6,5,3,0,true,1),
                new SudokuCell(14,46,1,4,6,2,2,false,1),
                new SudokuCell(13,47,2,4,6,3,0,true,1),
                new SudokuCell(12,48,3,4,6,9,9,false,1),
                new SudokuCell(11,49,4,5,6,7,7,false,1),
                new SudokuCell(10,50,5,5,6,5,5,false,1),
                new SudokuCell(17,51,6,5,6,1,0,true,1),
                new SudokuCell(9,52,7,6,6,4,4,false,1),
                new SudokuCell(7,53,8,6,6,6,6,false,1),
                new SudokuCell(6,54,9,6,6,8,0,true,1),
                new SudokuCell(5,55,1,7,7,9,0,true,1),
                new SudokuCell(4,56,2,7,7,7,0,true,1),
                new SudokuCell(3,57,3,7,7,5,0,true,1),
                new SudokuCell(2,58,4,8,7,1,0,true,1),
                new SudokuCell(1,59,5,8,7,6,0,true,1),
                new SudokuCell(8,60,6,8,7,8,8,false,1),
                new SudokuCell(18,61,7,9,7,3,3,false,1),
                new SudokuCell(19,62,8,9,7,2,2,false,1),
                new SudokuCell(20,63,9,9,7,4,0,true,1),
                new SudokuCell(37,64,1,7,8,6,0,true,1),
                new SudokuCell(36,65,2,7,8,1,0,true,1),
                new SudokuCell(35,66,3,7,8,2,2,false,1),
                new SudokuCell(34,67,4,8,8,4,0,true,1),
                new SudokuCell(33,68,5,8,8,7,7,false,1),
                new SudokuCell(32,69,6,8,8,3,0,true,1),
                new SudokuCell(31,70,7,9,8,8,0,true,1),
                new SudokuCell(30,71,8,9,8,5,0,true,1),
                new SudokuCell(29,72,9,9,8,9,0,true,1),
                new SudokuCell(28,73,1,7,9,8,0,true,1),
                new SudokuCell(27,74,2,7,9,4,0,true,1),
                new SudokuCell(26,75,3,7,9,3,0,true,1),
                new SudokuCell(25,76,4,8,9,5,0,true,1),
                new SudokuCell(24,77,5,8,9,9,0,true,1),
                new SudokuCell(23,78,6,8,9,2,0,true,1),
                new SudokuCell(22,79,7,9,9,7,7,false,1),
                new SudokuCell(21,80,8,9,9,1,0,true,1),
                new SudokuCell(60,81,9,9,9,6,6,false,1)
            };

            return cells;
        }

        public static List<SudokuCell> GetUpdateInvalidSudokuCells(int updatedValue)
        {
            var cells = new List<SudokuCell>()
            {
                new SudokuCell(81,1,1,1,1,4,4,false,2),
                new SudokuCell(58,2,2,1,1,2,updatedValue,true,1),
                new SudokuCell(57,3,3,1,1,8,8,false,1),
                new SudokuCell(56,4,4,2,1,6,0,true,1),
                new SudokuCell(55,5,5,2,1,1,0,true,1),
                new SudokuCell(54,6,6,2,1,5,0,true,1),
                new SudokuCell(53,7,7,3,1,9,0,true,1),
                new SudokuCell(52,8,8,3,1,3,0,true,1),
                new SudokuCell(51,9,9,3,1,7,0,true,1),
                new SudokuCell(50,10,1,1,2,3,3,false,1),
                new SudokuCell(49,11,2,1,2,9,0,true,1),
                new SudokuCell(48,12,3,1,2,6,6,false,1),
                new SudokuCell(47,13,4,2,2,2,0,true,1),
                new SudokuCell(46,14,5,2,2,8,0,true,1),
                new SudokuCell(45,15,6,2,2,7,0,true,1),
                new SudokuCell(44,16,7,3,2,1,1,false,1),
                new SudokuCell(43,17,8,3,2,4,4,false,1),
                new SudokuCell(42,18,9,3,2,5,0,false,1),
                new SudokuCell(59,19,1,1,3,1,0,true,1),
                new SudokuCell(61,20,2,1,3,5,0,true,1),
                new SudokuCell(80,21,3,1,3,7,7,false,1),
                new SudokuCell(62,22,4,2,3,3,0,true,1),
                new SudokuCell(79,23,5,2,3,4,0,true,1),
                new SudokuCell(78,24,6,2,3,9,9,false,1),
                new SudokuCell(77,25,7,3,3,6,0,true,1),
                new SudokuCell(76,26,8,3,3,8,0,true,1),
                new SudokuCell(75,27,9,3,3,2,2,false,1),
                new SudokuCell(74,28,1,4,4,5,0,true,1),
                new SudokuCell(73,29,2,4,4,8,8,false,1),
                new SudokuCell(72,30,3,4,4,4,4,false,1),
                new SudokuCell(71,31,4,5,4,9,0,true,1),
                new SudokuCell(70,32,5,5,4,3,0,true,1),
                new SudokuCell(69,33,6,5,4,6,0,true,1),
                new SudokuCell(68,34,7,6,4,2,0,true,1),
                new SudokuCell(67,35,8,6,4,7,0,true,1),
                new SudokuCell(66,36,9,6,4,1,0,true,1),
                new SudokuCell(65,37,1,4,5,7,7,false,1),
                new SudokuCell(64,38,2,4,5,6,0,true,1),
                new SudokuCell(63,39,3,4,5,1,0,true,1),
                new SudokuCell(41,40,4,5,5,8,8,false,1),
                new SudokuCell(40,41,5,5,5,2,2,false,1),
                new SudokuCell(39,42,6,5,5,4,4,false,1),
                new SudokuCell(38,43,7,6,5,5,5,false,1),
                new SudokuCell(16,44,8,6,5,9,0,true,1),
                new SudokuCell(15,45,9,6,5,3,0,true,1),
                new SudokuCell(14,46,1,4,6,2,2,false,1),
                new SudokuCell(13,47,2,4,6,3,0,true,1),
                new SudokuCell(12,48,3,4,6,9,9,false,1),
                new SudokuCell(11,49,4,5,6,7,7,false,1),
                new SudokuCell(10,50,5,5,6,5,5,false,1),
                new SudokuCell(17,51,6,5,6,1,0,true,1),
                new SudokuCell(9,52,7,6,6,4,4,false,1),
                new SudokuCell(7,53,8,6,6,6,6,false,1),
                new SudokuCell(6,54,9,6,6,8,0,true,1),
                new SudokuCell(5,55,1,7,7,9,0,true,1),
                new SudokuCell(4,56,2,7,7,7,0,true,1),
                new SudokuCell(3,57,3,7,7,5,0,true,1),
                new SudokuCell(2,58,4,8,7,1,0,true,1),
                new SudokuCell(1,59,5,8,7,6,0,true,1),
                new SudokuCell(8,60,6,8,7,8,8,false,1),
                new SudokuCell(18,61,7,9,7,3,3,false,1),
                new SudokuCell(19,62,8,9,7,2,2,false,1),
                new SudokuCell(20,63,9,9,7,4,0,true,1),
                new SudokuCell(37,64,1,7,8,6,0,true,1),
                new SudokuCell(36,65,2,7,8,1,0,true,1),
                new SudokuCell(35,66,3,7,8,2,2,false,1),
                new SudokuCell(34,67,4,8,8,4,0,true,1),
                new SudokuCell(33,68,5,8,8,7,7,false,1),
                new SudokuCell(32,69,6,8,8,3,0,true,1),
                new SudokuCell(31,70,7,9,8,8,0,true,1),
                new SudokuCell(30,71,8,9,8,5,0,true,1),
                new SudokuCell(29,72,9,9,8,9,0,true,1),
                new SudokuCell(28,73,1,7,9,8,0,true,1),
                new SudokuCell(27,74,2,7,9,4,0,true,1),
                new SudokuCell(26,75,3,7,9,3,0,true,1),
                new SudokuCell(25,76,4,8,9,5,0,true,1),
                new SudokuCell(24,77,5,8,9,9,0,true,1),
                new SudokuCell(23,78,6,8,9,2,0,true,1),
                new SudokuCell(22,79,7,9,9,7,7,false,1),
                new SudokuCell(21,80,8,9,9,1,0,true,1),
                new SudokuCell(60,81,9,9,9,6,6,false,1)
            };

            return cells;
        }

        public static List<SudokuCell> GetValidSudokuCells()
        {
            var cells = new List<SudokuCell>()
            {
                new SudokuCell(81,1,1,1,1,4,4,false,1),
                new SudokuCell(58,2,2,1,1,2,0,true,1),
                new SudokuCell(57,3,3,1,1,8,8,false,1),
                new SudokuCell(56,4,4,2,1,6,0,true,1),
                new SudokuCell(55,5,5,2,1,1,0,true,1),
                new SudokuCell(54,6,6,2,1,5,0,true,1),
                new SudokuCell(53,7,7,3,1,9,0,true,1),
                new SudokuCell(52,8,8,3,1,3,0,true,1),
                new SudokuCell(51,9,9,3,1,7,0,true,1),
                new SudokuCell(50,10,1,1,2,3,3,false,1),
                new SudokuCell(49,11,2,1,2,9,0,true,1),
                new SudokuCell(48,12,3,1,2,6,6,false,1),
                new SudokuCell(47,13,4,2,2,2,0,true,1),
                new SudokuCell(46,14,5,2,2,8,0,true,1),
                new SudokuCell(45,15,6,2,2,7,0,true,1),
                new SudokuCell(44,16,7,3,2,1,1,false,1),
                new SudokuCell(43,17,8,3,2,4,4,false,1),
                new SudokuCell(42,18,9,3,2,5,0,false,1),
                new SudokuCell(59,19,1,1,3,1,0,true,1),
                new SudokuCell(61,20,2,1,3,5,0,true,1),
                new SudokuCell(80,21,3,1,3,7,7,false,1),
                new SudokuCell(62,22,4,2,3,3,0,true,1),
                new SudokuCell(79,23,5,2,3,4,0,true,1),
                new SudokuCell(78,24,6,2,3,9,9,false,1),
                new SudokuCell(77,25,7,3,3,6,0,true,1),
                new SudokuCell(76,26,8,3,3,8,0,true,1),
                new SudokuCell(75,27,9,3,3,2,2,false,1),
                new SudokuCell(74,28,1,4,4,5,0,true,1),
                new SudokuCell(73,29,2,4,4,8,8,false,1),
                new SudokuCell(72,30,3,4,4,4,4,false,1),
                new SudokuCell(71,31,4,5,4,9,0,true,1),
                new SudokuCell(70,32,5,5,4,3,0,true,1),
                new SudokuCell(69,33,6,5,4,6,0,true,1),
                new SudokuCell(68,34,7,6,4,2,0,true,1),
                new SudokuCell(67,35,8,6,4,7,0,true,1),
                new SudokuCell(66,36,9,6,4,1,0,true,1),
                new SudokuCell(65,37,1,4,5,7,7,false,1),
                new SudokuCell(64,38,2,4,5,6,0,true,1),
                new SudokuCell(63,39,3,4,5,1,0,true,1),
                new SudokuCell(41,40,4,5,5,8,8,false,1),
                new SudokuCell(40,41,5,5,5,2,2,false,1),
                new SudokuCell(39,42,6,5,5,4,4,false,1),
                new SudokuCell(38,43,7,6,5,5,5,false,1),
                new SudokuCell(16,44,8,6,5,9,0,true,1),
                new SudokuCell(15,45,9,6,5,3,0,true,1),
                new SudokuCell(14,46,1,4,6,2,2,false,1),
                new SudokuCell(13,47,2,4,6,3,0,true,1),
                new SudokuCell(12,48,3,4,6,9,9,false,1),
                new SudokuCell(11,49,4,5,6,7,7,false,1),
                new SudokuCell(10,50,5,5,6,5,5,false,1),
                new SudokuCell(17,51,6,5,6,1,0,true,1),
                new SudokuCell(9,52,7,6,6,4,4,false,1),
                new SudokuCell(7,53,8,6,6,6,6,false,1),
                new SudokuCell(6,54,9,6,6,8,0,true,1),
                new SudokuCell(5,55,1,7,7,9,0,true,1),
                new SudokuCell(4,56,2,7,7,7,0,true,1),
                new SudokuCell(3,57,3,7,7,5,0,true,1),
                new SudokuCell(2,58,4,8,7,1,0,true,1),
                new SudokuCell(1,59,5,8,7,6,0,true,1),
                new SudokuCell(8,60,6,8,7,8,8,false,1),
                new SudokuCell(18,61,7,9,7,3,3,false,1),
                new SudokuCell(19,62,8,9,7,2,2,false,1),
                new SudokuCell(20,63,9,9,7,4,0,true,1),
                new SudokuCell(37,64,1,7,8,6,0,true,1),
                new SudokuCell(36,65,2,7,8,1,0,true,1),
                new SudokuCell(35,66,3,7,8,2,2,false,1),
                new SudokuCell(34,67,4,8,8,4,0,true,1),
                new SudokuCell(33,68,5,8,8,7,7,false,1),
                new SudokuCell(32,69,6,8,8,3,0,true,1),
                new SudokuCell(31,70,7,9,8,8,0,true,1),
                new SudokuCell(30,71,8,9,8,5,0,true,1),
                new SudokuCell(29,72,9,9,8,9,0,true,1),
                new SudokuCell(28,73,1,7,9,8,0,true,1),
                new SudokuCell(27,74,2,7,9,4,0,true,1),
                new SudokuCell(26,75,3,7,9,3,0,true,1),
                new SudokuCell(25,76,4,8,9,5,0,true,1),
                new SudokuCell(24,77,5,8,9,9,0,true,1),
                new SudokuCell(23,78,6,8,9,2,0,true,1),
                new SudokuCell(22,79,7,9,9,7,7,false,1),
                new SudokuCell(21,80,8,9,9,1,0,true,1),
                new SudokuCell(60,81,9,9,9,6,6,false,1)
            };

            return cells;
        }

        public static List<SudokuCell> GetSolvedSudokuCells()
        {
            var cells = new List<SudokuCell>()
            {
                new SudokuCell(81,1,1,1,1,4,4,false,1),
                new SudokuCell(58,2,2,1,1,2,2,true,1),
                new SudokuCell(57,3,3,1,1,8,8,false,1),
                new SudokuCell(56,4,4,2,1,6,6,true,1),
                new SudokuCell(55,5,5,2,1,1,1,true,1),
                new SudokuCell(54,6,6,2,1,5,5,true,1),
                new SudokuCell(53,7,7,3,1,9,9,true,1),
                new SudokuCell(52,8,8,3,1,3,3,true,1),
                new SudokuCell(51,9,9,3,1,7,7,true,1),
                new SudokuCell(50,10,1,1,2,3,3,false,1),
                new SudokuCell(49,11,2,1,2,9,9,true,1),
                new SudokuCell(48,12,3,1,2,6,6,false,1),
                new SudokuCell(47,13,4,2,2,2,2,true,1),
                new SudokuCell(46,14,5,2,2,8,8,true,1),
                new SudokuCell(45,15,6,2,2,7,7,true,1),
                new SudokuCell(44,16,7,3,2,1,1,false,1),
                new SudokuCell(43,17,8,3,2,4,4,false,1),
                new SudokuCell(42,18,9,3,2,5,5,false,1),
                new SudokuCell(59,19,1,1,3,1,1,true,1),
                new SudokuCell(61,20,2,1,3,5,5,true,1),
                new SudokuCell(80,21,3,1,3,7,7,false,1),
                new SudokuCell(62,22,4,2,3,3,3,true,1),
                new SudokuCell(79,23,5,2,3,4,4,true,1),
                new SudokuCell(78,24,6,2,3,9,9,false,1),
                new SudokuCell(77,25,7,3,3,6,6,true,1),
                new SudokuCell(76,26,8,3,3,8,8,true,1),
                new SudokuCell(75,27,9,3,3,2,2,false,1),
                new SudokuCell(74,28,1,4,4,5,5,true,1),
                new SudokuCell(73,29,2,4,4,8,8,false,1),
                new SudokuCell(72,30,3,4,4,4,4,false,1),
                new SudokuCell(71,31,4,5,4,9,9,true,1),
                new SudokuCell(70,32,5,5,4,3,3,true,1),
                new SudokuCell(69,33,6,5,4,6,6,true,1),
                new SudokuCell(68,34,7,6,4,2,2,true,1),
                new SudokuCell(67,35,8,6,4,7,7,true,1),
                new SudokuCell(66,36,9,6,4,1,1,true,1),
                new SudokuCell(65,37,1,4,5,7,7,false,1),
                new SudokuCell(64,38,2,4,5,6,6,true,1),
                new SudokuCell(63,39,3,4,5,1,1,true,1),
                new SudokuCell(41,40,4,5,5,8,8,false,1),
                new SudokuCell(40,41,5,5,5,2,2,false,1),
                new SudokuCell(39,42,6,5,5,4,4,false,1),
                new SudokuCell(38,43,7,6,5,5,5,false,1),
                new SudokuCell(16,44,8,6,5,9,9,true,1),
                new SudokuCell(15,45,9,6,5,3,3,true,1),
                new SudokuCell(14,46,1,4,6,2,2,false,1),
                new SudokuCell(13,47,2,4,6,3,3,true,1),
                new SudokuCell(12,48,3,4,6,9,9,false,1),
                new SudokuCell(11,49,4,5,6,7,7,false,1),
                new SudokuCell(10,50,5,5,6,5,5,false,1),
                new SudokuCell(17,51,6,5,6,1,1,true,1),
                new SudokuCell(9,52,7,6,6,4,4,false,1),
                new SudokuCell(7,53,8,6,6,6,6,false,1),
                new SudokuCell(6,54,9,6,6,8,8,true,1),
                new SudokuCell(5,55,1,7,7,9,9,true,1),
                new SudokuCell(4,56,2,7,7,7,7,true,1),
                new SudokuCell(3,57,3,7,7,5,5,true,1),
                new SudokuCell(2,58,4,8,7,1,1,true,1),
                new SudokuCell(1,59,5,8,7,6,6,true,1),
                new SudokuCell(8,60,6,8,7,8,8,false,1),
                new SudokuCell(18,61,7,9,7,3,3,false,1),
                new SudokuCell(19,62,8,9,7,2,2,false,1),
                new SudokuCell(20,63,9,9,7,4,4,true,1),
                new SudokuCell(37,64,1,7,8,6,6,true,1),
                new SudokuCell(36,65,2,7,8,1,1,true,1),
                new SudokuCell(35,66,3,7,8,2,2,false,1),
                new SudokuCell(34,67,4,8,8,4,4,true,1),
                new SudokuCell(33,68,5,8,8,7,7,false,1),
                new SudokuCell(32,69,6,8,8,3,3,true,1),
                new SudokuCell(31,70,7,9,8,8,8,true,1),
                new SudokuCell(30,71,8,9,8,5,5,true,1),
                new SudokuCell(29,72,9,9,8,9,9,true,1),
                new SudokuCell(28,73,1,7,9,8,8,true,1),
                new SudokuCell(27,74,2,7,9,4,4,true,1),
                new SudokuCell(26,75,3,7,9,3,3,true,1),
                new SudokuCell(25,76,4,8,9,5,5,true,1),
                new SudokuCell(24,77,5,8,9,9,9,true,1),
                new SudokuCell(23,78,6,8,9,2,2,true,1),
                new SudokuCell(22,79,7,9,9,7,7,false,1),
                new SudokuCell(21,80,8,9,9,1,1,true,1),
                new SudokuCell(60,81,9,9,9,6,6,false,1)
            };

            return cells;
        }

        public static EmailMetaData GetEmailMetaData() =>
            new EmailMetaData()
            {
                SmtpServer = "email-smtp.us-east-1.amazonaws.com",
                Port = 465,
                UserName = "AKIAVPGOYLGBWICBZ4GW",
                Password = "BMvhz4xMUuhlghfzkG1ooc66eqDqcB7aKCgiDvE1PogP",
                FromEmail = "sudokucollective-testing@sudokucollective.com"
            };

        public static EmailMetaData GetIncorrectEmailMetaData() =>
            new EmailMetaData()
            {
                SmtpServer = "smtp.mail.yahoo.com",
                Port = 465,
                UserName = "sudokucollectivetesting@yahoo.com",
                Password = "P@ssw0rd1",
                FromEmail = "SudokuCollectivetesting@yahoo.com"
            };

        public static User GetNewUser() =>
            new User
            {
                FirstName = "John",
                LastName = "Doe",
                UserName = "Johnny",
                Email = "john.doe@example.com",
                Password = "P@s5w1",
                DateCreated = DateTime.UtcNow
            };

        public static EmailConfirmation GetNewEmailConfirmation() =>
            new EmailConfirmation
            {
                UserId = 1,
                AppId = 1,
                Token = "cc924471-aab6-4809-8e6d-723ae422cb33"
            };

        public static EmailConfirmation GetUpdateEmailConfirmation() =>
            new EmailConfirmation
            {
                UserId = 1,
                AppId = 1,
                Token = "cc924471-aab6-4809-8e6d-723ae422cb33",
                OldEmailAddress = "TestSuperUser@example.com",
                NewEmailAddress = "UPDATEDTestSuperUser@example.com"
            };

        public static Request GetRequest() =>
            new Request()
            {
                License = GetLicense(),
                RequestorId = 1,
                AppId = 1,
                Paginator = GetPaginator(),
                Payload = new JsonElement()
            };

        public static CreateGamePayload GetCreateGamePayload() =>
            new CreateGamePayload()
            {
                DifficultyId = 4
            };

        public static GamePayload GetGamePayload(int updatedValue) =>
            new GamePayload()
            {
                SudokuCells = GetUpdateSudokuCells(updatedValue)
            };

        public static GamePayload GetInvalidGamePayload(int updatedValue) =>
            new GamePayload()
            {
                SudokuCells = GetUpdateInvalidSudokuCells(updatedValue)
            };

        public static GamePayload GetSolvedGamePayload() =>
            new GamePayload()
            {
                SudokuCells = GetValidSudokuCells()
            };

        public static GamePayload GetGameNotFoundGamePayload() =>
            new GamePayload()
            {
                SudokuCells = GetValidSudokuCells()
            };

        public static GamesPayload GetGamesPayload() =>
            new GamesPayload()
            {
                UserId = 1,
            };

        public static UpdateRolePayload GetUpdateRolePayload() =>
            new UpdateRolePayload()
            {
                Id = 1,
                Name = "Null UPDATED!"
            };

        public static UpdateRolePayload GetInvalidUpdateRolePayload() =>
            new UpdateRolePayload()
            {
                Id = 10,
                Name = "Null UPDATED!"
            };

        public static CreateRolePayload GetCreateRolePayload() => 
            new CreateRolePayload()
            {
                Name = "Test Role",
                RoleLevel = RoleLevel.NULL
            };

        public static Result GetResult() =>
            new Result()
            {
                IsSuccess = true,
                IsFromCache = true,
            };

        public static ConfirmEmailResult GetConfirmEmailResult() =>
            new ConfirmEmailResult()
            {
                UserName = "TestSuperUser",
                Email = "TestSuperUser@example.com",
                DateUpdated = DateTime.Now,
                AppTitle = "Test App 1",
                AppUrl = "https://localhost:4200",
                IsUpdate = true,
                NewEmailAddressConfirmed = false,
                ConfirmationEmailSuccessfullySent = true
            };

        public static InitiatePasswordResetResult GetInitiatePasswordResetResult() =>
            new InitiatePasswordResetResult()
            {
                App = new App(),
                User = new User(),
                ConfirmationEmailSuccessfullySent = true,
                Token = GetToken()
            };

        public static UserResult GetUserResult() =>
            new UserResult()
            {
                User = new User(),
                ConfirmationEmailSuccessfullySent = true,
                Token = GetToken()
            };

        public static Mock<IHttpContextAccessor> GetHttpContextAccessor(User user, App app)
        {
            var result = new Mock<IHttpContextAccessor>();

            var claim = new List<Claim> {

                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.Name, app.Id.ToString()),
            };
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("865542af-e02f-446d-ad34-b121554f37be"));
            
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expirationLimit = DateTime.UtcNow.AddDays(1);

            var jwtToken = new JwtSecurityToken(
                    "test",
                    "test",
                    claim.ToArray(),
                    notBefore: DateTime.UtcNow,
                    expires: expirationLimit,
                    signingCredentials: credentials
                );

            result.Setup(
                mock => mock.HttpContext.Request.Headers["Authorization"])
                .Returns(string.Format("bearer {0}", new JwtSecurityTokenHandler().WriteToken(jwtToken)));

            var request = GetRequest();

            string json;

            try 
            {
                json = JsonSerializer.Serialize<SudokuCollective.Logs.Models.Request>((SudokuCollective.Logs.Models.Request)request);
            }
            catch
            {
                throw;
            }

            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
            MemoryStream stream = new MemoryStream(byteArray);

            result.Setup(
                mock => mock.HttpContext.Request.Body)
                .Returns(stream);

            return result;
        }

        public static Mock<IHttpContextAccessor> GetInvalidHttpContextAccessor(User user)
        {
            var result = new Mock<IHttpContextAccessor>();

            var appId = 1;

            var claim = new List<Claim> {

                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.Name, appId.ToString()),
            };
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("865542af-e02f-446d-ad34-b121554f37be"));
            
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expirationLimit = DateTime.UtcNow.AddDays(1);

            var jwtInvalidToken = new JwtSecurityToken(
                    "test",
                    "test",
                    claim.ToArray(),
                    notBefore: DateTime.UtcNow,
                    expires: expirationLimit,
                    signingCredentials: credentials
                );

            result.Setup(
                mock => mock.HttpContext.Request.Headers["Authorization"])
                .Returns(string.Format("bearer {0}", new JwtSecurityTokenHandler().WriteToken(jwtInvalidToken)));

            return result;
        }

        public static SudokuCollective.Api.Models.PasswordReset GetPasswordReset() =>
            new SudokuCollective.Api.Models.PasswordReset
            {
                IsSuccess = true,
                UserId = 1,
                UserName = "UserName",
                NewPassword = "N3wP@ssw0rd!",
                ConfirmNewPassword = "N3wP@ssw0rd!",
                AppTitle = "AppTItle",
                AppId = 1,
                AppUrl = "https://localhost:5001",
                ErrorMessage = "Message"
            };

        public static AppPayload GetAppPayload() =>
            new AppPayload
            {
                Name = "Test App 1 UPDATED!",
                LocalUrl = "https://localhost:4200",
                StagingUrl = "https://testapp.dev.com",
                QaUrl = "https://testapp.qa.com",
                ProdUrl = "https://testapp.com",
                IsActive = true,
                Environment = ReleaseEnvironment.LOCAL,
                PermitSuperUserAccess = true,
                PermitCollectiveLogins = true,
                DisableCustomUrls = true,
                CustomEmailConfirmationAction = string.Empty,
                CustomPasswordResetAction = string.Empty,
                TimeFrame = TimeFrame.DAYS,
                AccessDuration = 7
            };

        public static AppPayload GetInvalidAppPayload() =>
            new AppPayload
            {
                Name = string.Empty,
                LocalUrl = "https://localhost:4200",
                StagingUrl = "https://testapp.dev.com",
                QaUrl = "https://testapp.qa.com",
                ProdUrl = "https://testapp.com",
                IsActive = true,
                Environment = ReleaseEnvironment.LOCAL,
                PermitSuperUserAccess = true,
                PermitCollectiveLogins = true,
                DisableCustomUrls = true,
                CustomEmailConfirmationAction = string.Empty,
                CustomPasswordResetAction = string.Empty,
                TimeFrame = TimeFrame.DAYS,
                AccessDuration = 7
            };

        public static LicensePayload GetLicensePayload() =>
            new LicensePayload
            {
                Name = "Test App 4",
                LocalUrl = "https://localhost:4200",
                StagingUrl = "https://testapp3.dev.com",
                QaUrl = "https://testapp3.qa.com",
                ProdUrl = "https://testapp3.com"
            };

        public static Values GetValues()
        {
            var context = TestDatabase.GetDatabaseContext().Result;
            var releaseEnvironment = new List<string> { "releaseEnvironment" };
            var all = new List<string> { "apps", "users", "games" };
            var apps = new List<string> { "apps" };
            var users = new List<string> { "users" };
            var games = new List<string> { "games" };
            var authToken = new List<string> { "authToken" };
            
            return new Values
            {
                Difficulties = context.Difficulties.ToList(),
                ReleaseEnvironments = new List<EnumListItem>
                    {
                        new EnumListItem { 
                            Label = "Local", 
                            Value = (int)ReleaseEnvironment.LOCAL,
                            AppliesTo = releaseEnvironment },
                        new EnumListItem { 
                            Label = "Staging", 
                            Value = (int)ReleaseEnvironment.STAGING,
                            AppliesTo = releaseEnvironment },
                        new EnumListItem { 
                            Label = "Quality Assurance", 
                            Value = (int)ReleaseEnvironment.QA,
                            AppliesTo = releaseEnvironment },
                        new EnumListItem { 
                            Label = "Production", 
                            Value = (int)ReleaseEnvironment.PROD,
                            AppliesTo = releaseEnvironment },
                    },
                SortValues = new List<EnumListItem>
                    {
                        new EnumListItem { 
                            Label = "Id", 
                            Value = (int)SortValue.ID,
                            AppliesTo = all },
                        new EnumListItem { 
                            Label = "Username", 
                            Value = (int)SortValue.USERNAME,
                            AppliesTo = users },
                        new EnumListItem { 
                            Label = "First Name", 
                            Value = (int)SortValue.FIRSTNAME,
                            AppliesTo = users },
                        new EnumListItem { 
                            Label = "Last Name", 
                            Value = (int)SortValue.LASTNAME,
                            AppliesTo = users },
                        new EnumListItem { 
                            Label = "Full Name", 
                            Value = (int)SortValue.FULLNAME,
                            AppliesTo = users },
                        new EnumListItem { 
                            Label = "Nick Name", 
                            Value = (int)SortValue.NICKNAME,
                            AppliesTo = users },
                        new EnumListItem { 
                            Label = "Game Count", 
                            Value = (int)SortValue.GAMECOUNT,
                            AppliesTo = users },
                        new EnumListItem { 
                            Label = "App Count", 
                            Value = (int)SortValue.APPCOUNT,
                            AppliesTo = users },
                        new EnumListItem { 
                            Label = "Name", 
                            Value = (int)SortValue.NAME,
                            AppliesTo = apps },
                        new EnumListItem { 
                            Label = "Date Created", 
                            Value = (int)SortValue.DATECREATED,
                            AppliesTo = all },
                        new EnumListItem { 
                            Label = "Date Updated", 
                            Value = (int)SortValue.DATEUPDATED,
                            AppliesTo = all },
                        new EnumListItem { 
                            Label = "Difficulty Level", 
                            Value = (int)SortValue.DIFFICULTYLEVEL,
                            AppliesTo = games },
                        new EnumListItem {
                            Label = "User Count",
                            Value = (int)SortValue.USERCOUNT,
                            AppliesTo = apps },
                        new EnumListItem { 
                            Label = "Score", 
                            Value = (int)SortValue.SCORE,
                            AppliesTo = games }
                    },
                TimeFrames = new List<EnumListItem>
                    {
                        new EnumListItem { 
                            Label = "Seconds", 
                            Value = (int)TimeFrame.SECONDS,
                            AppliesTo = authToken },
                        new EnumListItem { 
                            Label = "Minutes", 
                            Value = (int)TimeFrame.MINUTES,
                            AppliesTo = authToken },
                        new EnumListItem { 
                            Label = "Hours", 
                            Value = (int)TimeFrame.HOURS,
                            AppliesTo = authToken },
                        new EnumListItem { 
                            Label = "Days", 
                            Value = (int)TimeFrame.DAYS,
                            AppliesTo = authToken },
                        new EnumListItem { 
                            Label = "Months", 
                            Value = (int)TimeFrame.MONTHS,
                            AppliesTo = authToken },
                        new EnumListItem {
                            Label = "Years",
                            Value = (int)TimeFrame.YEARS,
                            AppliesTo = authToken },
                    }
            };
        }
    }
}
