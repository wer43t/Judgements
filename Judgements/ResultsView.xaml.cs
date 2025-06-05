using Judgements.Models;
namespace Judgements;

public partial class ResultsView : ContentPage
{
    private readonly DataCore _dataCore;
    private readonly Models.Stream _stream;

    public ResultsView(Models.Stream stream)
    {
        InitializeComponent();
        _dataCore = new DataCore();
        _stream = stream;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadResults();
    }

    private async Task LoadResults()
    {

        await _dataCore.InitializeAsync();
        var participants = await _dataCore.GetParticipantsByStreamAsync(_stream.Id);
        var allScores = new List<Score>();
        foreach (var participant in participants)
        {
            var scores = await _dataCore.GetScoresByParticipantAsync(participant.Id);
            allScores.AddRange(scores);
        }

        int groupSize = (_stream.Name == "Поток 3") ? 3 : 4;

        var groupedParticipantsWithScores = participants
            .Select((p, i) => new { Participant = p, Index = i })
            .GroupBy(x => x.Index / groupSize)
            .Select(g => g.Select(x =>
            {
                var score = allScores.FirstOrDefault(s => s.ParticipantId == x.Participant.Id); // Получаем первую оценку для примера
                return new ParticipantScoreViewModel
                {
                    RowNumber = x.Index % groupSize + 1,
                    Id = x.Participant.Id,
                    FullName = x.Participant.FullName,
                    Score = score?.Value // Используем одну оценку для примера
                };
            }).ToList())
            .ToList();

        var groupedResults = ProcessResults(groupedParticipantsWithScores);
        resultsCollectionView.ItemsSource = groupedResults;
    }

    private List<List<ParticipantResultViewModel>> ProcessResults(List<List<ParticipantScoreViewModel>> groupedParticipantsWithScores)
    {
        var groupedResults = new List<List<ParticipantResultViewModel>>();

        foreach (var group in groupedParticipantsWithScores)
        {
            var resultsInGroup = new List<ParticipantResultViewModel>();
            var scoresInGroup = group.Where(p => p.Score.HasValue).OrderBy(p => p.Score).ToList();
            int groupSize = scoresInGroup.Count;
            int placeCounter = 1;

            if (groupSize > 0)
            {
                float? minScore = scoresInGroup.First().Score;
                float? maxScore = scoresInGroup.Last().Score;

                for (int i = 0; i < groupSize; i++)
                {
                    var participantScore = scoresInGroup[i];
                    int place;

                    if (participantScore.Score == minScore && groupSize == 4)
                    {
                        place = 3;
                    }
                    else if (participantScore.Score == minScore && groupSize == 3)
                    {
                        place = 3;
                    }
                    else if (participantScore.Score == maxScore)
                    {
                        place = 1;
                    }
                    else
                    {
                        place = 2;
                    }

                    resultsInGroup.Add(new ParticipantResultViewModel
                    {
                        RowNumber = participantScore.RowNumber,
                        Id = participantScore.Id,
                        FullName = participantScore.FullName,
                        AverageScore = participantScore.Score, // В данном случае используем Score как итоговую оценку
                        Place = place
                    });

                    // Обработка одинаковых оценок и пропуск мест
                    if (i < groupSize - 1 && scoresInGroup[i].Score == scoresInGroup[i + 1].Score)
                    {
                        // Пропускаем следующего, так как у него такая же оценка
                    }
                    else
                    {
                        placeCounter++;
                    }
                }
            }

            // Добавляем участников без оценок в конец с последним местом (или без места)
            foreach (var participant in group.Where(p => !p.Score.HasValue))
            {
                resultsInGroup.Add(new ParticipantResultViewModel
                {
                    RowNumber = participant.RowNumber,
                    Id = participant.Id,
                    FullName = participant.FullName,
                    AverageScore = null,
                    Place = groupSize > 0 ? groupSize + 1 : 1 // Или другое значение по вашему усмотрению
                });
            }

            groupedResults.Add(resultsInGroup.OrderBy(r => r.RowNumber).ToList()); // Сохраняем порядок
        }

        return groupedResults;
    }

    private async void ExportToExcel_Clicked(object sender, EventArgs e)
    {
        try
        {
            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Итоги");

            // Заголовки
            worksheet.Cell(1, 1).Value = "№";
            worksheet.Cell(1, 2).Value = "ФИО";
            worksheet.Cell(1, 3).Value = "Оценка";
            worksheet.Cell(1, 4).Value = "Место";

            int row = 2;
            foreach (var group in resultsCollectionView.ItemsSource as List<List<ParticipantResultViewModel>> ?? [])
            {
                foreach (var result in group)
                {
                    worksheet.Cell(row, 1).Value = result.RowNumber;
                    worksheet.Cell(row, 2).Value = result.FullName;
                    worksheet.Cell(row, 3).Value = result.AverageScore.HasValue ? result.AverageScore.Value.ToString("F2") : "";
                    worksheet.Cell(row, 4).Value = result.Place;
                    row++;
                }

                // Пропускаем строку между группами
                row++;
            }

            // Сохраняем в файл
            var fileName = $"Итоги_{_stream.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var localPath = Path.Combine(FileSystem.CacheDirectory, fileName);
            workbook.SaveAs(localPath);

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Поделиться Excel",
                File = new ShareFile(localPath)
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось экспортировать: {ex.Message}", "OK");
        }
    }

}