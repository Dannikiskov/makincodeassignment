namespace GrpcService.Data.Entities;

public class UserEntity
{
    public string Uuid { get; init; } = Guid.CreateVersion7().ToString();
    
    public string Name { get; set; } = string.Empty;
    
    public DateTime Birthday { get; init; }
    
    public ICollection<NoteEntity> Notes { get; init; } = new List<NoteEntity>();
}
