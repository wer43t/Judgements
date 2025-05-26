using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Judgements.Models
{
    [Table("judgements")]
    public class JudgementsEntity : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("name")]
        public string Name { get; set; }
        //... etc.
    }
}
