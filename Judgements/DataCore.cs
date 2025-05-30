using Judgements.Models;
using Supabase;
using Supabase.Postgrest;  
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Supabase.Postgrest.Constants;

namespace Judgements
{
    public class DataCore
    {
        Supabase.Client _supabaseClient;

        public DataCore() { }

        public async Task InitializeAsync()
        {
            var url = "https://aggoaqbjmsnxwmqskpyl.supabase.co";
            var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImFnZ29hcWJqbXNueHdtcXNrcHlsIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDIzMTgzNTUsImV4cCI6MjA1Nzg5NDM1NX0.Y3uUZYnJXYNbPIfDuvdcvbbA4TsibORwxNvfruj7E5E";
            var options = new SupabaseOptions { AutoConnectRealtime = true };
            _supabaseClient = new Supabase.Client(url, key, options);
            await _supabaseClient.InitializeAsync();
        }

        // === JUDGES ===
        public async Task<Judge> CreateJudgement(Judge entity)
        {
            var result = await _supabaseClient
                .From<Judge>()
                .Filter("name", Operator.Equals, entity.Name)
                .Get();

            var existing = result.Models.FirstOrDefault();
            if (existing != null)
            {
                // Судья уже есть
                return existing;
            }

            // Создаем нового
            var insertResult = await _supabaseClient.From<Judge>().Insert(entity);
            return insertResult.Models.FirstOrDefault();
        }


        public async Task<List<Judge>> GetJudgesAsync()
        {
            var result = await _supabaseClient.From<Judge>().Get();
            return result.Models;
        }

        // === ADMINS ===
        public async Task<Admin> CreateAdminAsync(Admin admin)
        {
            var existing = await _supabaseClient
                .From<Admin>()
                .Where(a => a.Name == admin.Name)
                .Get();

            var match = existing.Models.FirstOrDefault();
            if (match is not null)
                return match;

            var insertResult = await _supabaseClient.From<Admin>().Insert(admin);
            return admin;
        }


        // === STREAMS ===
        public async Task<List<Models.Stream>> GetStreamsAsync()
        {
            var result = await _supabaseClient.From<Models.Stream>().Get();
            return result.Models;
        }

        public async Task<Models.Stream> CreateStreamAsync(Models.Stream stream)
        {
            var response = await _supabaseClient.From<Models.Stream>().Insert(stream);
            return response.Model;
        }

        // === PARTICIPANTS ===
        // Получить всех участников

        public async Task<List<Participant>> GetParticipantsByStreamAsync(Guid streamId)
        {
            var response = await _supabaseClient
                .From<Participant>()
                .Where(p => p.StreamId == streamId)
                .Get();

            return response.Models;
        }


        public async Task<List<Participant>> GetParticipantsAsync()
        {
            var result = await _supabaseClient.From<Participant>().Get();
            return result.Models;
        }

        // Добавить участника
        public async Task<Participant> AddParticipantAsync(Participant participant)
        {
            participant.Id = Guid.NewGuid();
            await _supabaseClient.From<Participant>().Insert(participant);
            return participant;
        }

        // Обновить участника
        public async Task UpdateParticipantAsync(Participant participant)
        {
            await _supabaseClient.From<Participant>().Update(participant);
        }

        // Удалить участника
        public async Task DeleteParticipantAsync(Guid id)
        {
            var participant = new Participant { Id = id };
            await _supabaseClient.From<Participant>().Delete(participant);
        }

        // === SCORES ===
        public async Task AddScoreAsync(Score score)
        {
            var result = await _supabaseClient
                .From<Score>()
                .Where(s => s.JudgeId == score.JudgeId && s.ParticipantId == score.ParticipantId)
                .Get();

            var existingScore = result.Models.FirstOrDefault();

            if (existingScore != null)
            {
                // Оценка уже существует, обновляем значение
                var response = await _supabaseClient
                    .From<Score>()
                    .Where(s => s.Id == existingScore.Id)
                    .Set(s => s.Value, score.Value)
                    .Update();
            }
            else
            {
                // Оценка не существует, создаем новую запись
                var response = await _supabaseClient
                    .From<Score>()
                    .Insert(score);
            }
        }

        public async Task<Models.Stream?> GetStreamByNameAsync(string name)
        {
            var response = await _supabaseClient
                .From<Models.Stream>()
                .Filter("name", Operator.Equals, name)
                .Get();

            return response.Models.FirstOrDefault();
        }




        //public async Task<List<Score>> GetScoresByStreamAsync(Guid streamId)
        //{
        //    var participants = await GetParticipantsByStream(streamId);
        //    var scores = new List<Score>();

        //    foreach (var participant in participants)
        //    {
        //        var result = await _supabaseClient
        //            .From<Score>()
        //            .Filter("participant_id", Constants.Operator.Equals, participant.Id.ToString())
        //            .Get();

        //        scores.AddRange(result.Models);
        //    }

        //    return scores;
        //}

        public async Task<List<Score>> GetScoresByParticipantAsync(Guid participantId)
        {
            var result = await _supabaseClient
                .From<Score>()
                .Filter("participant_id", Constants.Operator.Equals, participantId.ToString())
                .Get();

            return result.Models;
        }

        // === Итоги (оценка + место) ===
        //public async Task<Dictionary<Participant, int>> GetRankingAsync(Guid streamId)
        //{
        //    var participants = await GetParticipantsByStream(streamId);
        //    var result = new Dictionary<Participant, int>();

        //    // Разделяем на группы по 4 (или 3 для Потока 3 — это можно параметризовать позже)
        //    int groupSize = 4;
        //    var groups = participants
        //        .Select((x, i) => new { Index = i, Value = x })
        //        .GroupBy(x => x.Index / groupSize)
        //        .Select(g => g.Select(x => x.Value).ToList());

        //    foreach (var group in groups)
        //    {
        //        var scoredGroup = new List<(Participant, int)>();

        //        foreach (var participant in group)
        //        {
        //            var scores = await GetScoresByParticipantAsync(participant.Id);
        //            var score = scores.FirstOrDefault()?.Value ?? -1; // -1 = не оценен
        //            scoredGroup.Add((participant, score));
        //        }

        //        // Сортировка по оценке, присваиваем места
        //        var sorted = scoredGroup.Where(x => x.Item2 >= 0).OrderByDescending(x => x.Item2).ToList();

        //        for (int i = 0; i < sorted.Count; i++)
        //        {
        //            int place = i switch
        //            {
        //                0 => 1,
        //                1 => 2,
        //                _ => 3
        //            };
        //            result[sorted[i].Item1] = place;
        //        }
        //    }

        //    return result;
        //}
    }
}
