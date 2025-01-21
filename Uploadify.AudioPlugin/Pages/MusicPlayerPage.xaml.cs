using Uploadify.Maui.ViewModels;

namespace Uploadify.Maui.Pages;

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