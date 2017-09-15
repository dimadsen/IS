using System;
using System.Collections.Generic;

namespace IS
{
    public class Inspection
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DateOfInspection { get; set; }
        public string Comment { get; set; }
        public int InspectorId { get; set; }
        public Inspector Inspector { get; set; }
        public ICollection<Remark> Remarks { get; set; }
        public Inspection()
        {
            Remarks = new List<Remark>();
        }
    }
}
