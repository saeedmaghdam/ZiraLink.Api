using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Moq;

using ZiraLink.Api.Application;
using ZiraLink.Domain;
using ZiraLink.Domain.Enums;

namespace ZiraLink.Tests.Tools
{
    public class TestTools
    {

        public static DbContextOptions<AppDbContext>? contextOptions;
        public static AppDbContext? _dbContext;
        public static long customerId = 1;

        /// <summary>
        /// Initialization
        /// </summary>
        public static void Initialize()
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            dbContextOptionsBuilder.UseInMemoryDatabase("AppDbContext");
            contextOptions = dbContextOptionsBuilder.Options;
            _dbContext = new AppDbContext(contextOptions);
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
                Id = customerId,
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
                    CustomerId = customerId,
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
