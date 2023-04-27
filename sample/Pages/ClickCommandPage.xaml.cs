using CommunityToolkit.Mvvm.Input;

namespace The49.Maui.ContextMenu.Sample.Pages;

public partial class ClickCommandPage : ContentPage
{
	public ClickCommandPage()
	{
		InitializeComponent();
	}

	[RelayCommand]
	void OnClick(string message)
	{
		DisplayAlert("From click command", message, "Ok");
	}
}