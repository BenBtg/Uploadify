using VoiceVault.Maui.ViewModels;

namespace VoiceVault.Maui.Pages;

public partial class MusicPlayerPage : ContentPage
{
	public MusicPlayerPage(MusicPlayerPageViewModel musicPlayerPageViewModel)
	{
		InitializeComponent();

		BindingContext = musicPlayerPageViewModel;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();

		((MusicPlayerPageViewModel)BindingContext).TidyUp();
	}
}