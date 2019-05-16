namespace BlogDemo.Infrastructure.Dto
{
    public class PostImageDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Location => $"/uploads/{FileName}";
    }
}
