using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BlogDemo.Core.Entites;
using BlogDemo.Core.Interfaces.IRepository;
using BlogDemo.Core.Interfaces.UnitOfWork;
using BlogDemo.Infrastructure.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlogDemo.API.Controllers
{
    [Route("api/postimages")]
    public class PostImageController : Controller
    {
        private readonly IPostImageRepository _postImageRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHostingEnvironment _hostingEnvironment;

        public PostImageController(
            IPostImageRepository postImageRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IHostingEnvironment hostingEnvironment)
        {
            _postImageRepository = postImageRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        [AllowAnonymous]
        //上传单个文件用IFormFile
        //记得中间件
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null)
            {
                return BadRequest("File is null");
            }

            //文件长度是否为0
            if (file.Length == 0)
            {
                return BadRequest("File is empty");
            }

            //文件限制大小，最大10M
            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest("File size cannot exceed 10M");
            }

            //检查后缀名
            var acceptTypes = new[] { ".jpg", ".jpeg", ".png" };

            if (acceptTypes.All(t => t != Path.GetExtension(file.FileName).ToLower()))
            {
                return BadRequest("File type not valid, only jpg and png are acceptable.");
            }

            //检查目录是否存在
            if (string.IsNullOrWhiteSpace(_hostingEnvironment.WebRootPath))
            {
                _hostingEnvironment.WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            //如果目录不存在则创建该目录
            var uploadsFolderPath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolderPath))
            {
                Directory.CreateDirectory(uploadsFolderPath);
            }

            //拼接文件名
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolderPath, fileName);

            //上传
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            //把文件名放到数据库
            var postImage = new PostImage
            {
                FileName = fileName
            };

            _postImageRepository.Add(postImage);

            await _unitOfWork.SaveAsync();

            var result = _mapper.Map<PostImage, PostImageDto>(postImage);

            //返回结果
            return Ok(result);
        }
    }
}
