using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Data.Entity;
using System.Windows.Controls;
using System.Collections.Generic;

namespace IS
{
    public class ShellViewModel : ViewModelBase, IPageViewModel
    {

        #region Конструктор и контекст подключения к бд

        ISContext db = new ISContext();
        public ShellViewModel()
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

        ObservableCollection<Inspection> inspection;
        public ObservableCollection<Inspection> Inspections
        {
            get
            {

                if (inspection == null)
                {

                    List<Inspection> insp = (from i in db.Inspections
                                .Include(i => i.Inspector)
                                .Include(r => r.Remarks)
                                select i).ToList();

                    inspection = new ObservableCollection<Inspection>(insp);

                }
                return inspection;
            }
            set { inspection = value; }
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

        /// <summary>
        /// Команда для фильтрации замечаний. Используется, когда происходит двойной клик по item,
        /// где отображается коллекция инспекций.
        /// </summary>
        /// 
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

        #region Фильтр Control(Combobox)
        /// <summary>
        /// Для того, чтобы происходил фильтр в Сontrol, сначала создаётся коллекция инспекторов,
        /// затем через свойство происходит фильтрация
        /// </summary>

        //выгрузка в combobox
        ObservableCollection<Inspector> inspector;
        public ObservableCollection<Inspector> Inspectors
        {
            get
            {

                if (inspector == null)
                {
                    //Создайтся коллекцию инспекторов
                    List<Inspector> insp = (from i in db.Inspectors
                                            .Include(i => i.Inspections)
                                            select i).ToList();


                    //С помощью цикла foreach происходит присвоение  значения FullNam каждому элементу
                    //Это свойство для вывода имени в нужном формате на главной странице. Оно не заносится в базу.
                    foreach (Inspector i in insp)
                    {
                        Inspector fullname = (from full in insp
                                  where full.Id == i.Id
                                  select full).FirstOrDefault();

                        i.FullName = string.Format("{0} {1}.{2}. ({3})", fullname.LastName,
                            fullname.FirstName[0], fullname.MiddleName[0], fullname.Number);
                    }

                    //Создётся строка "Все" и добавляется  в коллекцию в первую позицию. Она нужна для вывода всех инспекторов.

                    Inspector insAll = new Inspector { LastName = "Все", FullName = "Все" };

                    insp.Insert(0, insAll);

                    inspector = new ObservableCollection<Inspector>(insp);

                }
                return inspector;
            }
        }

        Inspector selectedInspector;

        public Inspector SelectedInspector
        {
            get { return selectedInspector; }
            set
            {
                selectedInspector = value;

                OnPropertyChanged("SelectedInspector");


                if (selectedInspector.LastName == "Все") //вывод всех инспекторов
                {
                    InspectionsCollection.Filter = new Predicate<object>(o => ((Inspection)o).Inspector.LastName.Any());
                }
                else // Вывод выбранного инспектора
                {
                    InspectionsCollection.Filter = new Predicate<object>(o => ((Inspection)o).Inspector.LastName == selectedInspector.LastName);
                };
            }
        }
        

        #endregion

        #region Поиск по названию

        string textSearch;
        public string TextSearch
        {
            get { return textSearch; }
            set
            {
                textSearch = value;
                OnPropertyChanged("TextSearch");

                if (string.IsNullOrEmpty(value))
                    InspectionsCollection.Filter = null;
                else
                    InspectionsCollection.Filter = new Predicate<object>(o => ((Inspection)o).Name.ToUpper().Contains(TextSearch.ToUpper()));

            }
        }
        #endregion

        #region Название кнопки переключения страницы
        public string PageName
        {
            get
            {
                return "Главное окно";
            }
        }
        #endregion

        #region Валидация (в этом окне не используется)
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {

            throw new NotImplementedException();
        }
    }
    #endregion
   
}
