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
            await DisplayAlert("������", "������� ��� ��������������", "��");
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
            admin = result; // ���� ����� ��� ���� � ���������� ���
        }

        await Navigation.PushAsync(new AdminPage()); // ���� ����� ��������� ��������� ����������
    }

}