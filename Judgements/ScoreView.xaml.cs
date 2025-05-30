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

            int groupSize = (_stream.Name == "Поток 3") ? 3 : 4;

            var grouped = _participants
                .Select((p, i) =>
                {
                    int groupIndex = i / groupSize;
                    int indexInGroup = i % groupSize;
                    var groupParticipants = _participants.Skip(groupIndex * groupSize).Take(groupSize).ToList();

                    // Есть ли в текущей подгруппе оценка от ТЕКУЩЕГО судьи?
                    bool hasMyScoreInGroup = allScores.Any(s =>
                        groupParticipants.Any(gp => gp.Id == s.ParticipantId) &&
                        s.JudgeId == _judge.Id);

                    // Оценка текущего участника этим судьёй
                    var myScore = allScores.FirstOrDefault(s => s.ParticipantId == p.Id && s.JudgeId == _judge.Id);

                    // Есть ли у текущего участника оценка от ДРУГОГО судьи?
                    bool hasOtherScore = allScores.Any(s => s.ParticipantId == p.Id && s.JudgeId != _judge.Id);

                    // Может ли судья редактировать эту запись?
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
            // Сохраняем ссылку на сфокусированный Entry
            _focusedEntry = entry;

            // Находим все группы и устанавливаем IsEditing
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


    private bool _isSaving = false; // Флаг для предотвращения повторных нажатий

    private async void ConfirmScore_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is ParticipantScoreViewModel participant)
        {
            if (!participant.CanEdit || _isSaving) // Проверяем флаг
                return;

            if (participant.Score is float score)
            {
                _isSaving = true; // Устанавливаем флаг
                loadingIndicator.IsRunning = true;
                loadingIndicator.IsVisible = true;
                loadingBackground.IsVisible = true;
                btn.IsEnabled = false; // Отключаем кнопку, чтобы избежать повторных нажатий

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
                    btn.IsEnabled = true; // Включаем кнопку обратно
                    _isSaving = false; // Сбрасываем флаг
                }
            }
            else
            {
                await DisplayAlert("Ошибка", "Введите корректную оценку", "OK");
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

            await _dataCore.AddScoreAsync(score); // Добавь этот метод в DataCore
        }
        await LoadData();
        await DisplayAlert("Успех", "Оценки сохранены", "OK");
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

            // Заменяем запятую на точку
            newText = newText.Replace(',', '.');

            // Проверяем, является ли новый текст допустимым float или находится в процессе ввода float
            if (float.TryParse(newText, System.Globalization.CultureInfo.InvariantCulture, out float parsedValue) ||
                newText.EndsWith(".") ||
                (newText.Count(c => c == '.') <= 1 && newText.All(c => char.IsDigit(c) || c == '.')))
            {
                entry.Text = newText; // Обновляем текст в Entry

                if (entry.BindingContext is ParticipantScoreViewModel viewModel)
                {
                    viewModel.Score = parsedValue; // Обновляем ViewModel, даже если введено целое число
                }
            }
            else
            {
                entry.Text = oldText; // Отклоняем некорректный ввод
            }
        }
    }
    // The error indicates that "FindVisualElement" is not a method of "CollectionView".  
    // To fix this, you need to implement a helper method to find a visual element within the CollectionView.  

}