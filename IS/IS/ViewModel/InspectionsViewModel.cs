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
    public class InspectionsViewModel : ViewModelBase, IPageViewModel
    {
        #region Конструктор и контекст подключения к бд

        ISContext db = new ISContext();
        public InspectionsViewModel()
        {
            remarksCollection = new ListCollectionView(Remarks);
        }
        #endregion

        #region Выбор инспектора в ListView
        Inspector _selectedInspector;
        public Inspector SelectedInspector
        {
            get { return _selectedInspector; }
            set
            {
                _selectedInspector = value;
                OnPropertyChanged("SelectedInspector");
            }
        }
        #endregion

        #region Коллекция испекторов

        ObservableCollection<Inspector> inspectors;
        public ObservableCollection<Inspector> Inspectors
        {
            get
            {
                if (inspectors == null)
                {
                    List<Inspector> ins = (from i in db.Inspectors
                               select i).ToList();

                    //С помощью цикла foreach происходит присвоение  значения FullNam каждому элементу
                    //Это свойство для вывода имени в нужном формате на главной странице. Оно не заносится в базу. 
                    foreach (Inspector i in ins)
                    {
                        Inspector fullname = (from full in ins
                                  where full.Id == i.Id
                                  select full).FirstOrDefault();

                        i.FullName = string.Format("{0} {1}.{2}. ({3})", fullname.LastName,
                            fullname.FirstName[0], fullname.MiddleName[0], fullname.Number);
                    }
                    inspectors = new ObservableCollection<Inspector>(ins);
                }
                return inspectors;
            }
        }
        #endregion

        #region Переменная для добавления инспекции
        Inspection newInspection;
        public Inspection NewInspection
        {
            get
            {
                if (newInspection == null)
                    newInspection = new Inspection();
                return newInspection;
            }
            set
            {
                newInspection = value;
                OnPropertyChanged("NewInspection");
            }
        }
        #endregion

        #region Переменная для добавления замечаний
        Remark _newRemark;
        public Remark NewRemark
        {
            get
            {
                if (_newRemark == null)
                    _newRemark = new Remark();
                return _newRemark;
            }
            set
            {
                _newRemark = value;
                OnPropertyChanged("NewRemark");
            }
        }
        #endregion

        #region Коллекция замечаний ListCollectionView и ObservableCollection

        ListCollectionView remarksCollection;
        public ListCollectionView RemarksCollection
        {
            get
            {
                return remarksCollection;
            }
        }

        ObservableCollection<Remark> remarks;
        public ObservableCollection<Remark> Remarks
        {
            get
            {
                if (remarks == null)
                {

                    remarks = new ObservableCollection<Remark>();
                }
                return remarks;
            }
        }
        #endregion
        
        #region Переменная для выбора замечаний в ListBox

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
        #endregion

        #region Команды
        /// <summary>
        /// В этом классе определены несколько команд. Первая команда добавляет замечание в список.
        /// Вторая команда позволяет удалить из этого списка ненужное замечание.
        /// Третья команда сохраняет в базу инспекцию со списком замечаний 
        /// </summary>
        
        //Команда добавления замечания    
        Command addRemarkCommand;
        public Command AddRemarkCommand
        {
            get
            {
                if (addRemarkCommand == null)
                    
                    //экземпляру класса Command передаётся два метода. Один из них производит добавление,
                    //а второй проверяет, может ли команда выполнится. Если нет,то кнопка будет декативированна. 
                    addRemarkCommand = new Command(ExecuteAddRemarkCommand, CanExecuteRemarkCommand);
                return addRemarkCommand;
            }
        }

        public void ExecuteAddRemarkCommand(object parameter)
        {
            Remarks.Add(NewRemark);
            NewRemark = null;
            RemarkAddDate = DateTime.Today;
        }

        public bool CanExecuteRemarkCommand(object parameter)
        {
            if (string.IsNullOrEmpty(NewRemark.Comment) ||string.IsNullOrEmpty(NewRemark.TextOfComments))
                return false;

            return true;
        }

        //команда удаления замечания из списка
        Command removeCommand;
        public Command RemoveRemarkCommand
        {
            //В качестве параметра в команду будет передаваться удаляемый объект
            get
            {
                return removeCommand ??
                (removeCommand = new Command(obj =>
                {
                    Remark remark = obj as Remark; // приведение к нужному типу

                    if (remark != null)
                    {
                        Remarks.Remove(remark);
                    }
                },

                (obj) => Remarks.Count > 0)); //дектививация кнопки при отсутствии замечаний
            }
        }

        //Команда добавления инспекции
        Command addCommand;
        public Command AddCommand
        {
            get
            {

                if (addCommand == null)
                    addCommand = new Command(ExecuteAddCommand, CanExecuteCommand);
                return addCommand;
            }
        }

        public void ExecuteAddCommand(object parameter)
        {
            db.Inspections.Add(NewInspection);
            db.SaveChanges();

            List<Remark> remark = (from r in Remarks
                                   select r).ToList();

            //Циклом foreach добавляются все элементы коллекции 
            foreach (Remark r in remark)
            {
                r.InspectionId = NewInspection.Id;
                db.Remarks.Add(r);
                Remarks.Remove(r);
            }

            db.SaveChanges();

            NewInspection = null;
            InspectionAddDate = DateTime.Today;

            MessageBox.Show("Новая инспекция успешно добавлена");
        }

        public bool CanExecuteCommand(object parameter)
        {
            if (string.IsNullOrEmpty(NewInspection.Name) || NewInspection.Inspector == null || string.IsNullOrEmpty(NewInspection.Comment))
                return false;
            return true;
        }

        #endregion

        #region Свойство для DatePiсker

        /// <summary>
        /// Свойство для установки даты по умолчанию в DatePiсker
        /// </summary>
        DateTime? inspectionAddDate;
        public DateTime? InspectionAddDate
        {
            get
            {
                if (inspectionAddDate == null)
                {
                    inspectionAddDate = DateTime.Today;
                }
                return inspectionAddDate;
            }
            set
            {
                inspectionAddDate = value;
                OnPropertyChanged("InspectionAddDate");
            }
        }
        DateTime? remarkAddDate;
        public DateTime? RemarkAddDate
        {
            get
            {
                if (remarkAddDate == null)
                {
                    remarkAddDate = DateTime.Today;
                }
                return remarkAddDate;
            }
            set
            {
                remarkAddDate = value;
                OnPropertyChanged("RemarkAddDate");
            }
        }
        #endregion

        #region Название кнопки переключения страницы
        public string PageName
        {
            get
            {
                return "Новая Инспекция";
            }
        }
        #endregion

        #region Валидация
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            NewInspection.Name = (string)value;

            //Проверяется поле на соответствие нужных допустимых символов 
            if (!Regex.IsMatch(NewInspection.Name, @"^[а-яА-ЯёЁa-zA-Z\s]+$"))
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
