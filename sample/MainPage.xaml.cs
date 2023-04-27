using CommunityToolkit.Mvvm.Input;

namespace The49.Maui.ContextMenu.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	[RelayCommand]
	void GoToPage(Type page)
	{
		Navigation.PushAsync((Page)Activator.CreateInstance(page));
	}
}

