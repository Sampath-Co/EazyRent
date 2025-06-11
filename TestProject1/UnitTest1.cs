using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using EazyRent.Controllers;
using EazyRent.Models.Repositories;
using EazyRent.Models.DTO;
using System.Threading.Tasks;
using System.Collections.Generic;
using EazyRent.Models.Domains;
using EazyRent.Data;
using EazyRent.Models.Services;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace TestProject1
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

        }

        [Fact]
        public async Task Test_GetAllProperties_TenantController()
        {
            // Arrange
            var mockPropertyRepo = new Mock<IProperty>();
            mockPropertyRepo.Setup(repo => repo.GetAllPropertiesAsync(null, null, null))
                .ReturnsAsync(new List<PropertyDetailsDTO> { new PropertyDetailsDTO { Address = "123 Main St", RentAmount = 1000 } });

            var controller = new TenantController(mockPropertyRepo.Object);

            // Act
            var result = await controller.GetAllProperties(null, null, null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var properties = Assert.IsType<List<PropertyDetailsDTO>>(okResult.Value);
            Assert.Single(properties);
            Assert.Equal("123 Main St", properties[0].Address);
        }

        [Fact]
        public async Task Test_AddProperty_PropertyController()
        {
            // Arrange
            var mockPropertyRepo = new Mock<IProperty>();
            mockPropertyRepo.Setup(repo => repo.AddPropertyAsync("owner@example.com", It.IsAny<PropertyDetailsDTO>()))
                .ReturnsAsync(true);

            var controller = new PropertyController(mockPropertyRepo.Object);
            var propertyDto = new PropertyDetailsDTO { Address = "456 Elm St", RentAmount = 1500 };

            // Act
            var result = await controller.AddProperty(propertyDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Property added successfully", okResult.Value);
        }

        [Fact]
        public async Task Test_RequestLease_LeaseController()
        {
            // Arrange
            var mockLeaseRepo = new Mock<ILease>();
            mockLeaseRepo.Setup(repo => repo.CreateLeaseRequestAsync(1, It.IsAny<CreateLeaseDTO>()))
                .ReturnsAsync(new Lease { LeaseId = 1, PropertyId = 1, TenantId = 1 });

            var controller = new LeaseController(mockLeaseRepo.Object);
            var leaseDto = new CreateLeaseDTO { PropertyId = 1, StartDate = new DateOnly(2025, 6, 10), EndDate = new DateOnly(2025, 6, 20) };

            // Act
            var result = await controller.RequestLease(leaseDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Lease Created", okResult.Value);
        }

        [Fact]
        public async Task Test_RegisterOwner_UserController()
        {
            // Arrange
            var mockDbContext = new Mock<RentalDBContext>();
            var mockJwtTokenService = new Mock<IJwtTokenService>();
            var mockMapper = new Mock<IMapper>();

            var userDto = new RegistrationDTO
            {
                FullName = "John Doe",
                Email = "john.doe@example.com",
                Password = "password123",
                PhoneNumber = "1234567890",
                Role = "Owner"
            };

            var user = new User
            {
                UserId = 1,
                FullName = "John Doe",
                Email = "john.doe@example.com",
                PasswordHash = "hashedpassword",
                PhoneNumber = "1234567890",
                Role = "Owner"
            };

            mockMapper.Setup(m => m.Map<User>(userDto)).Returns(user);
            mockDbContext.Setup(db => db.Users.Add(user));
            mockDbContext.Setup(db => db.SaveChangesAsync(default)).ReturnsAsync(1);

            var controller = new UserController(mockDbContext.Object, mockJwtTokenService.Object, mockMapper.Object);

            // Act
            var result = await controller.RegisterOwner(userDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Owner registered", okResult.Value);
        }

        [Fact]
        public void Test_Login_UserController_Fixed()
        {
            // Arrange
            var mockDbContext = new Mock<RentalDBContext>();
            var mockJwtTokenService = new Mock<IJwtTokenService>();
            var mockMapper = new Mock<IMapper>();

            var loginDto = new LoginDTO
            {
                Email = "john.doe@example.com",
                Password = "password123"
            };

            var user = new User
            {
                UserId = 1,
                FullName = "John Doe",
                Email = "john.doe@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                Role = "Owner"
            };

            var userList = new List<User> { user }.AsQueryable();
            var mockUserDbSet = new Mock<DbSet<User>>();
            mockUserDbSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(userList.Provider);
            mockUserDbSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(userList.Expression);
            mockUserDbSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(userList.ElementType);
            mockUserDbSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(userList.GetEnumerator());
            mockDbContext.Setup(db => db.Users).Returns(mockUserDbSet.Object);
            mockJwtTokenService.Setup(jwt => jwt.GenerateJwtToken(user.Email, user.Role, user.UserId)).Returns("mockedToken");

            var controller = new UserController(mockDbContext.Object, mockJwtTokenService.Object, mockMapper.Object);

            // Act
            var result = controller.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var tokenResponse = okResult.Value;
            Assert.NotNull(tokenResponse);
            Assert.Equal("mockedToken", tokenResponse.GetType().GetProperty("token")?.GetValue(tokenResponse)?.ToString());
        }
    }
}