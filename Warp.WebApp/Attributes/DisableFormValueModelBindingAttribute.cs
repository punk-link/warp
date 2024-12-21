using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Warp.WebApp.Attributes;


[AttributeUsage(AttributeTargets.Method)]
public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var providerFactories = context.ValueProviderFactories;
        providerFactories.RemoveType<FormValueProviderFactory>();
        providerFactories.RemoveType<FormFileValueProviderFactory>();
        providerFactories.RemoveType<JQueryFormValueProviderFactory>();
    }


    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}
