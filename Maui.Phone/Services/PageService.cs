using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Maui.Phone.Services;

public class PageService
{
	private readonly IServiceProvider _services;
	private readonly ILogger<PageService> _logger;

	public PageService(IServiceProvider services, ILogger<PageService> logger)
	{
		_services = services;
		_logger = logger;
	}

	protected INavigation Navigation
	{
		get
		{
			var nav = Application.Current?.MainPage?.Navigation;
			if (nav is not null)
			{
				return nav;
			}

			throw new NullReferenceException("Navigation cannot be null!");
		}
	}

	public Task NavigateToPage<T>(bool animated = true) where T : Page
	{
		var page = ResolvePage<T>();

		if (page is not null)
		{
			_logger.LogInformation("Navigating to page {Page}", typeof(T).Name);
			return Navigation.PushAsync(page, animated);
		}
		throw new InvalidOperationException($"Unable to resolve page {typeof(T)}");
	}

	public Task NavigateToPage(Type pageType, bool animated = true)
	{
		var page = _services.GetService(pageType) as Page;
		if (page is not null)
        {
            _logger.LogInformation("Navigating to page {Page}", pageType.Name);
            return Navigation.PushAsync(page, animated);
        }
        throw new InvalidOperationException($"Unable to resolve page {pageType}");
    }

	public Task NavigateBack(bool animated = true)
	{
		if (Navigation.NavigationStack.Count > 1)
			return Navigation.PopAsync(animated);

		throw new InvalidOperationException("No pages left on navigation stack");
	}

	public Task NavigateHome(bool animated = true)
	{
		return Navigation.PopToRootAsync(animated);
	}

	private T ResolvePage<T>() where T : Page
	{
		return _services.GetService<T>();
	}

	public Task ShowAlertForPage<T>() where T : Page
	{
		try
		{
			var page = ResolvePage<T>();
			if (page is null)
				throw new InvalidOperationException("Invalid page");
			
			return page.DisplayAlert("test", "test", "test");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Could not display alert for page");
			return Task.FromException(ex);
		}
	}

	public Task ShowAlert(string title, string message, string cancelButtonText = "Cancel")
	{
		var currentPage = Navigation.NavigationStack.Last();
		return currentPage.DisplayAlert(title, message, cancelButtonText);
	}
	
	
	public Task<bool> ShowConfirmationAlert(string title, string message, string cancelButtonText = "Cancel", string okButtonText = "Ok")
	{
		var currentPage = Navigation.NavigationStack.Last();
		return currentPage.DisplayAlert(title, message, okButtonText, cancelButtonText);
	}

	public Task NavigateByName(string pageName)
	{
		var type = Type.GetType($"Maui.Phone.Views.{pageName}Page");
		if (type != null)
		{
			return NavigateToPage(type, true);
		}

        throw new InvalidOperationException($"Unable to find page {pageName}");
    }
}

