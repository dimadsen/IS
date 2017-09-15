namespace IS
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;

    public class ISContext : DbContext
    {
        static ISContext()
        {
            Database.SetInitializer(new MyContextInitializer());
        }
        public ISContext()
            : base("name=ISContext")
        {
        }

        public virtual DbSet<Inspection> Inspections { get; set; }
        public virtual DbSet<Inspector> Inspectors { get; set; }
        public virtual DbSet<Remark> Remarks { get; set; }
    }
    class MyContextInitializer : DropCreateDatabaseAlways<ISContext>
    {
        protected override void Seed(ISContext db)
        {
            Inspector inspector = new Inspector
            {
                FirstName = "Тест",
                LastName = "Тестов",
                MiddleName = "Tестович",
                Number = 123456,
                Id = 1
            };
            Inspector inspector2 = new Inspector
            {
                FirstName = "Пример",
                LastName = "Примеров",
                MiddleName = "Примерович",
                Number = 456789,
                Id = 2

            };
            db.Inspectors.AddRange(new List<Inspector> { inspector, inspector2 });

            Inspection inspection = new Inspection
            {
                Name = "Основная инспекция",
                DateOfInspection = DateTime.Parse("20/10/2013"),
                Comment = null,
                InspectorId = inspector.Id,
                Id = 1
            };
            Inspection inspection2 = new Inspection
            {
                Name = "Пожарная инспекция",
                DateOfInspection = DateTime.Parse("20/05/2013"),
                Comment = "Инспекция проведена с опозданием",
                InspectorId = inspector.Id,
                Id = 2
            };
            Inspection inspection3 = new Inspection
            {
                Name = "Пожарная инспекция",
                DateOfInspection = DateTime.Parse("10/12/2013"),
                Comment = null,
                InspectorId = inspector2.Id,
                Id = 3
            };

            db.Inspections.AddRange(new List<Inspection> { inspection, inspection2, inspection3 });

            Remark remark = new Remark
            {
                Comment = "...текст замечания Основной инспекции...",
                DateOfElimination = DateTime.Parse("21/10/2013"),
                TextOfComments = "...комментарий к этому замечанию...",
                InspectionId = inspection.Id,
                Id = 1

            };

            Remark remark2 = new Remark
            {
                Comment = "...текст второго замечания Основной инспекции...",
                DateOfElimination = DateTime.Parse("22/10/2013"),
                TextOfComments = "...комментарий ко второму замечанию...",
                InspectionId = inspection.Id,
                Id = 2
            };
            Remark remark3 = new Remark
            {
                Comment = "Не соответствие регламенту ISO-09",
                DateOfElimination = DateTime.Parse("05/05/2013"),
                TextOfComments = "...комментарий к замечанию...",
                InspectionId = inspection2.Id,
                Id = 3
            };
            Remark remark4 = new Remark
            {
                Comment = "Не соответствие регламенту ISO-0978",
                DateOfElimination = DateTime.Parse("22/05/2013"),
                TextOfComments = "...комментарий к замечанию...",
                InspectionId = inspection2.Id,
                Id = 4
            };

            Remark remark5 = new Remark
            {
                Comment = "...текст замечания выбранной инспекции...",
                DateOfElimination = DateTime.Parse("08/07/2017"),
                TextOfComments = "...комментарий к этому замечанию...",
                InspectionId = inspection3.Id,
                Id = 5

            };

            db.Remarks.AddRange(new List<Remark> { remark, remark2, remark3, remark4, remark5 });
            db.SaveChanges();

        }
    }

}