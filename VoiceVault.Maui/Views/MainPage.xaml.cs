﻿using VoiceVault.Maui.ViewModels;

namespace VoiceVault.Maui;

public partial class MainPage : ContentPage
{
	public MainPage(MainViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}

