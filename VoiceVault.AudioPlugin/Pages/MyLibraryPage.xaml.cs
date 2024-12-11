using VoiceVault.Maui.ViewModels;

namespace VoiceVault.Maui.Pages;

public partial class MyLibraryPage : ContentPage
{
	public MyLibraryPage(MyLibraryPageViewModel myLibraryPageViewModel)
	{
		InitializeComponent();

		BindingContext = myLibraryPageViewModel;
	}
}