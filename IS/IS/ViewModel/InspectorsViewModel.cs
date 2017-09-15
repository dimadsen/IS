using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace IS
{
    public class InspectorsViewModel : ViewModelBase, IPageViewModel
    {

        #region Конструктор и контекст подключения к бд
        ISContext db = new ISContext();
        public InspectorsViewModel()
        {
            inspectorsCollection = new ListCollectionView(Inspectors);
        }
        #endregion

        #region Выбор инспектора в ListView

        Inspector selectedInspector;
        public Inspector SelectedInspector
        {
            get { return selectedInspector; }
            set
            {
                selectedInspector = value;
                OnPropertyChanged("SelectedInspector");
            }
        }
        #endregion

        #region Переменная для добавления инспектора 

        Inspector newInspector;
        public Inspector NewInspector
        {
            get
            {
                if (newInspector == null)
                    newInspector = new Inspector();
                return newInspector;
            }
            set
            {
                newInspector = value;
                OnPropertyChanged("NewInspector");
            }
        }
        #endregion

        #region Коллекция испекторов ListCollectionView и ObservableCollection

        ObservableCollection<Inspector> inspectors;
        public ObservableCollection<Inspector> Inspectors
        {
            get
            {
                if (inspectors == null)
                {
                    List<Inspector> ins = (from i in db.Inspectors
                               select i).ToList();

                    inspectors = new ObservableCollection<Inspector>(ins);
                }
                return inspectors;
            }
        }
        
        ListCollectionView inspectorsCollection;
        public ListCollectionView InspectorsCollection
        {
            get { return inspectorsCollection; }
        }
        #endregion

        #region Команды

        /// <summary>
        /// Поскольку работа с паттерном MVVM, используются команды, а не обработчики событий.
        /// Команда хранится в свойстве и представляет собой объект класса Command. 
        /// Этот объект в конструкторе принимает действие - делегат Action<object>. 
        /// 
        /// В этом классе определены несколько команд. Добавление, удаление и редактирование
        /// </summary>
        /// 
        Command addCommand;
        public Command AddCommand
        {
            get
            {

                if (addCommand == null)
                    //экземпляру класса Command передаётся два метода. Один из них производит добавление,
                    //а второй проверяет, может ли команда выполнится. Если нет,то кнопка будет декативированна.
                    addCommand = new Command(ExecuteAddCommand, CanExecuteCommand);
                return addCommand;
            }
        }
        
        public void ExecuteAddCommand(object parameter)
        {
           
            Inspector ins = (from i in db.Inspectors
                       where i.Number == NewInspector.Number
                       select i).FirstOrDefault();

            if (ins != null)
            {
                MessageBox.Show("Такой номер уже существует. Инспектор не добавлен");
            }
            else
            {

                db.Inspectors.Add(NewInspector);
                db.SaveChanges();

                MessageBox.Show("Инспектор добавлен");
                Inspectors.Add(NewInspector);
                NewInspector = null;
            }

        }
        public bool CanExecuteCommand(object parameter)
        {
            //Проверка на пустое поле или поле с пробелами. Если что-то не заполнено, control добавления деактивируется.
            if (string.IsNullOrEmpty(NewInspector.FirstName) ||
            string.IsNullOrEmpty(NewInspector.LastName) ||
            string.IsNullOrEmpty(NewInspector.MiddleName) ||
            string.IsNullOrWhiteSpace(NewInspector.FirstName) ||
            string.IsNullOrWhiteSpace(NewInspector.LastName) ||
            string.IsNullOrWhiteSpace(NewInspector.MiddleName)||
            NewInspector.Number==0)
            {
                return false;
            }
            
            return true;

        }

        // команда редактирования
        Command editCommand;
        public Command EditCommand
        {
            get
            {
                return editCommand ??
                //В качестве параметра в команду будет передаваться редактируемый объект
                (editCommand = new Command(obj =>
                {

                    Inspector inspector = obj as Inspector;// приведение к нужному типу
                    if (inspector != null)
                    {
                        Inspector ins = (from i in db.Inspectors
                                         where i.Number == inspector.Number
                                         select i).FirstOrDefault();
                        if (ins == null)
                        {
                            db.SaveChanges();
                            MessageBox.Show("Изменения сохранены");
                        }
                        else
                        {
                            MessageBox.Show("Такой номер уже существует. Изменения не сохранены");
                        }
                        
                    }
                },
                (obj) => Inspectors.Count > 0));//Дективация Control'a, если коллекция пустая
            }
        }

        //удаление 
        Command removeCommand;
        public Command RemoveCommand
        {
            get
            {
                return removeCommand ??
                //В качестве параметра в команду будет передаваться удаляемый объект
                (removeCommand = new Command(obj =>
                {
                    Inspector inspector = obj as Inspector;
                    if (inspector != null)
                    {
                        //Подгружаются инспекции. При удалении инспектора они удаляются
                        db.Entry(inspector).Collection(i => i.Inspections).Load();
                        db.Inspectors.Remove(inspector);

                        db.SaveChanges();
                        Inspectors.Remove(inspector);
                        MessageBox.Show("Инспектор удалён");
                    }
                },
                (obj) => Inspectors.Count > 0));//деактивация кнопки
            }
        }

        #endregion

        #region Название кнопки переключения страницы 
        public string PageName
        {
            get
            {
                return "Справочник Инспекторы";
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
                    InspectorsCollection.Filter = null;
                else
                    InspectorsCollection.Filter = new Predicate<object>(o => ((Inspector)o).LastName.ToUpper().Contains(LastNameSearch.ToUpper()));

            }
        }
        #endregion

        #region Поиск по номеру

        string numberSearch;
        public string NumberSearch
        {
            get { return numberSearch; }
            set
            {
                numberSearch = value;
                OnPropertyChanged("NumberSearch");

                if (string.IsNullOrEmpty(value))
                    InspectorsCollection.Filter = null;
                else
                    InspectorsCollection.Filter = new Predicate<object>(o => ((Inspector)o).Number.ToString().Contains(NumberSearch));

            }
        }
        #endregion

        #region Валидация
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            //Символ, который поступает в Control присваивается полю класса Inspector
            NewInspector.FirstName = (string)value;
            NewInspector.LastName = (string)value;
            NewInspector.MiddleName = (string)value;

            //Если поля Control не заполнены, то возвращается true
            if (string.IsNullOrWhiteSpace(NewInspector.FirstName) ||
            string.IsNullOrWhiteSpace(NewInspector.LastName) ||
            string.IsNullOrWhiteSpace(NewInspector.MiddleName))
            {
                return new ValidationResult(true, null);
            }

            //Если поля содержат недопустивые символы, то возвращается false
            if (!Regex.IsMatch(NewInspector.FirstName, @"^[а-яА-ЯёЁa-zA-Z]+$") ||
            !Regex.IsMatch(NewInspector.LastName, @"^[а-яА-ЯёЁa-zA-Z]+$") ||
            !Regex.IsMatch(NewInspector.MiddleName, @"^[а-яА-ЯёЁa-zA-Z]+$"))
            {
                return new ValidationResult(false, "Поле может включать только буквы");
            }

            else
            {
                return new ValidationResult(true, null);
            }
        }
        #endregion
    }
}
