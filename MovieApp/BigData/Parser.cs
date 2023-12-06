using MoviesData.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MoviesData
{
    public class Parser
    {
        private Dictionary<Actor, List<Movie>> actorsMovies = new();
        private Dictionary<Tag, HashSet<Movie>> tagsMovies = new();
        private Dictionary<string, Movie> moviesImdb = new();
        public Dictionary<string, Movie> movies { get; private set; } = new();
        public Dictionary<string, Actor> actors { get; private set; } = new();
        public Dictionary<string, Tag> tags { get; private set; } = new();
        
        private void GenerateSimilarForAllMovies(BlockingCollection<Movie> movies)
        {
            foreach (var movie in movies.GetConsumingEnumerable())
            {
                List <Movie> list = movie.Actors.SelectMany(actor => actorsMovies.GetValueOrDefault(actor))
                             .Union(movie.Tags.SelectMany(tag => tagsMovies.GetValueOrDefault(tag)))
                             .Where(movie => !movie.movieId.IsNullOrEmpty())
                             .DistinctBy(movie => movie.movieId)
                             .ToList();

                //deleting the current movie
                list.Remove(movie); ;

                var Comparer = new MovieSimilarComparer(movie);
                List<Movie> result = new();

                //take top 10 similar movies
                for (int i = 0; i < 10; i++)
                {
                    Movie? maxMovie = list[0];
                    foreach(var m in list)
                    {
                        if (Comparer.Compare(m, maxMovie) == 1)
                        {
                            maxMovie = m;
                        }
                    }
                    list.Remove(maxMovie);
                    result.Add(maxMovie);
                }

                movie.SimilarMovies = result;
            }
        }
        private void ReadMovieCodes_IMDB(BlockingCollection<string> lines)
        {
            string filePath = "..\\..\\..\\ml-latest\\MovieCodes_IMDB.tsv";
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
                lines.CompleteAdding();
            }
        }
        private void ProcessMovieCodes_IMDB(BlockingCollection<string> lines)
        {
            foreach (var line in lines.GetConsumingEnumerable())
            {
                if (line.Contains("US") || line.Contains("RU"))
                {
                    string[] fields = line.Split('\t');
                    string imdbId = fields[0];

                    if (fields[3] == "RU" || fields[4] == "RU")
                    {

                        if (moviesImdb.ContainsKey(imdbId))
                        {
                            lock (moviesImdb)
                            {
                                moviesImdb[imdbId].NameRU = fields[2];
                            }
                        }
                        else
                        {
                            Movie movie = new Movie();
                            movie.NameRU = fields[2];
                            movie.imdbId = fields[0];
                            lock (moviesImdb)
                            {
                                moviesImdb[movie.imdbId] = movie;
                            }
                        }
                    }
                    if (fields[3] == "US" || fields[4] == "US")
                    {
                        if (moviesImdb.ContainsKey(imdbId))
                        {
                            lock (moviesImdb)
                            {
                                moviesImdb[imdbId].NameUS = fields[2];
                            }

                        }
                        else
                        {
                            Movie movie = new Movie();
                            movie.NameUS = fields[2];
                            movie.imdbId = fields[0];
                            lock (moviesImdb)
                            {
                                moviesImdb[movie.imdbId] = movie;
                            }

                        }
                    }
                }
            }
        }

        private void ReadActorsDirectorsNames_IMDB(BlockingCollection<string> lines)
        {
            string filePath = "..\\..\\..\\ml-latest\\ActorsDirectorsNames_IMDB.txt";
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
                lines.CompleteAdding();
            }
        }
        private void ProcessActorsDirectorsNames_IMDB(BlockingCollection<string> lines)
        {
            foreach (var line in lines.GetConsumingEnumerable())
            {
                Actor actor = new Actor();

                var lineSpan = line.AsSpan();

                var index = lineSpan.IndexOf('\t');
                actor.Id = lineSpan.Slice(0, index).ToString();
                lineSpan = lineSpan.Slice(index + 1);

                index = lineSpan.IndexOf('\t');
                actor.Name = lineSpan.Slice(0, index).ToString();

                lock (actors)
                {
                    actors[actor.Id] = actor;
                }
            }
        }
        private void ReadActorsDirectorsCodes_IMDB(BlockingCollection<string> lines)
        {
            string filePath = "..\\..\\..\\ml-latest\\ActorsDirectorsCodes_IMDB.tsv";
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
                lines.CompleteAdding();
            }
        }
        private void ProcessActorsDirectorsCodes_IMDB(BlockingCollection<string> lines)
        {
            foreach (var line in lines.GetConsumingEnumerable())
            {
                var lineSpan = line.AsSpan();
                var index = lineSpan.IndexOf('\t');
                string filmIMDBid = lineSpan.Slice(0, index).ToString();
                lineSpan = lineSpan.Slice(index + 1);

                index = lineSpan.IndexOf("\t");
                lineSpan = lineSpan.Slice(index + 1);

                index = lineSpan.IndexOf("\t");
                string actorId = lineSpan.Slice(0, index).ToString();

                if (moviesImdb.ContainsKey(filmIMDBid) && actors.ContainsKey(actorId))
                {
                    Movie movie = moviesImdb[filmIMDBid];
                    Actor actor = actors[actorId];

                    lock(movie)
                    {
                        movie.Actors.Add(actor);
                    }
      
                    if (!actorsMovies.ContainsKey(actor))
                    {
                        lock (actorsMovies)
                        {
                            actorsMovies[actor] = new();
                        }
                    }
                    lock (actorsMovies)
                    {
                        actorsMovies[actor].Add(moviesImdb[filmIMDBid]);
                    }
                }
            }
        }

        private void ReadRatings_IMDB(BlockingCollection<string> lines)
        {
            string filePath = "..\\..\\..\\ml-latest\\Ratings_IMDB.tsv";
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
                lines.CompleteAdding();
            }
        }
        private void ProcessRatings_IMDB(BlockingCollection<string> lines)
        {
            foreach (var line in lines.GetConsumingEnumerable())
            {
                var lineSpan = line.AsSpan();

                int index = lineSpan.IndexOf('\t');
                string filmIMDBid = lineSpan.Slice(0, index).ToString();

                lineSpan = lineSpan.Slice(index + 1);
                index = lineSpan.IndexOf('\t');
                double rate = Convert.ToDouble(lineSpan.Slice(0, index).ToString(), CultureInfo.InvariantCulture);

                if (moviesImdb.ContainsKey(filmIMDBid))
                {
                    lock (moviesImdb)
                    {
                        moviesImdb[filmIMDBid].Rate = rate;
                    }

                }
            }
        }
        private void Readlinks_IMDB_MovieLens(BlockingCollection<string> lines)
        {
            string filePath = "..\\..\\..\\ml-latest\\links_IMDB_MovieLens.csv";
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
                lines.CompleteAdding();
            }
        }
        private void Processlinks_IMDB_MovieLens(BlockingCollection<string> lines)
        {
            foreach (var line in lines.GetConsumingEnumerable())
            {
                var lineSpan = line.AsSpan();

                int index = lineSpan.IndexOf(',');
                string movieId = lineSpan.Slice(0, index).ToString();

                lineSpan = lineSpan.Slice(index + 1);
                index = lineSpan.IndexOf(',');
                string imdbId = "tt" + lineSpan.Slice(0, index).ToString();


                if (moviesImdb.ContainsKey(imdbId))
                {
                    lock (moviesImdb[imdbId])
                    {
                        moviesImdb[imdbId].movieId = movieId;
                    }
                    lock (movies)
                    {
                        movies[movieId] = moviesImdb[imdbId];
                    }
                }
            }
        }
        private void ReadTagCodes_MovieLens(BlockingCollection<string> lines)
        {
            string filePath = "..\\..\\..\\ml-latest\\TagCodes_MovieLens.csv";
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
                lines.CompleteAdding();
            }
        }
        private void ProcessTagCodes_MovieLens(BlockingCollection<string> lines)
        {
            foreach (var line in lines.GetConsumingEnumerable())
            {
                var lineSpan = line.AsSpan();

                int index = lineSpan.IndexOf(',');
                string tagId = lineSpan.Slice(0, index).ToString();
                lineSpan = lineSpan.Slice(index + 1);

                string tagName = lineSpan.ToString();

                lock (tags)
                {
                    tags[tagId] = new Tag(tagName, tagId);
                }
            }
        }
        private void ReadTagScores_MovieLens(BlockingCollection<string> lines)
        {
            string filePath = "..\\..\\..\\ml-latest\\TagScores_MovieLens.csv";
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                reader.ReadLine();
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
                lines.CompleteAdding();
            }
        }
        private void ProcessTagScores_MovieLens(BlockingCollection<string> lines)
        {
            foreach (var line in lines.GetConsumingEnumerable())
            {
                string[] fields = line.Split(',');
                string movieId = fields[0];
                string tagId = fields[1];
                string relevanceString = fields[2];
                if (relevanceString.Length > 4)
                {
                    relevanceString = relevanceString.Substring(0, 4);
                }
                double relevance = Convert.ToDouble(relevanceString, CultureInfo.InvariantCulture);
                if (relevance > 0.5 && movies.ContainsKey(movieId) && tags.ContainsKey(tagId))
                {
                    Tag tag = tags[tagId];
                    Movie movie = movies[movieId];

                    lock (movie)
                    {
                        movie.Tags.Add(tag);
                    }

                    if (!tagsMovies.ContainsKey(tag))
                    {
                        lock (tagsMovies)
                        {
                            tagsMovies[tag] = new();
                        }
                    }
                    lock (tagsMovies)
                    {
                        tagsMovies[tag].Add(movies[movieId]);
                    }
                }
            }
        }
        private Task[] StartProcess(Action<BlockingCollection<string>> read,Action<BlockingCollection<string>> process)
        {
            BlockingCollection<string> lines = new();

            int n = Environment.ProcessorCount;

            Task[] tasks = new Task[n];

            tasks[0] = Task.Factory.StartNew(() => read(lines), TaskCreationOptions.LongRunning);

            for (int i = 1; i < n; i++)
            {
                tasks[i] = Task.Factory.StartNew(() => process(lines), TaskCreationOptions.LongRunning);
            }
            Console.WriteLine(read.Method.Name + " and " + process.Method.Name + " is started");
            return tasks;
        }
        public void ReadInfo()
        {
            Task[] tasks = StartProcess(ReadMovieCodes_IMDB, ProcessMovieCodes_IMDB);
            Task.WaitAll(tasks);

            tasks = StartProcess(ReadActorsDirectorsNames_IMDB, ProcessActorsDirectorsNames_IMDB);
            Task.WaitAll(tasks);

            tasks = StartProcess(ReadTagCodes_MovieLens, ProcessTagCodes_MovieLens);
            Task.WaitAll(tasks);

            tasks = StartProcess(ReadRatings_IMDB, ProcessRatings_IMDB);
            Task.WaitAll(tasks);

            tasks = StartProcess(Readlinks_IMDB_MovieLens, Processlinks_IMDB_MovieLens);
            Task.WaitAll(tasks);

            tasks = StartProcess(ReadActorsDirectorsCodes_IMDB, ProcessActorsDirectorsCodes_IMDB);
            Task.WaitAll(tasks);

            tasks = StartProcess(ReadTagScores_MovieLens, ProcessTagScores_MovieLens);
            Task.WaitAll(tasks);

            Console.Write("end reading and processing");
        }
    }
}
