


using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

namespace WebAPI_Swagger_NetCore.Swagger
{
    public class SecurityRequirementsOperationFilter : IOperationFilter
    {
        private readonly IOptions<AuthorizationOptions> authorizationOptions;

        public SecurityRequirementsOperationFilter(IOptions<AuthorizationOptions> authorizationOptions)
        {
            this.authorizationOptions = authorizationOptions;
        }

        public void Apply(Operation operation, OperationFilterContext context)
        {
            var authorizeAttributes = context.ApiDescription.ControllerAttributes()
                .OfType<AuthorizeAttribute>();
            if (!authorizeAttributes.Any()) return;
            if (operation.Security == null)
                operation.Security = new List<IDictionary<string, IEnumerable<string>>>();
            operation.Security.Add(new Dictionary<string, IEnumerable<string>>
            {
                {"oauth2",Enumerable.Empty<string>() }
            });
        }
    }
}
