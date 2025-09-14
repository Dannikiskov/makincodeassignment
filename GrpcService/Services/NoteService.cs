using Grpc.Core;
using GrpcService.Data;

namespace GrpcService.Services;

public class NoteService(AppDbContext db) : GrpcService.NoteService.NoteServiceBase
{
    public override async Task<Note> CreateNote(CreateNoteRequest request, ServerCallContext context)
    {
        if (request is { Headline: null or "" } or { UserUuid: null or "" } or { Text: null or "" })
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Request must contain Headline, Text, and UserUuid."));
        }
        
        var user = await db.Users.FindAsync(request.UserUuid)
                    ?? throw new RpcException(new Status(StatusCode.NotFound, "User not found"));

        var note = new Data.Entities.NoteEntity
        {
            Headline = request.Headline,
            Text = request.Text,
            UserUuid = user.Uuid,
        };

        db.Notes.Add(note);
        await db.SaveChangesAsync();

        return new Note
        {
            Uuid = note.Uuid,
            Headline = note.Headline,
            Text = note.Text,
            UserUuid = user.Uuid,
        };
    }

    public override async Task<Note> GetNote(GetNoteRequest request, ServerCallContext context)
    {
        if (request is { Uuid: null or "" })
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Request must contain Uuid"));
        }
        
        var note = await db.Notes.FindAsync(request.Uuid) 
                    ?? throw new RpcException(new Status(StatusCode.NotFound, "Note not found"));

        return new Note
        {
            Uuid = note.Uuid,
            Headline = note.Headline,
            Text = note.Text,
            UserUuid = note.UserUuid
        };
    }

    public override async Task<Note> UpdateNote(UpdateNoteRequest request, ServerCallContext context)
    {
        if (request is { Headline: null or "" } or { Text: null or "" })
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Request must contain Headline and Text"));
        }
        
        var note = await db.Notes.FindAsync(request.Uuid)
                    ?? throw new RpcException(new Status(StatusCode.NotFound, "Note not found"));

        note.Headline = request.Headline;
        note.Text = request.Text;

        await db.SaveChangesAsync();

        return new Note
        {
            Uuid = note.Uuid,
            Headline = note.Headline,
            Text = note.Text,
            UserUuid = note.UserUuid
        };
    }

    public override async Task<DeleteNoteResponse> DeleteNote(DeleteNoteRequest request, ServerCallContext context)
    {
        if (request is { Uuid: null or "" })
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Request must contain Uuid"));
        }
        
        var note = await db.Notes.FindAsync(request.Uuid)
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Note not found"));

        db.Notes.Remove(note);
        await db.SaveChangesAsync();

        return new DeleteNoteResponse { Success = true };
    }
}