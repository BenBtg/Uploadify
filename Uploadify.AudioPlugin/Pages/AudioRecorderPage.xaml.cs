﻿namespace Uploadify.Maui.Pages;

public partial class AudioRecorderPage : ContentPage
{
	public AudioRecorderPage(ViewModels.AudioRecorderPageViewModel audioRecorderPageViewModel)
	{
		InitializeComponent();

		BindingContext = audioRecorderPageViewModel;
	}

	protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
	{
		base.OnNavigatedFrom(args);

		((ViewModels.AudioRecorderPageViewModel)BindingContext).OnNavigatedFrom();
	}
}