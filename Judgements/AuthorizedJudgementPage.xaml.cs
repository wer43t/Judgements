using Judgements.Models;

namespace Judgements;

public partial class AuthorizedJudgementPage : ContentPage
{
    public string Header { get; set; }
    private readonly DataCore _dataCore = new DataCore();
    private readonly Judge _judge;

    public AuthorizedJudgementPage(Judge entity)
	{
		InitializeComponent();
        OnLoad();
        Header = entity.Name;
        _judge = entity;
        BindingContext = this;
    }

    private async void OnLoad()
    {
        await _dataCore.InitializeAsync();
    }

    private async void StreamOne_Clicked(object sender, EventArgs e)
    {
        var stream = await _dataCore.GetStreamByNameAsync("����� 1");
        if (stream == null)
        {
            await DisplayAlert("������", "����� �� ������", "OK");
            return;
        }
        if (stream != null)
        {
            await Navigation.PushAsync(new ScoreView(_judge, stream));
        }
    }

    private async void StreamTwo_Clicked(object sender, EventArgs e)
    {
        var stream = await _dataCore.GetStreamByNameAsync("����� 2");
        if (stream == null)
        {
            await DisplayAlert("������", "����� �� ������", "OK");
            return;
        }
        if (stream != null)
        {
            await Navigation.PushAsync(new ScoreView(_judge, stream));
        }
    }

    private async void StreamThree_Clicked(object sender, EventArgs e)
    {
        var stream = await _dataCore.GetStreamByNameAsync("����� 3");
        if (stream == null)
        {
            await DisplayAlert("������", "����� �� ������", "OK");
            return;
        }
        if (stream != null)
        {
            await Navigation.PushAsync(new ScoreView(_judge, stream));
        }
    }
}