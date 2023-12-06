using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesData.Models
{
    public class Actor
    {
        public string Name { get; set; }
        [Key] public string Id { get; set; }
        public bool IsProducer = false;
        public List<Movie> Movies { get; set; }
        public Actor() { }

    }
}
