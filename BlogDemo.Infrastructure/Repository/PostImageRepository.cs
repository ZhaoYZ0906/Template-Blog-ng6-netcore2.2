using BlogDemo.Core.Entites;
using BlogDemo.Core.Interfaces;
using BlogDemo.Core.Interfaces.IRepository;
using BlogDemo.Infrastructure.Database;

namespace BlogDemo.Infrastructure.Repository.PostRepository
{
    public class PostImageRepository: IPostImageRepository
    {
        private readonly MyContext _myContext;

        public PostImageRepository(MyContext myContext)
        {
            _myContext = myContext;
        }

        public void Add(PostImage postImage)
        {
            _myContext.PostImage.Add(postImage);
        }

    }
}
