namespace ServerMultiTool.ViewModels.Contracts.Interfaces
{
    interface INavigationAware
    {
        void OnNavigatedTo(object parameter);
        void OnNavigatedFrom();
    }
}
