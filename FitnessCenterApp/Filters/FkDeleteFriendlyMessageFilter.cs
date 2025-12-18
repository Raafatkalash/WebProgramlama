using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.SqlClient;                 // مهم
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FitnessCenterApp.Filters
{
    public class FkDeleteFriendlyMessageFilter : IAsyncExceptionFilter
    {
        public Task OnExceptionAsync(ExceptionContext context)
        {
            // FK conflict in SQL Server = error number 547
            if (context.Exception is DbUpdateException dbEx &&
                dbEx.InnerException is SqlException sqlEx &&
                sqlEx.Number == 547)
            {
                var tempDataFactory = context.HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
                var tempData = tempDataFactory.GetTempData(context.HttpContext);

                tempData["Error"] = "Bu kayıt randevular ile ilişkili olduğu için silinemez. Önce ilgili randevuları siliniz/iptal ediniz.";

                var controller = (string?)context.RouteData.Values["controller"] ?? "Home";
                context.Result = new RedirectToActionResult("Index", controller, null);

                context.ExceptionHandled = true;
            }

            return Task.CompletedTask;
        }
    }
}
