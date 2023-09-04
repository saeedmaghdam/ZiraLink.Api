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
            List<Customer> customerList = new List<Customer>();
            int countCustomer = 3;
            for (int i = 1; i <= countCustomer; i++)
            {
                customerList.Add(new Customer
                {
                    ViewId = Guid.NewGuid(),
                    Username = $"TestUser{i}",
                    Email = $"TestUser{i}@ZiraLink.com",
                    Name = $"TestName{i}",
                    Family = $"User{i}",
                    ExternalId = i.ToString()
                });
            }

            AppMemoryDbContext.Customers.AddRange(customerList);

            // Add new project 
            List<Project> projectList = new List<Project>();
            for (int i = 1; i <= customerList.Count; i++)
            {
                projectList.Add(
                   new Project
                   {
                       ViewId = new Guid(),
                       Customer = customerList[i-1],
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

            AppMemoryDbContext.Customers.Add(new Customer
            {
                ViewId = Guid.NewGuid(),
                Username = $"TestUser{countCustomer + 1}",
                Email = $"TestUser{countCustomer + 1}@ZiraLink.com",
                Name = $"TestName{countCustomer + 1}",
                Family = $"User{countCustomer + 1}",
                ExternalId = (countCustomer + 1).ToString()
            });

            AppMemoryDbContext.SaveChangesAsync();

        }

    }
}
