using Microsoft.EntityFrameworkCore;

using ZiraLink.Api.Application;
using ZiraLink.Domain;
using ZiraLink.Domain.Enums;

namespace ZiraLink.UnitTests.Tools
{
    public class TestTools
    {

        public static DbContextOptions<AppDbContext>? _contextOptions;
        public static AppDbContext? _dbContext;
        public static long _customerId = 1;
        public static CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        /// <summary>
        /// Initialization
        /// </summary>
        public static void Initialize()
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            dbContextOptionsBuilder.UseInMemoryDatabase("AppDbContext");
            _contextOptions = dbContextOptionsBuilder.Options;
            _dbContext = new AppDbContext(_contextOptions);
            SeedData();
        }

        /// <summary>
        /// Initializing new data
        /// </summary>
        public static void SeedData()
        {

            // Add new customer
            var customer = new Customer
            {
                Id = _customerId,
                ViewId = Guid.NewGuid(),
                Username = "TestUser",
                Email = "TestUser@ZiraLink.com",
                Name = "Test",
                Family = "User",
                ExternalId = "1"
            };
             _dbContext.Customers.Add(customer);
             _dbContext.SaveChanges();


            // Add new customer
            Project project =  new Project {
                    Id = 1,
                    ViewId = new Guid(),
                    CustomerId = _customerId,
                    Title="Test",
                    DomainType = Domain.Enums.DomainType.Default,
                    Domain = "Test",
                    InternalUrl = "http://test.com",
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    State  = ProjectState.Active,
    };
            _dbContext.Projects.Add(project);

            _dbContext.SaveChanges();
        }

    }
}
