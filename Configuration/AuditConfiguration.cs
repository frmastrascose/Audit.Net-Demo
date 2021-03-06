using Audit.Core;
using Audit.Demo.Providers.Database;
using Audit.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Demo.Configuration
{
    public static class AuditConfiguration
    {
        private const string CorrelationIdField = "CorrelationId";

        /// <summary>
        /// Add the global audit filter to the MVC pipeline
        /// </summary>
        public static MvcOptions AddAudit(this MvcOptions mvcOptions)
        {
            // Configure the global Action Filter
            mvcOptions.AddAuditFilter(a => a
                    .LogAllActions()
                    .WithEventType("MVC:{verb}:{controller}:{action}")
                    .IncludeModelState()
                    .IncludeRequestBody()
                    .IncludeResponseBody());
            return mvcOptions;
        }

        /// <summary>
        /// Global Audit configuration
        /// </summary>
        public static IServiceCollection ConfigureAudit(this IServiceCollection serviceCollection)
        {
            // TODO: Configure the audit data provider and options. For more info see https://github.com/thepirat000/Audit.NET#data-providers.
            Audit.Core.Configuration.Setup()
                .UseFileLogProvider(_ => _
                    .Directory(@"C:\Temp")
                    .FilenameBuilder(ev => $"{ev.StartDate:yyyyMMddHHmmssffff}_{ev.CustomFields[CorrelationIdField]?.ToString().Replace(':', '_')}.json"))
                .WithCreationPolicy(EventCreationPolicy.InsertOnEnd);

            // Entity framework audit output configuration
            Audit.EntityFramework.Configuration.Setup()
                .ForContext<MyContext>(_ => _
                    .AuditEventType("EF:{context}"))
                .UseOptOut();

            return serviceCollection;
        }

        public static void UseAuditMiddleware(this IApplicationBuilder app)
        {
            // Configure the Middleware
            app.UseAuditMiddleware(_ => _
                .FilterByRequest(r => !r.Path.Value.EndsWith("favicon.ico"))
                .IncludeHeaders()
                .IncludeRequestBody()
                .IncludeResponseBody()
                .WithEventType("HTTP:{verb}:{url}"));
        }

        /// <summary>
        /// Add a RequestId so the audit events can be grouped per request
        /// </summary>
        public static void UseAuditCorrelationId(this IApplicationBuilder app, IHttpContextAccessor ctxAccesor)
        {

            Core.Configuration.AddCustomAction(ActionType.OnScopeCreated, scope =>
            {
                var httpContext = ctxAccesor.HttpContext;
                scope.Event.CustomFields[CorrelationIdField] = httpContext.TraceIdentifier;
            });
        }
    }
}
