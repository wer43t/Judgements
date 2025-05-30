using Judgements.Models;

namespace Judgements;

public partial class JudgementPage : ContentPage
{
    public JudgementPage()
    {
        InitializeComponent();
    }

    private async void AuthorizeBtn_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(nameEntry.Text))
        {
            await DisplayAlert("ќшибка", "¬ведите им€ судьи", "ќ ");
            return;
        }

        var entity = new Judge
        {
            Name = nameEntry.Text.Trim(),
        };

        var dataCore = new DataCore();
        await dataCore.InitializeAsync();
        var result = await dataCore.CreateJudgement(entity);

        if (result is not null)
        {
            entity = result; // если судь€ уже есть Ч используем его
        }

        await Navigation.PushAsync(new AuthorizedJudgementPage(entity));
    }
}
