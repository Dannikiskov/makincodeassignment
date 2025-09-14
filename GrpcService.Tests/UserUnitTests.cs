using GrpcService.Data;
using Microsoft.EntityFrameworkCore;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;

namespace GrpcService.Tests;

public class UserServiceTests
{
    private static AppDbContext GetInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateUser_ShouldReturnUser()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new GrpcService.Services.UserService(db);

        var request = new CreateUserRequest
        {
            Name = "Alice",
            Birthday = Timestamp.FromDateTime(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
        };

        // Act
        var response = await service.CreateUser(request, null!);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Alice", response.Name);
        Assert.Equal(request.Birthday, response.Birthday);
        Assert.NotNull(response.Uuid);
    }
    
    [Fact]
    public async Task GetUser_ShouldReturnUser_WhenExists()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new GrpcService.Services.UserService(db);
        var user = new Data.Entities.UserEntity
        {
            Name = "Bob",
            Birthday = new DateTime(1980, 1, 1)
        };
        
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var request = new GetUserRequest { Uuid = user.Uuid };

        // Act
        var response = await service.GetUser(request, null!);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(user.Name, response.Name);
        Assert.Equal(Timestamp.FromDateTime(user.Birthday.ToUniversalTime()), response.Birthday);
        Assert.Equal(user.Uuid, response.Uuid);
    }
    
    [Fact]
    public async Task UpdateUser_ShouldUpdateName()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new GrpcService.Services.UserService(db);
        var user = new Data.Entities.UserEntity
        {
            Name = "Charlie",
            Birthday = new DateTime(1990, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var request = new UpdateUserRequest
        {
            Uuid = user.Uuid,
            Name = "CharlieUpdated"
        };

        // Act
        var response = await service.UpdateUser(request, null!);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("CharlieUpdated", response.Name);
        Assert.Equal(user.Birthday, response.Birthday.ToDateTime());
    }

    [Fact]
    public async Task DeleteUser_ShouldReturnSuccess()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new GrpcService.Services.UserService(db);
        var user = new Data.Entities.UserEntity
        {
            Name = "Dana",
            Birthday = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };
        
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var request = new DeleteUserRequest { Uuid = user.Uuid };

        // Act
        var response = await service.DeleteUser(request, null!);

        // Assert
        Assert.True(response.Success);
        Assert.Empty(db.Users);
    }
    
    [Fact]
    public async Task GetUserWithNotes_ShouldReturnUserWithNotes()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new GrpcService.Services.UserService(db);

        var user = new Data.Entities.UserEntity
        {
            Name = "Eve",
            Birthday = new DateTime(1995, 1, 1)
        };
        
        var note1 = new Data.Entities.NoteEntity
        {
            Headline = "Note1",
            Text = "Text1",
            User = user
        };
        
        var note2 = new Data.Entities.NoteEntity
        {
            Headline = "Note2",
            Text = "Text2",
            User = user
        };
        
        user.Notes.Add(note1);
        user.Notes.Add(note2);
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var request = new GetUserRequest { Uuid = user.Uuid };

        // Act
        var response = await service.GetUserWithNotes(request, null!);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(user.Name, response.Name);
        Assert.Equal(note1.Headline, response.Notes[0].Headline);
        Assert.Equal(note1.Text, response.Notes[0].Text);
        Assert.Equal(note2.Headline, response.Notes[1].Headline);
        Assert.Equal(note2.Text, response.Notes[1].Text);
    }
    
    [Fact]
    public async Task GetUser_ShouldThrowIfNotFound()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new GrpcService.Services.UserService(db);
        var request = new GetUserRequest { Uuid = "non-existent" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.GetUser(request, null!));
        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_ShouldThrowIfNotFound()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new GrpcService.Services.UserService(db);

        var request = new UpdateUserRequest
        {
            Uuid = "non-existent",
            Name = "NewName"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.UpdateUser(request, null!));
        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_ShouldThrowIfNotFound()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new GrpcService.Services.UserService(db);
        var request = new DeleteUserRequest { Uuid = "non-existent" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RpcException>(() =>
            service.DeleteUser(request, null!));
        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }
    
    [Fact]
    public async Task CreateUser_ShouldThrowIfNameIsInvalid()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new GrpcService.Services.UserService(db);
        
        var request = new CreateUserRequest
        {
            Name = "",
            Birthday = null
        };

        // Act
        var ex = await Assert.ThrowsAsync<RpcException>(() => service.CreateUser(request, null!));
        
        // Assert
        Assert.Equal(StatusCode.InvalidArgument, ex.StatusCode);
    }
}
