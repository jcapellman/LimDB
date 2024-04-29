using LimDB.lib.Sources;
using LimDB.lib;
using LimDB.Tests.Objects;

namespace LimDB.Tests.Sources
{
    [TestClass]
    public class HttpStorageSourceTests
    {
        private const string TestDbFileURL =
            "https://raw.githubusercontent.com/jcapellman/LimDb/main/LimDB.Tests/Data/db.file";

        [TestMethod]
        public async Task ValidFile()
        {
            var hss = new HttpStorageSource(TestDbFileURL);

            var dbContext = await LimDbContext<Posts>.CreateAsync(hss);

            var posts = dbContext.GetMany();

            Assert.IsNotNull(posts);
            Assert.IsTrue(posts.Any());
        }

        [TestMethod]
        public async Task WrapperValidFile()
        {
            var dbContext = await LimDbContext<Posts>.CreateFromHttpStorageSourceAsync(TestDbFileURL);

            var posts = dbContext.GetMany();

            Assert.IsNotNull(posts);
            Assert.IsTrue(posts.Any());
        }
    }
}