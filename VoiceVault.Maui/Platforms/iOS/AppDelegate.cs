using Foundation;
using UIKit;

namespace VoiceVault.Maui;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    private static readonly Dictionary<NSUrl, Action> BackgroundSessionCompletionHandlers = new Dictionary<NSUrl, Action>();

	protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication app, NSDictionary options)
    {
        return base.FinishedLaunching(app, options);
    }

    /* For some reason this override is not working, so I had to use the Export attribute 
    public override void HandleEventsForBackgroundUrl(UIApplication application, string sessionIdentifier, Action completionHandler)
    {
        
    }
    */

    [Foundation.Export("application:handleEventsForBackgroundURLSession:completionHandler:")]
    public void HandleEventsForBackgroundUrl(UIApplication app, NSUrl url, NSDictionary options, Action completionHandler)
    {
        // Save the completion handler to call it later
        BackgroundSessionCompletionHandlers[url] = completionHandler;
    }

    [Export("URLSessionDidFinishEventsForBackgroundURLSession:")]
    public void DidFinishEventsForBackgroundSession(NSUrlSession session)
    {
        NSUrl url = new NSUrl(session.Configuration.Identifier ?? string.Empty);
        if (BackgroundSessionCompletionHandlers.TryGetValue(url, out var completionHandler))
        {
            completionHandler?.Invoke();
            BackgroundSessionCompletionHandlers.Remove(url);
        }
    }
}
