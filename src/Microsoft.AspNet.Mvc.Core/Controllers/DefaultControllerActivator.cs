// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNet.Mvc.Core;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.AspNet.Mvc.Internal;

namespace Microsoft.AspNet.Mvc.Controllers
{
    /// <summary>
    /// <see cref="IControllerActivator"/> that uses type activation to create controllers.
    /// </summary>
    public class DefaultControllerActivator : IControllerActivator
    {
        private readonly ITypeActivatorCache _typeActivatorCache;

        /// <summary>
        /// Creates a new <see cref="DefaultControllerActivator"/>.
        /// </summary>
        /// <param name="typeActivatorCache">The <see cref="ITypeActivatorCache"/>.</param>
        public DefaultControllerActivator(ITypeActivatorCache typeActivatorCache)
        {
            if (typeActivatorCache == null)
            {
                throw new ArgumentNullException(nameof(typeActivatorCache));
            }

            _typeActivatorCache = typeActivatorCache;
        }

        /// <inheritdoc />
        public virtual object Create(ControllerContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            if (actionContext.ActionDescriptor == null)
            {
                throw new ArgumentException(Resources.FormatPropertyOfTypeCannotBeNull(
                    nameof(ControllerContext.ActionDescriptor),
                    nameof(ControllerContext)));
            }

            var controllerTypeInfo = actionContext.ActionDescriptor.ControllerTypeInfo;
            if (controllerTypeInfo.IsValueType ||
                controllerTypeInfo.IsInterface ||
                controllerTypeInfo.IsAbstract ||
                (controllerTypeInfo.IsGenericType && controllerTypeInfo.IsGenericTypeDefinition))
            {
                var message = Resources.FormatValueInterfaceAbstractOrOpenGenericTypesCannotBeActivated(
                    controllerTypeInfo.FullName,
                    GetType().FullName);

                throw new InvalidOperationException(message);
            }

            var serviceProvider = actionContext.HttpContext.RequestServices;
            return _typeActivatorCache.CreateInstance<object>(serviceProvider, controllerTypeInfo.AsType());
        }

        public virtual void Release(ControllerContext context, object controller)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (controller == null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            var disposable = controller as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}
