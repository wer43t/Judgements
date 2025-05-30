using Judgements.Models;

namespace Judgements;

public partial class ScoreView : ContentPage
{

    private readonly Judge _judge;
    private readonly Models.Stream _stream;
    private readonly DataCore _dataCore = new();
    private List<Participant> _participants;
    private Dictionary<Guid, float> _scores = new();
    private Entry _focusedEntry;
    public ScoreView(Judge judge, Models.Stream stream)
	{
		InitializeComponent();
        _judge = judge;
        _stream = stream;
        LoadData();
    }

    private async Task LoadData()
    {
        loadingIndicator.IsRunning = true;
        loadingIndicator.IsVisible = true;
        loadingBackground.IsVisible = true;

        try
        {
            await _dataCore.InitializeAsync();
            _participants = await _dataCore.GetParticipantsByStreamAsync(_stream.Id);

            var allScores = new List<Score>();
            foreach (var participant in _participants)
            {
                var scores = await _dataCore.GetScoresByParticipantAsync(participant.Id);
                allScores.AddRange(scores);
            }

            int groupSize = (_stream.Name == "����� 3") ? 3 : 4;

            var grouped = _participants
                .Select((p, i) =>
                {
                    int groupIndex = i / groupSize;
                    int indexInGroup = i % groupSize;
                    var groupParticipants = _participants.Skip(groupIndex * groupSize).Take(groupSize).ToList();

                    // ���� �� � ������� ��������� ������ �� �������� �����?
                    bool hasMyScoreInGroup = allScores.Any(s =>
                        groupParticipants.Any(gp => gp.Id == s.ParticipantId) &&
                        s.JudgeId == _judge.Id);

                    // ������ �������� ��������� ���� ������
                    var myScore = allScores.FirstOrDefault(s => s.ParticipantId == p.Id && s.JudgeId == _judge.Id);

                    // ���� �� � �������� ��������� ������ �� ������� �����?
                    bool hasOtherScore = allScores.Any(s => s.ParticipantId == p.Id && s.JudgeId != _judge.Id);

                    // ����� �� ����� ������������� ��� ������?
                    bool canEdit = (!hasMyScoreInGroup && !hasOtherScore) || (myScore != null);

                    return new ParticipantScoreViewModel
                    {
                        RowNumber = indexInGroup + 1,
                        Id = p.Id,
                        FullName = p.FullName,
                        Score = allScores.FirstOrDefault(s => s.ParticipantId == p.Id)?.Value,
                        CanEdit = canEdit,
                        IsEditing = false
                    };
                })
                .Select((vm, i) => new { vm, i })
                .GroupBy(x => x.i / groupSize)
                .Select(g => g.Select(x => x.vm).ToList())
                .ToList();

            groupedParticipantsCollection.ItemsSource = grouped;
        }
        finally
        {
            loadingIndicator.IsRunning = false;
            loadingIndicator.IsVisible = false;
            loadingBackground.IsVisible = false;
        }
    }

    private void Entry_Focused(object sender, FocusEventArgs e)
    {
        if (sender is Entry entry && entry.BindingContext is ParticipantScoreViewModel focusedParticipant)
        {
            // ��������� ������ �� ��������������� Entry
            _focusedEntry = entry;

            // ������� ��� ������ � ������������� IsEditing
            var allGroups = groupedParticipantsCollection.ItemsSource as IEnumerable<IEnumerable<ParticipantScoreViewModel>>;
            if (allGroups == null) return;

            foreach (var group in allGroups)
            {
                foreach (var participant in group)
                {
                    participant.IsEditing = participant == focusedParticipant;
                }
            }
        }
    }


    private bool _isSaving = false; // ���� ��� �������������� ��������� �������

    private async void ConfirmScore_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is ParticipantScoreViewModel participant)
        {
            if (!participant.CanEdit || _isSaving) // ��������� ����
                return;

            if (participant.Score is float score)
            {
                _isSaving = true; // ������������� ����
                loadingIndicator.IsRunning = true;
                loadingIndicator.IsVisible = true;
                loadingBackground.IsVisible = true;
                btn.IsEnabled = false; // ��������� ������, ����� �������� ��������� �������

                try
                {
                    var newScore = new Score
                    {
                        Id = Guid.NewGuid(),
                        JudgeId = _judge.Id,
                        ParticipantId = participant.Id,
                        Value = score
                    };
                    await _dataCore.AddScoreAsync(newScore);
                    await LoadData();

                    Dispatcher.Dispatch(() =>
                    {
                        if (_focusedEntry != null)
                        {
                            _focusedEntry.Unfocus();
                            _focusedEntry = null;
                        }
                    });
                }
                finally
                {
                    loadingIndicator.IsRunning = false;
                    loadingIndicator.IsVisible = false;
                    loadingBackground.IsVisible = false;
                    btn.IsEnabled = true; // �������� ������ �������
                    _isSaving = false; // ���������� ����
                }
            }
            else
            {
                await DisplayAlert("������", "������� ���������� ������", "OK");
            }
        }
    }




    private async void SaveScores_Clicked(object sender, EventArgs e)
    {
        foreach (var kvp in _scores)
        {
            var score = new Score
            {
                Id = Guid.NewGuid(),
                JudgeId = _judge.Id,
                ParticipantId = kvp.Key,
                Value = kvp.Value
            };

            await _dataCore.AddScoreAsync(score); // ������ ���� ����� � DataCore
        }
        await LoadData();
        await DisplayAlert("�����", "������ ���������", "OK");
    }

    private void Score_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Entry entry)
        {
            string newText = e.NewTextValue;
            string oldText = e.OldTextValue;

            if (string.IsNullOrWhiteSpace(newText))
            {
                if (entry.BindingContext is ParticipantScoreViewModel viewModel)
                {
                    viewModel.Score = null;
                }
                return;
            }

            // �������� ������� �� �����
            newText = newText.Replace(',', '.');

            // ���������, �������� �� ����� ����� ���������� float ��� ��������� � �������� ����� float
            if (float.TryParse(newText, System.Globalization.CultureInfo.InvariantCulture, out float parsedValue) ||
                newText.EndsWith(".") ||
                (newText.Count(c => c == '.') <= 1 && newText.All(c => char.IsDigit(c) || c == '.')))
            {
                entry.Text = newText; // ��������� ����� � Entry

                if (entry.BindingContext is ParticipantScoreViewModel viewModel)
                {
                    viewModel.Score = parsedValue; // ��������� ViewModel, ���� ���� ������� ����� �����
                }
            }
            else
            {
                entry.Text = oldText; // ��������� ������������ ����
            }
        }
    }
    // The error indicates that "FindVisualElement" is not a method of "CollectionView".  
    // To fix this, you need to implement a helper method to find a visual element within the CollectionView.  

}