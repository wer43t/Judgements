using Judgements.Models;

namespace Judgements;

public partial class AuthorizedJudgementPage : ContentPage
{
    public string Header { get; set; }
	public AuthorizedJudgementPage(JudgementsEntity entity)
	{
		InitializeComponent();
        Header = entity.Name;
        BindingContext = this;
    }

    private void StreamOne_Clicked(object sender, EventArgs e)
    {

    }

    private void StreamTwo_Clicked(object sender, EventArgs e)
    {

    }

    private void StreamThree_Clicked(object sender, EventArgs e)
    {

    }
}