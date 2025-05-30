using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judgements
{
    public class ParticipantResultViewModel
    {
        public int RowNumber { get; set; }
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public float? AverageScore { get; set; }
        public int Place { get; set; }
    }
}
