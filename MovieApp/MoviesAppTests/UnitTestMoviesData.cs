using MoviesData.Models;
using MoviesData;

namespace MoviesAppTests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GenerateSimilarForMovieTest()
        {
            Movie? movie = MoviesDb.getMovieById("103794");
            Assert.IsTrue(MoviesDb.GenerateSimilarForMovie(movie).Count() > 0);
        }
        [Test]
        public void SimilarityTest()
        {
            var terminatorM = MoviesDb.getMovieById("1240");
            var terminatorM2 = MoviesDb.getMovieById("120799");
            var rateMovie = MoviesDb.getMovieById("170705");
            double similar1 = terminatorM.Similarity(terminatorM2);
            double similar2 = terminatorM.Similarity(rateMovie);
            Assert.IsTrue(similar1 > similar2);
        }
    }
}