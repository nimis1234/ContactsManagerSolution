using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace CURDExample.Filters
{
    // filter which is used to validate the model, this can be used on the method where all we can modal validation
    public class ModalValidationFilterFactory : Attribute, IFilterFactory
    {
        private readonly ILogger<ModalValidationFilterFactory> _logger;
        private int _order;

        public bool IsReusable => false;

        public ModalValidationFilterFactory(int Order)
        {
            _order = Order;
        }
        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            var filter = serviceProvider.GetRequiredService<ModalValidationFilter>(); // get the instance of the filter for which we need to create the instance
            filter.Order = _order;
            return filter;
        }
    }

    // another class for 
    public class ModalValidationFilter : IAsyncActionFilter, IOrderedFilter
    {
        private readonly ILogger<ModalValidationFilter> _logger;

        public int Order { get; set; } // this a property which is used to set the order of the filter

        public ModalValidationFilter(ILogger<ModalValidationFilter> logger)
        {
            _logger = logger;
        }



        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _logger.LogInformation("ModalValidationFilter is running");
            if(!context.ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed for request: {Path}", context.HttpContext.Request.Path);

                string previousAction = context.RouteData.Values["action"]?.ToString() ?? "Index";
                string previousController = context.RouteData.Values["controller"]?.ToString() ?? "Person";

                // Prepare error view with model state errors
                var viewResult = new ViewResult { ViewName = "ModelErrors" };
                viewResult.ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), context.ModelState);

                viewResult.ViewData["Errors"] = context.ModelState;
                viewResult.ViewData["PreviousAction"] = previousAction;
                viewResult.ViewData["PreviousController"] = previousController;
                context.Result = viewResult;
                return; // Stop further execution

            }
            else
            {
                _logger.LogInformation("Model validation passed for request: {Path}", context.HttpContext.Request.Path);
            }
            await next();
        }
    }
}
