using Judgements.Models;

namespace Judgements;

public partial class AdminPage : ContentPage
{
    private DataCore _dataCore = new DataCore();
    private Participant _selectedParticipant;
    private List<Participant> _allParticipants = new();
    private List<Participant> _participants = new();
    private List<Models.Stream> _streams = new();

    public AdminPage()
    {
        InitializeComponent();
        LoadData();
    }

    private async void LoadData()
    {
        await _dataCore.InitializeAsync();

        _allParticipants = await _dataCore.GetParticipantsAsync();
        _streams = await _dataCore.GetStreamsAsync();

        streamPicker.ItemsSource = _streams.Select(s => s.Name).ToList();
        filterStreamPicker.ItemsSource = _streams.Select(s => s.Name).ToList();

        filterStreamPicker.SelectedIndex = 0;

        UpdateParticipantsList();
    }

    private void FilterStreamPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        UpdateParticipantsList();
    }

    private void SearchBar_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateParticipantsList();
    }

    private void UpdateParticipantsList()
    {
        if (_streams.Count == 0 || filterStreamPicker.SelectedIndex < 0)
            return;

        var selectedStream = _streams[filterStreamPicker.SelectedIndex];
        var filtered = _allParticipants
            .Where(p => p.StreamId == selectedStream.Id &&
                        (string.IsNullOrWhiteSpace(searchBar.Text) || p.FullName.ToLower().Contains(searchBar.Text.ToLower())))
            .ToList();

        participantsCollection.ItemsSource = filtered;
        participantsCountLabel.Text = $"Участников: {filtered.Count}";
    }

    private async void AddParticipantBtn_Clicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(fullNameEntry.Text) || streamPicker.SelectedIndex == -1)
            return;

        var selectedStream = _streams[streamPicker.SelectedIndex];

        var newParticipant = new Participant
        {
            FullName = fullNameEntry.Text.Trim(),
            StreamId = selectedStream.Id
        };

        await _dataCore.AddParticipantAsync(newParticipant);
        LoadData();
        ClearForm();
    }

    private void participantsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedParticipant = (Participant)e.CurrentSelection.FirstOrDefault();
        if (_selectedParticipant != null)
        {
            fullNameEntry.Text = _selectedParticipant.FullName;
            var index = _streams.FindIndex(s => s.Id == _selectedParticipant.StreamId);
            streamPicker.SelectedIndex = index;
            updateBtn.IsVisible = true;
            deleteBtn.IsVisible = true;
        }
    }

    private async void updateBtn_Clicked(object sender, EventArgs e)
    {
        if (_selectedParticipant == null) return;

        _selectedParticipant.FullName = fullNameEntry.Text;
        _selectedParticipant.StreamId = _streams[streamPicker.SelectedIndex].Id;

        await _dataCore.UpdateParticipantAsync(_selectedParticipant);
        LoadData();
        ClearForm();
    }

    private async void deleteBtn_Clicked(object sender, EventArgs e)
    {
        if (_selectedParticipant == null) return;

        await _dataCore.DeleteParticipantAsync(_selectedParticipant.Id);
        LoadData();
        ClearForm();
    }

    private void ClearForm()
    {
        fullNameEntry.Text = string.Empty;
        streamPicker.SelectedIndex = -1;
        _selectedParticipant = null;
        updateBtn.IsVisible = false;
        deleteBtn.IsVisible = false;
    }

    private void addTabBtn_Clicked(object sender, EventArgs e)
    {
        AddTabContent.IsVisible = true;
        ListTabContent.IsVisible = false;

        addTabBtn.BackgroundColor = Color.FromArgb("#89c7d8");
        addTabBtn.TextColor = Colors.White;

        listTabBtn.BackgroundColor = Color.FromArgb("#f0f4f8");
        listTabBtn.TextColor = Color.FromArgb("#1f3a93");
    }

    private void listTabBtn_Clicked(object sender, EventArgs e)
    {
        AddTabContent.IsVisible = false;
        ListTabContent.IsVisible = true;

        addTabBtn.BackgroundColor = Color.FromArgb("#f0f4f8");
        addTabBtn.TextColor = Color.FromArgb("#1f3a93");

        listTabBtn.BackgroundColor = Color.FromArgb("#89c7d8");
        listTabBtn.TextColor = Colors.White;
    }
}
