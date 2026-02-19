namespace UniTask.Api.Tasks;

public class AttachmentDto
{
    public int Id { get; set; }
    public string? ExternalId { get; set; }
    public required string Name { get; set; }
    public required string Url { get; set; }
    public required string InternalName { get; set; }
    public required string FileType { get; set; }
    public int? UploadedById { get; set; }
}
