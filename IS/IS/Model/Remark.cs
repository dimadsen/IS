using System;

namespace IS
{
    public class Remark
    {
        public int Id { get; set; }
        public string TextOfComments { get; set; }
        public DateTime DateOfElimination { get; set; }
        public string Comment { get; set; }
        public int InspectionId { get; set; }
        public Inspection Inspection { get; set; }
    }
}
