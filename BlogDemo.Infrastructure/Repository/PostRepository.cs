using BlogDemo.Core.Entites;
using BlogDemo.Core.Entites.QueryParameters;
using BlogDemo.Core.Entites.x_xxx;
using BlogDemo.Core.Interfaces.IRepository;
using BlogDemo.Infrastructure.Database;
using BlogDemo.Infrastructure.Dto;
using BlogDemo.Infrastructure.Extensions;
using BlogDemo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogDemo.Infrastructure.Repository.PostRepository
{
    /// <summary>
    /// 针对post表的持久化操作
    /// </summary>
    public class PostRepository : IPostRepository
    {
        private readonly MyContext myContext;
        private readonly IPropertyMappingContainer propertyMappingContainer;

        public PostRepository(MyContext myContext, IPropertyMappingContainer propertyMappingContainer)
        {
            this.myContext = myContext;
            this.propertyMappingContainer = propertyMappingContainer;
        }

        /// <summary>
        /// 添加一个post
        /// </summary>
        /// <param name="post"></param>
        public void AddPost(Post post)
        {
             myContext.Posts.Add(post);
        }

        public void DeletePost(Post post)
        {
            myContext.Remove(post);
        }

        /// <summary>
        /// 根据id查询一个post
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Post> GetPostByIdAsync(int id) {
            return await myContext.Posts.FindAsync(id);
        }


        /// <summary>
        /// 获取所有的post，排序
        /// </summary>
        /// <param name="postParameter"></param>
        /// <returns></returns>
        public async Task<PaginatedList<Post>> GetPostsAllAsync(PostParameter postParameter)
        {
            var query = myContext.Posts.AsQueryable();

            if (!string.IsNullOrEmpty(postParameter.Title))
            {
                var title = postParameter.Title.ToLowerInvariant();
                query = query.Where(x => x.Title.ToLowerInvariant().Contains(title));
            }

            //排序代码
            query = query.ApplySort(postParameter.OrderBy, propertyMappingContainer.Resolve<PostDto, Post>());

            var count = await query.CountAsync();

            var list= await query.
                Skip(postParameter.PageIndex * postParameter.PageSize).
                Take(postParameter.PageSize).
                ToListAsync();

            return new PaginatedList<Post>(postParameter.PageIndex, postParameter.PageSize, count, list);
        }

        public void UpdatePost(Post post)
        {
            myContext.Entry(post).State = EntityState.Modified;
        }
    }
}
