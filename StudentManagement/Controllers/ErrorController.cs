using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace StudentManagement.Controllers
{
    public class ErrorController : Controller
    {
        private readonly ILogger<ErrorController> _logger;

        /// <summary>
        /// 通过依赖注入服务，注入 ILogger
        /// 将指定类型的控制器作为泛型参数
        /// </summary>
        /// <param name="logger"></param>
        public ErrorController(ILogger<ErrorController> logger)
        {
            _logger = logger;
        }

        [Route("Error/{statusCode}")]
        public IActionResult HttpStatusCodeHandler(int statusCode)
        {
            var statusCodeResult = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

            switch (statusCode)
            {
                case 404:
                    ViewBag.ErrorMessage = "抱歉，您访问的页面不存在";
                    //ViewBag.Path = statusCodeResult.OriginalPath;
                    //ViewBag.QueryStr = statusCodeResult.OriginalQueryString;

                    _logger.LogWarning($"发生了一个 404 错误。\n路径：{statusCodeResult.OriginalPath}\n查询字符串：{statusCodeResult.OriginalQueryString}");

                    break;

            }

            return View("NotFound");
        }

        [AllowAnonymous]
        [Route("Error")]
        public IActionResult Error()
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            _logger.LogError($"路径：{exceptionHandlerPathFeature.Path}\n产生了一个错误：{exceptionHandlerPathFeature.Error}");

            //ViewBag.ExceptionPath = exceptionHandlerPathFeature.Path;
            //ViewBag.ExceptionMessage = exceptionHandlerPathFeature.Error.Message;
            //ViewBag.StackTrace = exceptionHandlerPathFeature.Error.StackTrace;

            return View();
        }
    }
}