using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesData.Models
{
    public class Tag
    {
        public Tag() { }
        public Tag(string name, string id)
        {
            Name = name;
            Id = id;
        }
        public string Name { get; set; }
        [Key] public string Id { get; set; }
        public List<Movie> Movies { get; set; } = new();
    }
}
