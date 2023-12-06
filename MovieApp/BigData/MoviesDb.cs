using Azure.Core;
using MoviesData.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.ComponentModel.Design;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;

namespace MoviesData
{
    public class MoviesDb
    {
        private static void ReacreateDb()
        {
            Parser parser = new Parser();
            parser.ReadInfo();

            using ApplicationContext db = new ApplicationContext();
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Actors.AddRange(parser.actors.Values);
            db.SaveChanges();
            db.Tags.AddRange(parser.tags.Values);
            db.SaveChanges();
            db.Movies.AddRange(parser.movies.Values);
            db.SaveChanges();
        }

        /// <param name="id">movieId(not IMDB id)</param>
        /// <returns>Returns the movie by its movieId. The result may be NULL</returns>
        public static Movie? getMovieById(string id)
        {
            using var db = new ApplicationContext();
            return db.Movies.Where(m => m.movieId == id)
                .Include(m => m.Actors)
                .Include(m => m.Tags)
                .FirstOrDefault();
        }

        /// <param name="id"></param>
        /// <returns>Returns the actor by its Id. The result may be NULL</returns>
        public static Actor? getActorById(string id)
        {
            using var db = new ApplicationContext();
            return db.Actors.Where(a => a.Id == id)
                .Include(a => a.Movies)
                .FirstOrDefault();
        }

        /// <param name="id"></param>
        /// <returns>Returns the tag by its Id. The result may be NULL</returns>
        public static Tag? getTagById(string id)
        {
            using var db = new ApplicationContext();
            return db.Tags.Where(t => t.Id == id)
                .Include(t => t.Movies)
                .FirstOrDefault();
        }

        /// <param name="name"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public static List<Movie> FindMovies(string name, int quantity)
        {
            using var db = new ApplicationContext();
            return db.Movies
                .Where(m => (m.NameRU + m.NameUS).ToLower().Contains(name))
                .Take(quantity)
                .ToList();
        }

        /// <param name="name">Movie name (RU or US)</param>
        /// <param name="quantity">Required number of movies</param>
        public static List<Actor> FindActors(string name, int quantity)
        {
            using var db = new ApplicationContext();
            return db.Actors
                .Where(a => a.Name.ToLower().Contains(name.ToLower()))
                .Take(quantity)
                .ToList();
        }

        /// <param name="name">Tag name</param>
        /// <param name="quantity">Required number of tags.</param>
        /// <returns></returns>
        public static List<Tag> FindTags(string name, int quantity)
        {
            using var db = new ApplicationContext();
            return db.Tags
                .Where(t => t.Name.ToLower().Contains(name.ToLower()))
                .Take(quantity)
                .ToList();
        }

        public static void Main() { }
        
    }
}