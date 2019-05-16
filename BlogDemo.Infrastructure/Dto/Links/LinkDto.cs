namespace BlogDemo.Infrastructure.Dto.Links
{
    public class LinkDto
    {
        public LinkDto(string href, string rel, string method)
        {
            Href = href;
            Rel = rel;
            Method = method;
        }

        //链接
        public string Href { get; set; }
        //关系 比如self
        public string Rel { get; set; }
        //动作
        public string Method { get; set; }
    }
}
