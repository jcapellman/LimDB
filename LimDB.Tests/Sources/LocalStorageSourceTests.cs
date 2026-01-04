using Microsoft.VisualStudio.TestTools.UnitTesting;
using LimDB.lib;
using LimDB.lib.Sources;
using LimDB.Tests.Objects;

namespace LimDB.Tests.Sources
{
    [TestClass]
    public class LocalStorageSourceTests
    {
        private readonly string _testDbName = Path.Combine(AppContext.BaseDirectory, $"Data{Path.DirectorySeparatorChar}", "db.file");
        private readonly List<string> _tempFiles = new();

        private string CreateTempDb(string? content = null)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"LimDb_{Guid.NewGuid():N}.json");

            if (content is null)
            {
                File.Copy(_testDbName, tempPath, true);
            }
            else
            {
                File.WriteAllText(tempPath, content);
            }

            _tempFiles.Add(tempPath);

            return tempPath;
        }

        [TestCleanup]
        public void CleanupTempFiles()
        {
            foreach (var file in _tempFiles)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
            }

            _tempFiles.Clear();
        }

        [TestMethod]
        public async Task ValidFile()
        {
            var lss = new LocalStorageSource(_testDbName);

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
            var dbContext = await LimDbContext<Posts>.CreateFromLocalStorageSourceAsync(_testDbName);

            var posts = dbContext.GetMany();

            Assert.IsNotNull(posts);
            Assert.IsTrue(posts.Any());
        }

        [TestMethod]
        public async Task ValidFile_InsertAndDeleteTest()
        {
            var lss = new LocalStorageSource(_testDbName);

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
            var lss = new LocalStorageSource(_testDbName);

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
        public async Task InsertAsync_AssignsIdWhenDbEmpty()
        {
            var tempDb = CreateTempDb("[]");

            var dbContext = await LimDbContext<Posts>.CreateFromLocalStorageSourceAsync(tempDb);

            var post = new Posts
            {
                Title = "Empty Insert",
                Body = "Body",
                Category = "General",
                URL = "https://example.com",
                PostDate = DateTime.UtcNow
            };

            var newId = await dbContext.InsertAsync(post);

            Assert.AreEqual(1, newId);

            var reloadContext = await LimDbContext<Posts>.CreateFromLocalStorageSourceAsync(tempDb);
            var storedPost = reloadContext.GetOneById(1);

            Assert.IsNotNull(storedPost);
            Assert.AreEqual(post.Title, storedPost.Title);
        }

        [TestMethod]
        public async Task GetMany_ReturnsCopyOfData()
        {
            var tempDb = CreateTempDb();
            var dbContext = await LimDbContext<Posts>.CreateFromLocalStorageSourceAsync(tempDb);

            var firstCall = dbContext.GetMany()?.ToList();

            Assert.IsNotNull(firstCall);
            Assert.IsTrue(firstCall.Count > 0);

            firstCall.Clear();

            var secondCall = dbContext.GetMany();

            Assert.IsNotNull(secondCall);
            Assert.IsTrue(secondCall.Any());
        }

        [TestMethod]
        public async Task InitializeAsync_DeserializesCamelCaseJson()
        {
            var camelCaseJson = """
[
  {
    "id": 1,
    "title": "Camel Case",
    "body": "Body",
    "category": "general",
    "url": "https://example.com",
    "postDate": "2024-01-01T00:00:00Z",
    "active": true,
    "created": "2024-01-01T00:00:00Z",
    "modified": "2024-01-01T00:00:00Z"
  }
]
""";

            var tempDb = CreateTempDb(camelCaseJson);
            var dbContext = await LimDbContext<Posts>.CreateFromLocalStorageSourceAsync(tempDb);

            var post = dbContext.GetOne();

            Assert.IsNotNull(post);
            Assert.AreEqual("Camel Case", post.Title);
        }

        [TestMethod]
        public async Task InsertAsync_IsThreadSafe()
        {
            var tempDb = CreateTempDb("[]");
            var dbContext = await LimDbContext<Posts>.CreateFromLocalStorageSourceAsync(tempDb);

            var insertTasks = Enumerable.Range(0, 10)
                .Select(i => dbContext.InsertAsync(new Posts
                {
                    Title = $"Threaded {i}",
                    Body = "Body",
                    Category = "General",
                    URL = "https://example.com",
                    PostDate = DateTime.UtcNow.AddMinutes(i)
                }))
                .ToArray();

            var ids = await Task.WhenAll(insertTasks);

            Assert.IsTrue(ids.All(id => id.HasValue));
            Assert.AreEqual(10, ids.Select(id => id!.Value).Distinct().Count());

            var reloadContext = await LimDbContext<Posts>.CreateFromLocalStorageSourceAsync(tempDb);
            var posts = reloadContext.GetMany();

            Assert.IsNotNull(posts);
            Assert.AreEqual(10, posts.Count());
        }

        [TestMethod]
        public async Task InvalidFile()
        {
            var lss = new LocalStorageSource("testo.file");

            try
            {
                _ = await LimDbContext<Posts>.CreateAsync(lss);
                Assert.Fail("Expected FileNotFoundException was not thrown.");
            }
            catch (FileNotFoundException)
            {
                // success, expected exception thrown
            }
        }
    }
}