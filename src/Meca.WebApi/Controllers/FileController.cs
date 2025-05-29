using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Meca.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using UtilityFramework.Application.Core3;
using UtilityFramework.Application.Core3.ViewModels;

// ReSharper disable UnusedMember.Local

namespace Meca.WebApi.Controllers
{
    [EnableCors("AllowAllOrigin")]
    [Route("api/v1/File")]
    public class FileController : MainController
    {
        private readonly IHostingEnvironment _env;
        private readonly List<string> _acceptedFiles;
        private int limitMb = 10;
        public FileController(IHostingEnvironment env, IHttpContextAccessor httpContext, IConfiguration configuration) : base(null, httpContext, configuration)
        {
            _env = env;
            _acceptedFiles = [".png", ".jpeg", ".jpg", ".gif", ".pdf"];
        }

        /// <summary>
        /// Upload one file
        /// </summary>
        /// <remarks>
        ///
        ///     POST api/v1/profile/Upload
        ///
        ///         content-type: multipart/form-data
        ///         file = archive
        ///
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Upload")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] bool? returnWithUrl, [FromForm] string path = "", [FromForm] bool checkLength = true)
        {

            object response = null;
            try
            {
                if (file == null || file.Length <= 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FileNotFound));

                var totalMb = ConvertBytesToMegabytes(file.Length);

                if (checkLength == true && totalMb > limitMb)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.LargeFile.Replace("{{ limitMb }}", limitMb.ToString())));

                var extension = file.GetExtension(false);

                if (_acceptedFiles.Count(x => x == extension) == 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidFile));

                var folder = $"{_env.ContentRootPath}\\content\\upload\\{path}".Trim();

                var exists = Directory.Exists(folder);

                if (exists == false)
                    Directory.CreateDirectory(folder);

                var arquivo = Utilities.GetUniqueFileName(Path.GetFileNameWithoutExtension(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()), folder, extension).ToLower();

                arquivo = $"{arquivo}{extension}";

                var pathFile = Path.Combine(folder, arquivo);

                using (var destinationStream = System.IO.File.Create(pathFile))
                {
                    await file.CopyToAsync(destinationStream);
                }

                response = new
                {
                    fileName = returnWithUrl == true ? arquivo.SetPathImage() : arquivo
                };


                return Ok(Utilities.ReturnSuccess(data: response));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(data: response));
            }
        }

        /// <summary>
        /// UPLOAD FILES
        /// </summary>
        /// <remarks>
        ///
        ///     POST api/v1/profile/aploads
        ///
        ///         content-type: multipart/form-data
        ///         files = archive
        ///         files = archive
        ///         path = foldername
        ///
        /// </remarks>
        /// <response code="200">Returns success</response>
        /// <response code="400">Custom Error</response>
        /// <response code="401">Unauthorize Error</response>
        /// <response code="500">Exception Error</response>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Uploads")]
        [ProducesResponseType(typeof(ReturnViewModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> Uploads([FromForm] List<IFormFile> files, [FromForm] bool? returnWithUrl, [FromForm] string path = "", [FromForm] bool checkLength = true)
        {
            object response = null;
            try
            {
                if (files.Count == 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.FileNotFound));

                var filesNames = new List<string>();

                if (files.Count(file => _acceptedFiles.Any(x => x == file.GetExtension(false))) == 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.InvalidFile));

                if (checkLength == true && files.Count(file => ConvertBytesToMegabytes(file.Length) > limitMb) > 0)
                    return BadRequest(Utilities.ReturnErro(DefaultMessages.OneOrMoreFilesToLarge));

                var folder = $"{_env.ContentRootPath}\\content\\upload\\{path}".Trim();

                var exists = Directory.Exists(folder);

                if (exists == false)
                    Directory.CreateDirectory(folder);

                for (int i = 0; i < files.Count; i++)
                {
                    var fileName = files[i];

                    if (fileName.Length <= 0)
                        continue;

                    var extension = files[i].GetExtension(false);

                    var arquivo = Utilities.GetUniqueFileName(Path.GetFileNameWithoutExtension(DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString()), folder, extension).ToLower();

                    arquivo = $"{arquivo}{extension}";

                    var pathFile = Path.Combine(folder, arquivo);

                    using (var destinationStream = System.IO.File.Create(pathFile))
                    {
                        await fileName.CopyToAsync(destinationStream);
                    }

                    filesNames.Add(returnWithUrl == true
                    ? arquivo.SetPathImage()
                    : arquivo);
                }

                response = new
                {
                    fileNames = filesNames
                };

                return Ok(Utilities.ReturnSuccess(data: response));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.ReturnErro(data: response));
            }
        }

        private static double ConvertBytesToMegabytes(long bytes) => (bytes / 1024f) / 1024f;

        private static double ConvertKilobytesToMegabytes(long kilobytes) => kilobytes / 1024f;
    }
}