using System.ComponentModel;
using Maui.Phone.Interfaces;
using Maui.Phone.Services;
using Microsoft.Extensions.Logging;

namespace Maui.Phone.ViewModels;

public class MainPageViewModel: INotifyPropertyChanged
{

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;

	public void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
    #endregion
    private readonly PageService _pageService;
	private readonly ILogger<MainPageViewModel> _logger;

    public MainPageViewModel(PageService pageService, ILogger<MainPageViewModel> logger)
    {
        _pageService = pageService;
        _logger = logger;
    }

    private string _message = string.Empty;
	public string Message
	{
		get => _message; 
		set
        {
            _message = value;
            RaisePropertyChanged(nameof(Message));
		}
	}

	public Command GoToPageCommand => new(GoToPage);

	private async void GoToPage(object page)
	{
		await _pageService.NavigateByName(page.ToString());
	}
}