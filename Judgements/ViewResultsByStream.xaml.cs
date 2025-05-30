using Judgements.Models;

namespace Judgements;

public partial class ViewResultsByStream : ContentPage
{
    public string Header { get; set; }
    private readonly DataCore _dataCore = new DataCore();
    private readonly Judge _judge;

    public ViewResultsByStream()
	{
		InitializeComponent();
        OnLoad();
    }

    private async void OnLoad()
    {
        await _dataCore.InitializeAsync();
    }

    private async void StreamOne_Clicked(object sender, EventArgs e)
    {
        var stream = await _dataCore.GetStreamByNameAsync("Поток 1");
        if (stream == null)
        {
            await DisplayAlert("Ошибка", "Поток не найден", "OK");
            return;
        }
        if (stream != null)
        {
            await Navigation.PushAsync(new ResultsView(stream));
        }
    }

    private async void StreamTwo_Clicked(object sender, EventArgs e)
    {
        var stream = await _dataCore.GetStreamByNameAsync("Поток 2");
        if (stream == null)
        {
            await DisplayAlert("Ошибка", "Поток не найден", "OK");
            return;
        }
        if (stream != null)
        {
            await Navigation.PushAsync(new ResultsView(stream));
        }
    }

    private async void StreamThree_Clicked(object sender, EventArgs e)
    {
        var stream = await _dataCore.GetStreamByNameAsync("Поток 3");
        if (stream == null)
        {
            await DisplayAlert("Ошибка", "Поток не найден", "OK");
            return;
        }
        if (stream != null)
        {
            await Navigation.PushAsync(new ResultsView(stream));
        }
    }
}