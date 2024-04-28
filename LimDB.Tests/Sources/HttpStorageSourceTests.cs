using LimDB.lib.Sources;
using LimDB.lib;
using LimDB.Tests.Objects;

namespace LimDB.Tests.Sources
{
    [TestClass]
    public class HttpStorageSourceTests
    {
        [TestMethod]
        public async Task ValidFile()
        {
            var hss = new HttpStorageSource("https://github.com/jcapellman/LimDB/LimDB.Tests/Data/db.file");

            var dbContext = await LimDbContext<Posts>.CreateAsync(hss);

            var posts = dbContext.GetMany();

            Assert.IsNotNull(posts);
            Assert.IsTrue(posts.Any());
        }
    }
}   