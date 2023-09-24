using Microsoft.EntityFrameworkCore;

using ZiraLink.Api.Application;
using ZiraLink.Domain;
using ZiraLink.Domain.Enums;
namespace ZiraLink.Api.Application.UnitTests.Tools
{
    public class TestTools
    {

        public AppDbContext AppMemoryDbContext;

        /// <summary>
        /// Initialization
        /// </summary>
        public void Initialize(string testClassName)
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            dbContextOptionsBuilder.UseInMemoryDatabase($"AppDbContext_{testClassName}");
            DbContextOptions<AppDbContext>? contextOptions = dbContextOptionsBuilder.Options;
            AppMemoryDbContext = new AppDbContext(contextOptions);
            SeedData();
        }

        /// <summary>
        /// Initializing new data
        /// </summary>
        public void SeedData()
        {
            List<Customer> customerList = new List<Customer>();
            int countCustomer = 3;
            if (!AppMemoryDbContext.Customers.Any())
            {
                // Add new customer

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
            }

            if (!AppMemoryDbContext.Projects.Any())
            {
                // Add new project  
                for (int i = 1; i <= customerList.Count; i++)
                {
                    AppMemoryDbContext.Projects.Add(
                       new Project
                       {
                           ViewId = Guid.NewGuid(),
                           Customer = customerList[i - 1],
                           Title = $"TestTitle{i}",
                           DomainType = DomainType.Default,
                           Domain = $"TestDomain{i}",
                           InternalUrl = $"http://localhost:300{i}",
                           DateCreated = DateTime.Now,
                           DateUpdated = DateTime.Now,
                           State = ProjectState.Active,
                       }
                   );
                } 
            }
            if (!AppMemoryDbContext.AppProjects.Any())
            {
                // Add new project  
                for (int i = 1; i <= customerList.Count; i++)
                {
                    AppMemoryDbContext.AppProjects.Add(
                       new AppProject
                       {
                           ViewId = Guid.NewGuid(),
                           Customer = customerList[i - 1],
                           Title = $"TestTitle{i}",
                           AppProjectViewId = Guid.NewGuid(),
                           AppProjectType = AppProjectType.SharePort,
                           InternalPort = 2020,
                           DateCreated = DateTime.Now,
                           DateUpdated = DateTime.Now,
                           State = ProjectState.Active,
                       }
                   );
                } 
            }

            if (!AppMemoryDbContext.Customers.Any(x => x.ExternalId == (countCustomer + 1).ToString()))
            {
                AppMemoryDbContext.Customers.Add(new Customer
                {
                    ViewId = Guid.NewGuid(),
                    Username = $"TestUser{countCustomer + 1}",
                    Email = $"TestUser{countCustomer + 1}@ZiraLink.com",
                    Name = $"TestName{countCustomer + 1}",
                    Family = $"User{countCustomer + 1}",
                    ExternalId = (countCustomer + 1).ToString()
                });
            }
            AppMemoryDbContext.SaveChangesAsync();
            /*
            row 1 : readonly
            row 2 : delete
            row 3 : patch
            */
        }

    }
}
