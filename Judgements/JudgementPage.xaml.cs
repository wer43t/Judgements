using Judgements.Models;

namespace Judgements;

public partial class JudgementPage : ContentPage
{
	public JudgementPage()
	{
		InitializeComponent();
	}

    private void AuthorizeBtn_Clicked(object sender, EventArgs e)
    {
		JudgementsEntity entity = new JudgementsEntity { Name = nameEntry.Text, CreatedAt = DateTime.Now };
		DataCore dataCore = new DataCore();
		dataCore.InitializeAsync();
		dataCore.CreateJudgement(entity);
		//if(result is not null)
		//{
		//	entity = result;
		//}
		Navigation.PushAsync(new AuthorizedJudgementPage(entity));
    }
}