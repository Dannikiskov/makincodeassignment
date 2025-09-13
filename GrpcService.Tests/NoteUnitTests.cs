using Grpc.Core;
using GrpcService.Data;
using GrpcService.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Google.Protobuf.WellKnownTypes;

namespace GrpcService.Tests;

public class NoteServiceTests
{
    private static AppDbContext GetInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateNote_ShouldReturnNote()
    {
        // Arrange
        var db = GetInMemoryDb();
        var user = new UserEntity { Name = "Alice", Birthday = DateTime.UtcNow };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var service = new GrpcService.Services.NoteService(db);
        var request = new CreateNoteRequest
        {
            Headline = "Hello",
            Text = "World",
            UserUuid = user.Uuid
        };

        // Act
        var response = await service.CreateNote(request, null!);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Hello", response.Headline);
        Assert.Equal("World", response.Text);
        Assert.Equal(user.Uuid, response.UserUuid);
    }

    [Fact]
    public async Task GetNote_ShouldReturnNote()
    {
        // Arrange
        var db = GetInMemoryDb();
        var user = new UserEntity { Name = "Alice", Birthday = DateTime.UtcNow };
        var note = new NoteEntity { Headline = "H", Text = "T", UserUuid = "1" };
        db.Notes.Add(note);
        await db.SaveChangesAsync();

        var service = new GrpcService.Services.NoteService(db);
        var request = new GetNoteRequest { Uuid = note.Uuid };

        // Act
        var response = await service.GetNote(request, null!);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(note.Headline, response.Headline);
        Assert.Equal(note.Text, response.Text);
        Assert.Equal(note.Uuid, response.Uuid);
    }

    [Fact]
    public async Task UpdateNote_ShouldReturnUpdatedNote()
    {
        // Arrange
        var db = GetInMemoryDb();
        var note = new NoteEntity { Headline = "Old", Text = "OldText", UserUuid = "1" };
        db.Notes.Add(note);
        await db.SaveChangesAsync();

        var service = new GrpcService.Services.NoteService(db);
        var request = new UpdateNoteRequest
        {
            Uuid = note.Uuid,
            Headline = "New",
            Text = "NewText"
        };

        // Act
        var response = await service.UpdateNote(request, null!);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("New", response.Headline);
        Assert.Equal("NewText", response.Text);
    }

    [Fact]
    public async Task DeleteNote_ShouldReturnSuccess()
    {
        // Arrange
        var db = GetInMemoryDb();
        var note = new NoteEntity { Headline = "H", Text = "T", UserUuid = "1" };
        db.Notes.Add(note);
        await db.SaveChangesAsync();

        var service = new GrpcService.Services.NoteService(db);
        var request = new DeleteNoteRequest { Uuid = note.Uuid };

        // Act
        var response = await service.DeleteNote(request, null!);

        // Assert
        Assert.True(response.Success);
        Assert.Null(await db.Notes.FindAsync(note.Uuid));
    }

    [Fact]
    public async Task CreateNote_ShouldThrowIfUserNotFound()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new GrpcService.Services.NoteService(db);
        var request = new CreateNoteRequest
        {
            Headline = "H",
            Text = "T",
            UserUuid = "non-existent"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RpcException>(() => service.CreateNote(request, null!));
        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task GetNote_ShouldThrowIfNotFound()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new GrpcService.Services.NoteService(db);
        var request = new GetNoteRequest { Uuid = "non-existent" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RpcException>(() => service.GetNote(request, null!));
        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task UpdateNote_ShouldThrowIfNotFound()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new GrpcService.Services.NoteService(db);
        var request = new UpdateNoteRequest
        {
            Uuid = "non-existent",
            Headline = "H",
            Text = "T"
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RpcException>(() => service.UpdateNote(request, null!));
        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }

    [Fact]
    public async Task DeleteNote_ShouldThrowIfNotFound()
    {
        // Arrange
        var db = GetInMemoryDb();
        var service = new GrpcService.Services.NoteService(db);
        var request = new DeleteNoteRequest { Uuid = "non-existent" };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<RpcException>(() => service.DeleteNote(request, null!));
        Assert.Equal(StatusCode.NotFound, ex.StatusCode);
    }
}
