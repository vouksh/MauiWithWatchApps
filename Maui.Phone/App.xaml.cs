using Maui.Phone.Services;
using Microsoft.Extensions.Logging;
using System.Timers;

namespace Maui.Phone;

public partial class App : Application
{
	private readonly ILogger<App> _logger;
    private readonly System.Timers.Timer _timer;
    private readonly WearableInteractionService _interactionService;
    public App(MainPage mainPage, ILogger<App> logger,  WearableInteractionService interactionService)
    {
        InitializeComponent();
        _logger = logger;
        _logger.LogInformation("Opening main page");
        MainPage = new NavigationPage(mainPage);
        _timer = new System.Timers.Timer(TimeSpan.FromSeconds(10))
        {
            AutoReset = true
        };
        _timer.Elapsed += Timer_Tick;
        _timer.Start();
        _interactionService = interactionService;
    }

    private void Timer_Tick(object sender, EventArgs e)
    {
        _interactionService.SendMessage();
    }

    protected override void OnStart()
    {
        base.OnStart();
        _interactionService.Connect();
    }

    protected override void OnSleep()
    {
        base.OnSleep();
    }

    protected override void OnResume()
    {
        base.OnResume();
        _interactionService.Connect();
    }
}