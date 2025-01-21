using Uploadify.Maui.ViewModels;

namespace Uploadify.Maui;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}

