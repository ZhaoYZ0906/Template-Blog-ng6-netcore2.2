using BlogDemo.Core.Entites;
using BlogDemo.Core.Entites.QueryParameters;
using BlogDemo.Core.Entites.x_xxx;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BlogDemo.Core.Interfaces.IRepository
{
    public interface IPostRepository
    {
        //获得post表中的所有数据
        Task<PaginatedList<Post>> GetPostsAllAsync(PostParameter postParameter);

        //根据Id查询一条数据
        Task<Post> GetPostByIdAsync(int id);

        //添加一个对象
        void AddPost(Post post);

        //删除一个对象
        void DeletePost(Post post);

        //Patch更新对象
        void UpdatePost(Post post);

    }
}
