using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace Judgements.Models
{
    [Table("judges")]
    public class Judge : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }

    [Table("admins")]
    public class Admin : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }

    [Table("streams")]
    public class Stream : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }

    [Table("participants")]
    public class Participant : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("full_name")]
        public string FullName { get; set; }

        [Column("stream_id")]
        public Guid StreamId { get; set; }
    }

    [Table("scores")]
    public class Score : BaseModel
    {
        [PrimaryKey("id")]
        public Guid Id { get; set; }

        [Column("judge_id")]
        public Guid JudgeId { get; set; }

        [Column("participant_id")]
        public Guid ParticipantId { get; set; }

        [Column("value")]
        public float Value { get; set; }
    }
}
