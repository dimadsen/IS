using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;


namespace IS
{
    public class Inspector
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public int Number { get; set; }

        [NotMapped]
        public string FullName { get; set; }
        public ICollection<Inspection> Inspections { get; set; }
        public Inspector()
        {
            Inspections = new List<Inspection>();
        }
    }
}
