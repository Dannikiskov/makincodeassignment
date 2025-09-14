using Grpc.Core;
using GrpcService.Data;
using Google.Protobuf.WellKnownTypes;
using Microsoft.EntityFrameworkCore;

namespace GrpcService.Services;

public class UserService(AppDbContext db) : GrpcService.UserService.UserServiceBase
{
    public override async Task<User> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        var entity = new Data.Entities.UserEntity
        {
            Name = request.Name,
            Birthday = request.Birthday.ToDateTime()
        };

        db.Users.Add(entity);
        await db.SaveChangesAsync();

        return new User
        {
            Uuid = entity.Uuid,
            Name = entity.Name,
            Birthday = Timestamp.FromDateTime(entity.Birthday.ToUniversalTime())
        };
    }

    public override async Task<User> GetUser(GetUserRequest request, ServerCallContext context)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Uuid == request.Uuid)
                    ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        return new User
        {
            Uuid = user.Uuid,
            Name = user.Name,
            Birthday = Timestamp.FromDateTime(user.Birthday.ToUniversalTime())
        };
    }
    
    public override async Task<UserWithNotes> GetUserWithNotes(GetUserRequest request, ServerCallContext context)
    {
        var user = await db.Users.Include(u => u.Notes)
            .FirstOrDefaultAsync(u => u.Uuid == request.Uuid)
                ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var userWithNotes = new UserWithNotes
        {
            Uuid = user.Uuid,
            Name = user.Name,
            Birthday = Timestamp.FromDateTime(user.Birthday.ToUniversalTime())
        };
        
        var mappedNotes = user.Notes.Select(noteEntity => new Note
        {
            Uuid = noteEntity.Uuid,
            Text = noteEntity.Text,
            Headline = noteEntity.Headline,
            UserUuid = noteEntity.UserUuid
        });
        
        userWithNotes.Notes.AddRange(mappedNotes);

        return userWithNotes;
    }

    public override async Task<User> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        var user = await db.Users.FindAsync(request.Uuid)
                    ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        user.Name = request.Name;
        await db.SaveChangesAsync();

        return new User
        {
            Uuid = user.Uuid,
            Name = user.Name,
            Birthday = Timestamp.FromDateTime(user.Birthday.ToUniversalTime())
        };
    }

    public override async Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
    {
        var user = await db.Users.FindAsync(request.Uuid)
                    ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        return new DeleteUserResponse { Success = true };
    }
}
