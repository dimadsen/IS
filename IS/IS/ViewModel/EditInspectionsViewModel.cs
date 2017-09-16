using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Data.Entity;
using System.Windows;
using System.Text.RegularExpressions;

namespace IS
{
    public class EditInspectionsViewModel : ViewModelBase, IPageViewModel
    {
        #region Конструктор и контекст подключения к бд

        ISContext db = new ISContext();
        public EditInspectionsViewModel()
        {
            InspectionsCollection = new ListCollectionView(Inspections);

            RemarksCollection = new ListCollectionView(Remarks);
        }
        #endregion

        #region Коллекция инспекций ListCollectionView и ObservableCollection

        ListCollectionView inspectionsCollection;
        public ListCollectionView InspectionsCollection
        {
            get
            {
                return inspectionsCollection;
            }
            set
            {
                inspectionsCollection = value;
            }
        }

        ObservableCollection<Inspection> inspections;
        public ObservableCollection<Inspection> Inspections
        {
            get
            {

                if (inspections == null)
                {

                    List<Inspection> insp = (from i in db.Inspections
                                .Include(i => i.Inspector)
                                .Include(r => r.Remarks)
                                             select i).ToList();
                    //С помощью цикла foreach происходит присвоение  значения FullName каждому элементу
                    //Это свойство для вывода имени в нужном формате на главной странице. Оно не заносится в базу.

                    foreach ( Inspection i in insp)
                    {
                        Inspection fullname = (from full in insp
                                              where full.Id == i.Id
                                              select full).FirstOrDefault();

                        i.Inspector.FullName = string.Format("{0} {1} {2} ({3})", fullname.Inspector.LastName,
                            fullname.Inspector.FirstName, fullname.Inspector.MiddleName, fullname.Inspector.Number);
                    }

                    inspections = new ObservableCollection<Inspection>(insp);

                }
                return inspections;
            }
            set { inspections = value; }
        }

        ObservableCollection<Inspector> inspectors;
        public ObservableCollection<Inspector> Inspectors
        {
            get
            {

                if (inspectors == null)
                {

                    List<Inspector> insp = (from i in db.Inspectors
                                             .Include(i=>i.Inspections)   
                                             select i).ToList();
                    //С помощью цикла foreach происходит присвоение  значения FullName каждому элементу
                    //Это свойство для вывода имени в нужном формате на главной странице. Оно не заносится в базу.

                    foreach (Inspector i in insp)
                    {
                        Inspector fullname = (from full in insp
                                               where full.Id == i.Id
                                               select full).FirstOrDefault();

                        i.FullName = string.Format("{0} {1}.{2}. ({3})", fullname.LastName,
                            fullname.FirstName[0], fullname.MiddleName[0], fullname.Number);
                    }

                    inspectors = new ObservableCollection<Inspector>(insp);

                }
                return inspectors;
            }
            set { inspectors = value; }
        }
        #endregion

        #region Коллекция замечаний (ListCollectionView и ObservableCollection )

        ListCollectionView remarksCollection;
        public ListCollectionView RemarksCollection
        {
            get
            {
                return remarksCollection;
            }
            set
            {
                remarksCollection = value;
            }
        }

        ObservableCollection<Remark> remarks;
        public ObservableCollection<Remark> Remarks
        {
            get
            {
                if (remarks == null)
                {
                    List<Remark> rem = (from r in db.Remarks
                               .Include(r => r.Inspection)
                                        select r).ToList();

                    remarks = new ObservableCollection<Remark>(rem);
                }
                return remarks;
            }
        }
        #endregion

        #region Команды
        /// <summary>
        /// Поскольку работа с паттерном MVVM, используются команды, а не обработчики событий.
        /// Команда хранится в свойстве и представляет собой объект класса Command. 
        /// Этот объект в конструкторе принимает действие - делегат Action<object>. 
        /// </summary>
        /// 
        
        //Удаление инспекции
        Command removeCommand;
        public Command RemoveCommand
        {
            get
            {
                return removeCommand ??
                    (removeCommand = new Command(obj =>
                    {
                        Inspection inspection = obj as Inspection;
                        if (inspection != null)
                        {
                            //Подгружается, чтобы удалились все замечания инспекции
                            db.Entry(inspection).Collection(i => i.Remarks).Load();
                            db.Inspections.Remove(inspection);
                            db.SaveChanges();

                            //Удаляется из коллекции, которая отображается странице
                            Inspections.Remove(inspection);

                        }

                    },
                      //Если количество инспекций будет меньше 1, то клавиша даективируется
                      (obj) => Inspections.Count > 0));
            }
        }
        // команда редактирования инспекции
        Command editCommand;
        public Command EditCommand
        {
            get
            {
                return editCommand ??
                //В качестве параметра в команду будет передаваться редактируемый объект
                (editCommand = new Command(obj =>
                {

                    Inspection inspection = obj as Inspection;// приведение к нужному типу
                    if (inspection != null)
                    {
                        Inspection ins = (from i in db.Inspections
                                         where i.Id==inspection.Id
                                         select i).FirstOrDefault();
                        
                            db.SaveChanges();
                            MessageBox.Show("Изменения сохранены");
                        
                    }
                },
                (obj) => Inspections.Count > 0));//Дективация Control'a, если коллекция пустая
            }
        }

        // команда редактирования замечаний
        Command editRemarkCommand;
        public Command EditRemarkCommand
        {
            get
            {
                return editRemarkCommand ??
                //В качестве параметра в команду будет передаваться редактируемый объект
                (editRemarkCommand = new Command(obj =>
                {

                    Remark remark = obj as Remark;// приведение к нужному типу
                    if (remark != null)
                    {
                        Remark ins = (from i in db.Remarks
                                          where i.Id == remark.Id
                                          select i).FirstOrDefault();

                        db.SaveChanges();
                        MessageBox.Show("Изменения сохранены");
                    }
                },
                (obj) => Remarks.Count > 0));//Дективация Control'a, если коллекция пустая
            }
        }

        //команда удаления замечания из списка
        Command removeRemarkCommand;
        public Command RemoveRemarkCommand
        {
            //В качестве параметра в команду будет передаваться удаляемый объект
            get
            {
                return removeRemarkCommand ??
                (removeRemarkCommand = new Command(obj =>
                {
                    Remark remark = obj as Remark; // приведение к нужному типу

                    if (remark != null)
                    {
                        Remark rem = (from r in db.Remarks
                                      where r.Id == remark.Id
                                      select r).FirstOrDefault();
                        db.Remarks.Remove(rem);
                        Remarks.Remove(remark);
                        db.SaveChanges();
                    }
                },

                (obj) => Remarks.Count > 0)); //дектививация кнопки при отсутствии замечаний
            }
        }
        //фильтрация по выбору двойному клику item
        Command filterCommand;
        public Command FilterCommand
        {
            get
            {
                return filterCommand ??
                    (filterCommand = new Command(obj =>
                    {
                        Inspection inspection = obj as Inspection;
                        RemarksCollection.Filter = new Predicate<object>(o => ((Remark)o).InspectionId == inspection.Id);
                    }
                    ));
            }

        }
        #endregion

        #region Выбор инспектора и замечния в Listbox
        Inspection selectedInspection;
        public Inspection SelectedInspection
        {
            get
            {
                return selectedInspection;
            }
            set
            {
                selectedInspection = value;
                OnPropertyChanged("SelectedInspection");
            }
        }
        Remark selectedRemark;
        public Remark SelectedRemark
        {
            get { return selectedRemark; }
            set
            {
                selectedRemark = value;
                OnPropertyChanged("SelectedRemark");
            }
        }
        Inspection selectedInspector;
        public Inspection SelectedInspector
        {
            get { return selectedInspector; }
            set
            {
                selectedInspector = value;
                OnPropertyChanged("SelectedInspector");
            }
        }
        #endregion

        #region Свойство для DatePiсker

        /// <summary>
        /// Свойство для установки даты по умолчанию в DatePiсker
        /// </summary>
        DateTime? inspectionEditDate;
        public DateTime? InspectionEditDate
        {
            get
            {
                if (inspectionEditDate == null)
                {
                    inspectionEditDate = DateTime.Today;
                }
                return inspectionEditDate;
            }
            set
            {
                inspectionEditDate = value;
                OnPropertyChanged("InspectionEditDate");
            }
        }
        DateTime? remarkEditDate;
        public DateTime? RemarkEditDate
        {
            get
            {
                if (remarkEditDate == null)
                {
                    remarkEditDate = DateTime.Today;
                }
                return remarkEditDate;
            }
            set
            {
                remarkEditDate = value;
                OnPropertyChanged("RemarkEditDate");
            }
        }
        #endregion

        #region Название кнопки переключения страниц
        public string PageName
        {
            get
            {
                return "Редактирование инспекций";
            }
        }
        #endregion

        #region Поиск по фамилии

        string lastNameSearch;
        public string LastNameSearch
        {
            get { return lastNameSearch; }
            set
            {
                lastNameSearch = value;
                OnPropertyChanged("LastNameSearch");

                if (string.IsNullOrEmpty(value))
                    InspectionsCollection.Filter = null;
                else
                    InspectionsCollection.Filter = new Predicate<object>(o => ((Inspection)o).Inspector.LastName.ToUpper().Contains(LastNameSearch.ToUpper()));

            }
        }

        string inspectionSearch;
        public string InspectionSearch
        {
            get { return inspectionSearch; }
            set
            {
                inspectionSearch = value;
                OnPropertyChanged("InspectionSearch");

                if (string.IsNullOrEmpty(value))
                    InspectionsCollection.Filter = null;
                else
                    InspectionsCollection.Filter = new Predicate<object>(o => ((Inspection)o).Name.ToUpper().Contains(InspectionSearch.ToUpper()));

            }
        }
        #endregion

        #region Валидация

        Inspection editInspection;
        public Inspection EditInspection
        {
            get
            {
                if (editInspection == null)
                    editInspection = new Inspection();
                return editInspection;
            }
            set
            {
                editInspection = value;
                OnPropertyChanged("EditInspection");
            }
        }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            EditInspection.Name = (string)value;

            //Проверяется поле на соответствие нужных допустимых символов 
            if (!Regex.IsMatch(EditInspection.Name, @"^[а-яА-ЯёЁa-zA-Z\s]+$"))
            {

                return new ValidationResult(false, "Поле может включать только буквы.");

            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
        #endregion
    }
}
