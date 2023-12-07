using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoviesData.Models
{
    public class Movie
    {
        public string? NameRU { get; set; }
        public string? NameUS { get; set; }
        public List<Actor> Actors = new();
        public List<Tag> Tags = new();
        public double? Rate { get; set; }
        public string? imdbId { get; set; }
        [Key] public string movieId { get; set; }


        /// <summary>
        /// Calculates a similarity score based on the similarity of actors and tags. 
        /// Also use the rating of the film in params.
        /// </summary>
        /// <param name="movie"></param>
        /// <returns>Similarity coefficient from 0 to 1</returns>
        public double Similarity(Movie movie)
        {
            double actorsIntersect = Actors.Select(a => a.Id)
                .Intersect(movie.Actors.Select(a => a.Id))
                .Count();
            //int actorsUnion = Actors.Union(movie.Actors).Count();
            double actorsUnion = Actors.Count;

            double tagsIntersect = Tags.Select(t => t.Id)
                .Intersect(movie.Tags.Select(t => t.Id))
                .Count();
            //int tagsUnion = Tags.Union(movie.Tags).Count();
            double tagsUnion = Tags.Count;

            double actorsRate; //from 0 to 1
            if (actorsUnion != 0)
                actorsRate = actorsIntersect / actorsUnion;
            else
                actorsRate = 0;

            double tagsRate; //from 0 to 1
            if (tagsUnion != 0)
                tagsRate = tagsIntersect / tagsUnion;
            else
                tagsRate = 0;

            double rate = (actorsRate + tagsRate) * 0.5;

            double anotherMovieRate = 0; //from 0 to 10

            if (movie.Rate != null)
            {
                anotherMovieRate = (double)movie.Rate;
            }

            //90% - similar actors and tags, 10% - rate
            double resultRate = rate * 0.9 + (anotherMovieRate * 0.1) * 0.1;
            

            return resultRate;
        }
        public Movie()
        {

        }
    }
    public class MovieSimilarComparer : IComparer<Movie>
    {
        public Movie mainMovie { get; private set; }
        public MovieSimilarComparer(Movie mainMovie)
        {
            this.mainMovie = mainMovie;
        }
        public int Compare(Movie? x, Movie? y)
        {
            if (mainMovie.Similarity(x) < mainMovie.Similarity(y))
                return -1;
            else if (mainMovie.Similarity(x) > mainMovie.Similarity(y))
                return 1;
            else
                return 0;
        }
    }
}
