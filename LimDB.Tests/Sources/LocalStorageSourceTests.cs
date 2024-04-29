using LimDB.lib;
using LimDB.lib.Sources;
using LimDB.Tests.Objects;

namespace LimDB.Tests.Sources
{
    [TestClass]
    public class LocalStorageSourceTests
    {
        [TestMethod]
        public async Task ValidFile()
        {
            var lss = new LocalStorageSource(Path.Combine(AppContext.BaseDirectory, @"Data\", "db.file"));

            var dbContext = await LimDbContext<Posts>.CreateAsync(lss);

            var posts = dbContext.GetMany();

            Assert.IsNotNull(posts);
            Assert.IsTrue(posts.Any());
        }

        [TestMethod]
        public async Task WrapperValidFile()
        {
            var dbContext = await LimDbContext<Posts>.CreateFromLocalStorageSourceAsync(Path.Combine(AppContext.BaseDirectory, @"Data\", "db.file"));

            var posts = dbContext.GetMany();

            Assert.IsNotNull(posts);
            Assert.IsTrue(posts.Any());
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public async Task InvalidFile()
        {
            var lss = new LocalStorageSource("testo.file");

            var _ = await LimDbContext<Posts>.CreateAsync(lss);
        }
    }
}