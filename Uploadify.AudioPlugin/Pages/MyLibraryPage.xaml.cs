using Uploadify.Maui.ViewModels;

namespace Uploadify.Maui.Pages;

public partial class MyLibraryPage : ContentPage
{
	public MyLibraryPage(MyLibraryPageViewModel myLibraryPageViewModel)
	{
		InitializeComponent();

		BindingContext = myLibraryPageViewModel;
	}
}