using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace IS
{
    /// <summary>
    /// Этот класс является стартовой страницей приложения. На View этого класса будут располагаться 
    /// кнопки переключения между страницами. 
    /// </summary>
    public class HeadViewModel : ViewModelBase
    {

        #region Конструктор класса
        
        public HeadViewModel()
        {
            // добавлeние необходимых ViewModel
            
            PageViewModels.Add(new ShellViewModel());
            PageViewModels.Add(new InspectionsViewModel());
            PageViewModels.Add(new EditInspectionsViewModel());
            PageViewModels.Add(new InspectorsViewModel());

            //Устанаовка в качестве первой загруженной страницы
            CurrentPageViewModel = PageViewModels.FirstOrDefault();

        }
        #endregion

        #region Коллекция страниц

        /// <summary>
        /// В эту коллекцию добавляются все необходимые ViewModel
        /// </summary>
       
        List<IPageViewModel> pageViewModels;
        public List<IPageViewModel> PageViewModels
        {
            get
            {
                if (pageViewModels == null)
                    pageViewModels = new List<IPageViewModel>();

                return pageViewModels;
            }
        }
        #endregion

        #region Команда смены страницы

        ICommand changePageCommand;
        public ICommand ChangePageCommand
        {
            get
            {
                if (changePageCommand == null)
                {
                    changePageCommand = new Command(
                        page => ChangeViewModel((IPageViewModel)page),
                        page => page is IPageViewModel);

                }

                return changePageCommand;
            }
        }
        #endregion

        #region Поле и свойство текущей страницы
        
        IPageViewModel currentPageViewModel;
        public IPageViewModel CurrentPageViewModel
        {
            get
            {
                return currentPageViewModel;
            }
            set
            {
                currentPageViewModel = value;
                OnPropertyChanged("CurrentPageViewModel");
            }
        }
        #endregion

        #region Метод смены страницы
        public void ChangeViewModel(IPageViewModel viewmodel)
        {
            CurrentPageViewModel = (from page in PageViewModels
                                    where page == viewmodel
                                    select page).FirstOrDefault();
            
        }
        #endregion

        #region Валидация(в этом окне не используется)
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
