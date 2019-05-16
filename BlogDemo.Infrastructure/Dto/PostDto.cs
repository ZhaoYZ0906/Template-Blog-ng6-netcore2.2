using System;
using System.Collections.Generic;
using System.Text;

namespace BlogDemo.Infrastructure.Dto
{
    public class PostDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public DateTime updatetime { get; set; }
        public string Remark { get; set; }
    }
}
