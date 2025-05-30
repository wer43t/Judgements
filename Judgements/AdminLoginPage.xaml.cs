using Judgements.Models;

namespace Judgements;

public partial class AdminLoginPage : ContentPage
{
	public AdminLoginPage()
	{
		InitializeComponent();
	}


    private async void AuthorizeBtn_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(nameEntry.Text))
        {
            await DisplayAlert("ќшибка", "¬ведите им€ администратора", "ќ ");
            return;
        }

        var admin = new Admin
        {
            Name = nameEntry.Text.Trim(),
        };

        var dataCore = new DataCore();
        await dataCore.InitializeAsync();
        var result = await dataCore.CreateAdminAsync(admin);

        if (result is not null)
        {
            admin = result; // если админ уже есть Ч используем его
        }

        await Navigation.PushAsync(new AdminPage()); // сюда потом подключим интерфейс управлени€
    }

}