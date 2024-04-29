using Autofac;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VxGuardClient.Autofac
{
    public class AutofacServiceProvider : IServiceProvider, ISupportRequiredService
    {
        private readonly IComponentContext _componentContext;

        public AutofacServiceProvider(IComponentContext componentContext)
        {
            this._componentContext = componentContext;
        }

        public object GetRequiredService(Type serviceType)
        {
            return this._componentContext.Resolve(serviceType);
        }

        public object GetService(Type serviceType)
        {
            return this._componentContext.ResolveOptional(serviceType);
        }
    }
}
