using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase;
using Judgements.Models;

namespace Judgements
{
    public class DataCore
    {
        Supabase.Client _supabaseClient;
        public DataCore()
        {
        }   
        public async Task InitializeAsync()
        {
            var url = "https://aggoaqbjmsnxwmqskpyl.supabase.co";
            var key = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImFnZ29hcWJqbXNueHdtcXNrcHlsIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDIzMTgzNTUsImV4cCI6MjA1Nzg5NDM1NX0.Y3uUZYnJXYNbPIfDuvdcvbbA4TsibORwxNvfruj7E5E";
            var options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = true
            };
            _supabaseClient = new Supabase.Client(url, key, options);
            await _supabaseClient.InitializeAsync();
        }

        public async Task<JudgementsEntity> CreateJudgement(JudgementsEntity entity)
        {
            var result = await _supabaseClient.From<JudgementsEntity>().Get();
            var models = result.Models.Where(x => x.Name == entity.Name);
            if(models.Count() > 0)
            {
                // Judgement already exists, handle accordingly
                Console.WriteLine("Judgement with this name already exists.");
                return models.FirstOrDefault();
            }
            await _supabaseClient.From<JudgementsEntity>().Insert(entity);
            return null;
        }



    }
}
