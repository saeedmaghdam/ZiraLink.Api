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
            List<Customer> customerList = new List<Customer> {
                new Customer
                    {
                        ViewId = Guid.NewGuid(),
                        Username = "TestUser",
                        Email = "TestUser@ZiraLink.com",
                        Name = "Test",
                        Family = "User",
                        ExternalId = "1"
                    },
                    new Customer
                    {
                        ViewId = Guid.NewGuid(),
                        Username = "TestUser2",
                        Email = "TestUser2@ZiraLink.com",
                        Name = "Test2",
                        Family = "User2",
                        ExternalId = "2"
                    }
                };

            AppMemoryDbContext.Customers.AddRange(customerList);

            AppMemoryDbContext.Customers.Add(new Customer
                    {
                        ViewId = Guid.NewGuid(),
                        Username = "TestUser3",
                        Email = "TestUser3@ZiraLink.com",
                        Name = "Test3",
                        Family = "User3",
                        ExternalId = "3"
                    });

            // Add new project 
            List<Project> projectList = new List<Project>();
            for (int i = 0; i < customerList.Count; i++)
            {
                 projectList.Add(
                    new Project
                    {
                        ViewId = new Guid(),
                        Customer = customerList[i],
                        Title = $"Test{i}",
                        DomainType = Domain.Enums.DomainType.Default,
                        Domain = $"Test{i}",
                        InternalUrl = $"http://localhost:300{i}",
                        DateCreated = DateTime.Now,
                        DateUpdated = DateTime.Now,
                        State = ProjectState.Active,
                    }
                );
            }
             
            AppMemoryDbContext.Projects.AddRange(projectList);
  
            AppMemoryDbContext.SaveChangesAsync();

        }

    }
}
