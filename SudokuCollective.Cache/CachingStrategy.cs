using System;
using SudokuCollective.Core.Interfaces.Cache;

namespace SudokuCollective.Cache
{
    public class CachingStrategy : ICachingStrategy
    {
        public DateTime Light 
        { 
            get
            {
                return DateTime.Now.AddMinutes(15);
            } 
        }
        public DateTime Medium
        {
            get
            {
                return DateTime.Now.AddHours(1);
            }
        }
        public DateTime Heavy
        {
            get
            {
                return DateTime.Now.AddDays(1);
            }
        }
    }
}
