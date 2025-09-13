namespace GrpcService.Data.Entities;

public class NoteEntity
{
    public string Uuid { get; init; } = Guid.CreateVersion7().ToString();
    
    public string Headline { get; set; } = string.Empty;
    
    public string Text { get; set; } = string.Empty;
    
    // Foreign key to User
    public string UserUuid { get; init; } = null!;
    
    public UserEntity User { get; init; } = null!;
}
