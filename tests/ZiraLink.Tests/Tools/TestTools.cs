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
        public static AppDbContext? dbContext;

        /// <summary>
        /// Initialization
        /// </summary>
        public static void Initialize()
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            dbContextOptionsBuilder.UseInMemoryDatabase("AppDbContext");
            contextOptions = dbContextOptionsBuilder.Options;
            dbContext = new AppDbContext(contextOptions);
            SeedData();
        }

        /// <summary>
        /// Initializing new data
        /// </summary>
        public static void SeedData()
        {

            // Add new customer

            List<Project> projects = new() {
                new Project {
                    Id = 1,
                    ViewId = new Guid(),
                    CustomerId = 1,
                    Title="Test",
                    DomainType = Domain.Enums.DomainType.Default,
                    Domain = "Test",
                    InternalUrl = "http://test.com",
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now,
                    State  = ProjectState.Active,

    }
    };
            dbContext.Projects.AddRange(projects);

            dbContext.SaveChanges();
        }

    }
}
