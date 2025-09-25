using API.Commons;
using API.Models;
using API.Services;
using API.Services.Interfaces;
using API.ViewModels;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace API.Tests
{
    public class ClassesServiceTests
    {
        private readonly Sep490Context _context;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<ILog> _mockLogger;
        private readonly ClassesService _service;

        public ClassesServiceTests()
        {
            var options = new DbContextOptionsBuilder<Sep490Context>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new Sep490Context(options);
            _mockMapper = new Mock<IMapper>();
            _mockLogger = new Mock<ILog>();
            _service = new ClassesService(_context, _mockMapper.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task GetAllClasses_NoClassesFound_ReturnsErrorMessage()
        {
            // Arrange
            var search = new SearchClassVM { CurrentPage = 1, PageSize = 10 };
            _mockMapper.Setup(m => m.Map<List<ClassVM>>(It.IsAny<List<Class>>())).Returns(new List<ClassVM>());

            // Act
            var (message, result) = await _service.GetAllClasses(search);

            // Assert
            Assert.Equal("No classes found.", message);
            Assert.Null(result);
        }
        [Fact]
        public async Task GetList_WithValidSearch_ReturnsSearchResult()
        {
            // Arrange
            var search = new SearchClassVM { CurrentPage = 1, PageSize = 10, TextSearch = "Test" };
            var classEntity = new Class { ClassId = "1", ClassCode = "TestClass", CreatedBy = "TestUser" };
            var classVM = new ClassVM { ClassId = "1", ClassCode = "TestClass" };

            // Seed data into in-memory database
            _context.Classes.Add(classEntity);
            await _context.SaveChangesAsync();

            // Mock the mapper to return a list of ClassVM
            _mockMapper.Setup(m => m.Map<List<ClassVM>>(It.IsAny<List<Class>>()))
                       .Returns(new List<ClassVM> { classVM });

            // Act
            var (message, result) = await _service.GetAllClasses(search);

            // Assert
            Assert.Empty(message); // Check message is empty
            Assert.NotNull(result); // Ensure result is not null
            var classes = result?.Result as List<ClassVM>; // Safe cast to List<ClassVM>
            Assert.NotNull(classes); // Ensure cast was successful
            Assert.Single(classes); // Check that the list contains exactly one item
            Assert.Equal(classVM.ClassId, classes.First().ClassId); // Verify ClassId matches
        }

        [Fact]
        public async Task GetClassById_NullId_ReturnsErrorMessage()
        {
            // Act
            var (message, result) = await _service.GetClassById(null);

            // Assert
            Assert.Equal("Class ID cannot be null or empty.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetClassById_ClassNotFound_ReturnsErrorMessage()
        {
            // Act
            var (message, result) = await _service.GetClassById("1");

            // Assert
            Assert.Equal("Class not found.", message);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetClassById_ValidId_ReturnsClassVM()
        {
            // Arrange
            var classEntity = new Class { ClassId = "1", ClassCode = "TestClass", CreatedBy = "TestUser" };
            var classVM = new ClassVM { ClassId = "1", ClassCode = "TestClass" };

            // Seed data into in-memory database
            _context.Classes.Add(classEntity);
            await _context.SaveChangesAsync();

            _mockMapper.Setup(m => m.Map<ClassVM>(classEntity)).Returns(classVM);

            // Act
            var (message, result) = await _service.GetClassById("1");

            // Assert
            Assert.Empty(message);
            Assert.Equal(classVM, result);
        }

        [Fact]
        public async Task DoDeactivateClass_NullId_ReturnsErrorMessage()
        {
            // Act
            var message = await _service.DoDeactivateClass(null, "userToken");

            // Assert
            Assert.Equal("Class ID cannot be null or empty.", message);
        }

        [Fact]
        public async Task DoDeactivateClass_ClassNotFound_ReturnsErrorMessage()
        {
            // Act
            var message = await _service.DoDeactivateClass("1", "userToken");

            // Assert
            Assert.Equal("Class not found.", message);
        }

        [Fact]
        public async Task DoDeactivateClass_ValidId_DeactivatesClass()
        {
            // Arrange
            var classEntity = new Class { ClassId = "2", ClassCode = "TestClass", IsActive = true, CreatedBy = "TestUser" };
            _context.Classes.Add(classEntity);
            await _context.SaveChangesAsync();

            _mockLogger.Setup(l => l.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");

            // Act
            var message = await _service.DoDeactivateClass("2", "userToken");

            // Assert
            Assert.Empty(message);
            var updatedClass = await _context.Classes.FindAsync("2");
            Assert.False(updatedClass.IsActive);
        }

        [Fact]
        public async Task DoCreateUpdateClass_NullInput_ReturnsErrorMessage()
        {
            // Act
            var message = await _service.DoCreateUpdateClass(null, "userToken");

            // Assert
            Assert.Equal("Data input cannot be null.", message);
        }

        [Fact]
        public async Task DoCreateUpdateClass_CreateNewClass_Success()
        {
            // Arrange
            var input = new CreateUpdateClassVM
            {
                ClassCode = "TestClass",
                Description = "Test",
                IsActive = true,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            };
            _mockLogger.Setup(l => l.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");

            // Act
            var message = await _service.DoCreateUpdateClass(input, "userToken");

            // Assert
            Assert.Empty(message);
            var createdClass = await _context.Classes.FirstOrDefaultAsync(c => c.ClassCode == "TestClass");
            Assert.NotNull(createdClass);
            Assert.Equal(input.ClassCode, createdClass.ClassCode);
            Assert.Equal(input.Description, createdClass.Description);
            Assert.Equal(input.IsActive, createdClass.IsActive);
        }

        [Fact]
        public async Task DoCreateUpdateClass_UpdateExistingClass_Success()
        {
            // Arrange
            var classEntity = new Class { ClassId = "1", ClassCode = "OldClass", Description = "Old", IsActive = true, CreatedBy = "TestUser" };
            _context.Classes.Add(classEntity);
            await _context.SaveChangesAsync();

            var input = new CreateUpdateClassVM
            {
                ClassId = "1",
                ClassCode = "TestClass",
                Description = "Test",
                IsActive = false,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            };
            _mockLogger.Setup(l => l.WriteActivity(It.IsAny<AddUserLogVM>())).ReturnsAsync("");

            // Act
            var message = await _service.DoCreateUpdateClass(input, "userToken");

            // Assert
            Assert.Empty(message);
            var updatedClass = await _context.Classes.FindAsync("1");
            Assert.Equal(input.ClassCode, updatedClass.ClassCode);
            Assert.Equal(input.Description, updatedClass.Description);
            Assert.Equal(input.IsActive, updatedClass.IsActive);
        }

        [Fact]
        public async Task DoCreateUpdateClass_DuplicateClassCode_ReturnsErrorMessage()
        {
            // Arrange
            var existingClass = new Class { ClassId = "1", ClassCode = "TestClass", CreatedBy = "TestUser" };
            _context.Classes.Add(existingClass);
            await _context.SaveChangesAsync();

            var input = new CreateUpdateClassVM
            {
                ClassCode = "TestClass",
                Description = "Test",
                IsActive = true,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            // Act
            var message = await _service.DoCreateUpdateClass(input, "userToken");

            // Assert
            Assert.Equal("This ClassCode is already in use. Please enter a different one.", message);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}