﻿namespace Judgements
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            //count++;

            //if (count == 1)
            //    CounterBtn.Text = $"Clicked {count} time";
            //else
            //    CounterBtn.Text = $"Clicked {count} times";

            //SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private void JudgementsBtn_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new JudgementPage());
        }

        private void AdminBtn_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new AdminLoginPage());
        }

        private void ScoresBtn_Clicked(object sender, EventArgs e)
        {
            Navigation.PushAsync(new ViewResultsByStream());
        }
    }
}
