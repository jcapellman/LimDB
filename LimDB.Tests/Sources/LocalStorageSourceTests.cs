using LimDB.lib;
using LimDB.lib.Sources;
using LimDB.Tests.Objects;

namespace LimDB.Tests.Sources
{
    [TestClass]
    public class LocalStorageSourceTests
    {
        private string TestDBName = Path.Combine(AppContext.BaseDirectory, $"Data{Path.DirectorySeparatorChar.ToString()}", "db.file");

        [TestMethod]
        public async Task ValidFile()
        {
            var lss = new LocalStorageSource(TestDBName);

            var dbContext = await LimDbContext<Posts>.CreateAsync(lss);

            var posts = dbContext.GetMany();

            Assert.IsNotNull(posts);
            Assert.IsTrue(posts.Any());

            var singlePost = dbContext.GetOne();

            Assert.IsNotNull(singlePost);

            var id = singlePost.Id;

            singlePost = dbContext.GetOneById(id);

            Assert.IsNotNull(singlePost);

            posts = dbContext.GetMany(a => a.Active && a.Modified > DateTime.MinValue && a.Created > DateTime.MinValue);

            Assert.IsNotNull(posts);
            Assert.IsTrue(posts.Any());
        }

        [TestMethod]
        public async Task WrapperValidFile()
        {
            var dbContext = await LimDbContext<Posts>.CreateFromLocalStorageSourceAsync(TestDBName);

            var posts = dbContext.GetMany();

            Assert.IsNotNull(posts);
            Assert.IsTrue(posts.Any());
        }

        [TestMethod]
        public async Task ValidFile_InsertAndDeleteTest()
        {
            var lss = new LocalStorageSource(TestDBName);

            var dbContext = await LimDbContext<Posts>.CreateAsync(lss);

            var post = dbContext.GetOne();

            Assert.IsNotNull(post);
            
            var id = post.Id;

            var result = await dbContext.DeleteByIdAsync(id);

            Assert.IsTrue(result);

            var newId = await dbContext.InsertAsync(post);

            Assert.IsNotNull(newId);

            Assert.IsTrue(id != newId.Value);
        }

        [TestMethod]
        public async Task ValidFile_UpdateTest()
        {
            var lss = new LocalStorageSource(TestDBName);

            var dbContext = await LimDbContext<Posts>.CreateAsync(lss);

            var newPost = new Posts
            {
                Title = "TestDo " + DateTime.Now,
                Body = "Testing",
                Category = "test",
                PostDate = DateTime.Now,
                URL = "test"
            };

            var newId = await dbContext.InsertAsync(newPost);

            Assert.IsNotNull(newId);

            var copyPost = dbContext.GetOneById(newId.Value);

            Assert.IsNotNull(copyPost);

            copyPost.Title = "Wrongo";

            var result = await dbContext.UpdateAsync(copyPost);

            Assert.IsTrue(result);

            var retPost = dbContext.GetOneById(newId.Value);

            Assert.IsNotNull(retPost);

            Assert.AreEqual(copyPost.Title, retPost.Title);
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