using VoiceVault.Maui.ViewModels;

namespace VoiceVault.Maui;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}

