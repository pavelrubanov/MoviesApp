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
        public List<Movie> SimilarMovies = new();
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
            int actorsIntersect = Actors.Intersect(movie.Actors).Count();
            int actorsUnion = Actors.Union(movie.Actors).Count();

            int tagsIntersect = Tags.Intersect(movie.Tags).Count();
            int tagsUnion = Tags.Union(movie.Tags).Count();

            double actorsRate;
            if (actorsUnion != 0)
                actorsRate = actorsIntersect / actorsUnion;
            else
                actorsRate = 0;

            double tagsRate;
            if (tagsUnion != 0)
                tagsRate = tagsIntersect / tagsUnion;
            else
                tagsRate = 0;

            double rate = (actorsRate + tagsRate) * 0.5;

            double anotherMovieRate = 0;

            if (movie.Rate != null)
            {
                anotherMovieRate = (double)movie.Rate * 0.05;
            }
            double resultRate = rate * 0.5 + anotherMovieRate;

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
