using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BlogDemo.Api.Helpers;
using BlogDemo.Core.Entites;
using BlogDemo.Core.Entites.Enum;
using BlogDemo.Core.Entites.QueryParameters;
using BlogDemo.Core.Interfaces.IRepository;
using BlogDemo.Core.Interfaces.UnitOfWork;
using BlogDemo.Infrastructure.Database;
using BlogDemo.Infrastructure.Dto;
using BlogDemo.Infrastructure.Dto.Links;
using BlogDemo.Infrastructure.ResourcePlasticity;
using BlogDemo.Infrastructure.ResourcePlasticity.PlasticityValidator;
using BlogDemo.Infrastructure.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BlogDemo.API.Controllers
{
    [Route("api/Posts")]
    [ApiController]
    //允许不认证访问
    [AllowAnonymous]
    public class PostsController : ControllerBase
    {
        private readonly IPostRepository repository;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<PostsController> logger;
        private readonly IMapper mapper;
        private readonly IUrlHelper urlHelper;
        private readonly ITypeHelperService typeHelperService;
        private readonly IPropertyMappingContainer propertyMappingContainer;

        public PostsController(
            IPostRepository Repository,
            IUnitOfWork unitOfWork,
            ILogger<PostsController> logger,
            IMapper mapper,
            IUrlHelper urlHelper,
            ITypeHelperService typeHelperService,
            IPropertyMappingContainer propertyMappingContainer)
        {
            repository = Repository;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
            this.mapper = mapper;
            this.urlHelper = urlHelper;
            this.typeHelperService = typeHelperService;
            this.propertyMappingContainer = propertyMappingContainer;
        }


        /// <summary>
        /// 查询posts方法，可以通过url传递参数确定分页的大小和当前页
        /// </summary>
        /// <param name="postParameter"></param>
        /// <returns></returns>
        [HttpGet(Name = "GetPosts")]
        [AllowAnonymous]
        [RequestHeaderMatchingMediaType("Accept", new[] { "application/vnd.zyz.hateoas+json" })]
        public async Task<IActionResult> Get([FromQuery]PostParameter postParameter)
        {
            logger.LogInformation("测试日志");

            //检查器部分
            if (!propertyMappingContainer.ValidateMappingExistsFor<PostDto, Post>(postParameter.OrderBy))
            {
                return BadRequest("排序属性不存在！");
            }

            if (!typeHelperService.TypeHasProperties<PostDto>(postParameter.Fields))
            {
                return BadRequest("塑形属性不存在！");
            }

            //查询-转换dto-塑形-添加links
            var postList = await repository.GetPostsAllAsync(postParameter);

            //这部分属于head附带资源
            {
                //已经写在了link里面
                //生成上一页下一页链接
                //var previouspage = postList.HasPrevious ? CreatePostUri(postParameter, DtoUriType.PreviousPage) : null;
                //var nextpage = postList.HasNext ? CreatePostUri(postParameter, DtoUriType.NextPage) : null;

                var meta = new
                {
                    postList.PageSize,
                    postList.PageIndex,
                    postList.PageCount,
                    postList.TotalItemsCount
                };


                //自定义一个x-Pagination的header，跨域设置那里要添加，然后跨域不返回这个
                Response.Headers.Add("x-Pagination", JsonConvert.SerializeObject(meta, new JsonSerializerSettings
                {
                    //负责把以大写开头的属性名在返回时变成小写
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                }));
            }


            var ss = mapper.Map<IEnumerable<Post>, IEnumerable<PostDto>>(postList);

            var shapedPostResources = ss.ToDynamicIEnumerable(postParameter.Fields);


            var resultValue = shapedPostResources.Select(x =>
            {
                var dict = x as IDictionary<string, object>;
                var postLinks = CreateLinksForPost((int)dict["Id"], postParameter.Fields);
                dict.Add("links", postLinks);
                return dict;
            });

            var resultLink = CreateLinksForPosts(postParameter, postList.HasPrevious, postList.HasNext);

            var result = new
            {
                Value = resultValue,
                resultLink
            };
            return Ok(result);

        }




        /// <summary>
        /// 针对某一个id进行查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetPost")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(int id, string fields)
        {

            if (!typeHelperService.TypeHasProperties<PostDto>(fields))
            {
                return BadRequest("塑形属性不存在！");
            }

            var post = await repository.GetPostByIdAsync(id);

            if (post == null)
            {
                return NotFound();
            }

            var postDto = mapper.Map<Post, PostDto>(post);

            var PostResult = postDto.ToDynamic(fields);

            var result = PostResult as IDictionary<string, object>;

            var links = CreateLinksForPost(id, fields);

            result.Add("links", links);

            return Ok(PostResult);
        }


        /// <summary>
        /// post添加一个对象
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [RequestHeaderMatchingMediaType("Content-Type", new[] { "application/vnd.post.create+json" })]
        [RequestHeaderMatchingMediaType("Accept", new[] { "application/vnd.zyz.hateoas+json" })]
        public async Task<IActionResult> Post([FromBody] PostAddDto postAddDto)
        {

            if (!ModelState.IsValid)
            {
                return new MyUnprocessableEntityObjectResult(ModelState);
            }

            if (postAddDto == null)
            {
                return BadRequest();
            }

            var addPost = mapper.Map<PostAddDto, Post>(postAddDto);


            var userName = User.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.PreferredUserName)?.Value;
            addPost.Author = userName;
            addPost.LastModified = DateTime.Now;

            repository.AddPost(addPost);

            if (!await unitOfWork.SaveAsync())
            {
                throw new Exception();
            }

            var postdto = mapper.Map<Post, PostDto>(addPost);

            //添加links
            var postLinks = CreateLinksForPost(addPost.Id);
            var resultPost = postdto.ToDynamic() as IDictionary<string, object>;
            resultPost.Add("links", postLinks);

            return CreatedAtRoute("GetPost", new { id = addPost.Id }, resultPost);
        }


        /// <summary>
        /// 删除一个对象
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("{id}", Name = "DeletePost")]
        public async Task<IActionResult> DeletePost(int id) {

            var post =await repository.GetPostByIdAsync(id);
            if (post == null)
                return NotFound();

            repository.DeletePost(post);

            if (!await unitOfWork.SaveAsync())
                throw new Exception($"删除id={id}的时候失败了");

            return NoContent();
        }


        [HttpPut("{id}",Name ="PutPost")]
        [RequestHeaderMatchingMediaType("Content-Type", new[] { "application/vnd.post.update+json" })]
        public async Task<IActionResult> UpdatePost(int id,[FromBody] PostUpdateDto postUpdateDto)
        {
            if (postUpdateDto==null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return new MyUnprocessableEntityObjectResult(ModelState);
            }

            var post = await repository.GetPostByIdAsync(id);

            if (post==null)
            {
                return NotFound();
            }

            post.LastModified = DateTime.Now;

            mapper.Map(postUpdateDto, post);

            if (!await unitOfWork.SaveAsync())
                throw new Exception($"更新对象id为{id}的时候失败了");

            return NoContent();
        }

        [HttpPatch("{id}", Name = "PartiallyUpdatePost")]
        public async Task<IActionResult> PartiallyUpdateCityForCountry(int id,
    [FromBody] JsonPatchDocument<PostUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var post = await repository.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var postToPatch = mapper.Map<PostUpdateDto>(post);

            patchDoc.ApplyTo(postToPatch, ModelState);

            TryValidateModel(postToPatch);

            if (!ModelState.IsValid)
            {
                return new MyUnprocessableEntityObjectResult(ModelState);
            }

            mapper.Map(postToPatch, post);
            post.LastModified = DateTime.Now;
            repository.UpdatePost(post);

            if (!await unitOfWork.SaveAsync())
            {
                throw new Exception($"Patching city {id} failed when saving.");
            }

            return NoContent();
        }










        /// <summary>
        /// 生成上一页下一页的url
        /// </summary>
        /// <param name="parameters">查询参数包括当前第几页，一页几行</param>
        /// <param name="uriType">生成的是上一页的url还是下一页的url</param>
        /// <returns></returns>
        private string CreatePostUri(PostParameter parameters, DtoUriType uriType)
        {
            switch (uriType)
            {
                case DtoUriType.PreviousPage:
                    var previousParameters = new
                    {
                        pageIndex = parameters.PageIndex - 1,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields
                    };
                    return urlHelper.Link("GetPosts", previousParameters);

                case DtoUriType.NextPage:
                    var nextParameters = new
                    {
                        pageIndex = parameters.PageIndex + 1,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields
                    };
                    return urlHelper.Link("GetPosts", nextParameters);

                default:
                    var currentParameters = new
                    {
                        pageIndex = parameters.PageIndex,
                        pageSize = parameters.PageSize,
                        orderBy = parameters.OrderBy,
                        fields = parameters.Fields
                    };
                    return urlHelper.Link("GetPosts", currentParameters);
            }
        }




        /// <summary>
        /// 针对单个对象生成link
        /// </summary>
        /// <param name="id">对象id</param>
        /// <param name="fields">是否塑形</param>
        /// <returns></returns>
        private IEnumerable<LinkDto> CreateLinksForPost(int id, string fields = null)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                links.Add(
                    new LinkDto(
                        urlHelper.Link("GetPost", new { id }), "self", "GET"));
            }
            else
            {
                links.Add(
                    new LinkDto(
                        urlHelper.Link("GetPost", new { id, fields }), "self", "GET"));
            }

            links.Add(
                new LinkDto(
                    urlHelper.Link("DeletePost", new { id }), "delete_post", "DELETE"));

            return links;
        }

        /// <summary>
        /// 针对集合产生link，link里面有上一集合，下一集合（上一页下一页）
        /// </summary>
        /// <param name="postResourceParameters"></param>
        /// <param name="hasPrevious"></param>
        /// <param name="hasNext"></param>
        /// <returns></returns>
        private IEnumerable<LinkDto> CreateLinksForPosts(PostParameter postResourceParameters,
            bool hasPrevious, bool hasNext)
        {
            var links = new List<LinkDto>
            {
                new LinkDto(
                    CreatePostUri(postResourceParameters, DtoUriType.CurrentPage),
                    "self", "GET")
            };

            if (hasPrevious)
            {
                links.Add(
                    new LinkDto(
                        CreatePostUri(postResourceParameters, DtoUriType.PreviousPage),
                        "previous_page", "GET"));
            }

            if (hasNext)
            {
                links.Add(
                    new LinkDto(
                        CreatePostUri(postResourceParameters, DtoUriType.NextPage),
                        "next_page", "GET"));
            }

            return links;
        }

    }
}