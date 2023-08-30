using Microsoft.EntityFrameworkCore;

using ZiraLink.Api.Application;
using ZiraLink.Domain;
using ZiraLink.Domain.Enums;
namespace ZiraLink.Api.Application.UnitTests.Tools
{
    public class TestTools
    {
         
        public static AppDbContext AppMemoryDbContext;

        /// <summary>
        /// Initialization
        /// </summary>
        public static void Initialize()
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            dbContextOptionsBuilder.UseInMemoryDatabase("AppDbContext");
            DbContextOptions<AppDbContext>? contextOptions = dbContextOptionsBuilder.Options;
            AppMemoryDbContext = new AppDbContext(contextOptions);
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
                    Id = 1,
                    ViewId = Guid.NewGuid(),
                    Username = "TestUser",
                    Email = "TestUser@ZiraLink.com",
                    Name = "Test",
                    Family = "User",
                    ExternalId = "1"
                };
                AppMemoryDbContext.Customers.Add(customer);
             

                // Add new customer
                Project project = new Project
                {
                    Id = 1,
                    ViewId = new Guid(),
                    CustomerId = 1,
                    Title = "Test",
                    DomainType = Domain.Enums.DomainType.Default,
                    Domain = "Test",
                    InternalUrl = "http://test.com",
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    State = ProjectState.Active,
                };
                 AppMemoryDbContext.Projects.AddAsync(project);
                 AppMemoryDbContext.SaveChangesAsync();
             
        }

    }
}
