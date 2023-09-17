﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using ZiraLink.Domain;
using ZiraLink.Domain.Enums;

namespace ZiraLink.Api.Application.UnitTests
{
    public class TestInitalizeFixture : IDisposable
    {
        public AppDbContext AppMemoryDbContext;
        public TestInitalizeFixture()
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
                List<Project> projectList = new List<Project>();
                for (int i = 1; i <= customerList.Count; i++)
                {
                    projectList.Add(
                       new Project
                       {
                           ViewId = new Guid(),
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

                AppMemoryDbContext.Projects.AddRange(projectList);
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

        public void Dispose()
        {
            // clean up test data from the database
        }
    }
}