// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNet.Http.Internal;
using Microsoft.AspNet.Mvc.Abstractions;
using Microsoft.AspNet.Mvc.Infrastructure;
using Microsoft.AspNet.Mvc.Internal;
using Microsoft.AspNet.Mvc.ModelBinding;
using Microsoft.AspNet.Mvc.ModelBinding.Validation;
using Microsoft.AspNet.Routing;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Mvc.Controllers
{
    public class DefaultControllerActivatorTest
    {
        [Theory]
        [InlineData(typeof(TypeDerivingFromController))]
        [InlineData(typeof(PocoType))]
        public void Create_CreatesInstancesOfTypes(Type type)
        {
            // Arrange
            var activator = new DefaultControllerActivator(new TypeActivatorCache());
            var serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider.Object
            };

            var actionContext = new ControllerContext(
                new ActionContext(
                    httpContext,
                    new RouteData(),
                    new ControllerActionDescriptor
                    {
                        ControllerTypeInfo = type.GetTypeInfo()
                    }));

            // Act
            var instance = activator.Create(actionContext);

            // Assert
            Assert.IsType(type, instance);
        }

        [Theory]
        [InlineData(typeof(int))]
        [InlineData(typeof(OpenGenericType<>))]
        [InlineData(typeof(AbstractType))]
        [InlineData(typeof(InterfaceType))]
        public void CreateController_ThrowsIfControllerCannotBeActivated(Type type)
        {
            // Arrange
            var actionDescriptor = new ControllerActionDescriptor
            {
                ControllerTypeInfo = type.GetTypeInfo()
            };

            var context = new ControllerContext()
            {
                ActionDescriptor = actionDescriptor,
                HttpContext = new DefaultHttpContext()
                {
                    RequestServices = GetServices(),
                },
            };
            var factory = new DefaultControllerActivator(new DefaultTypeActivatorCache());

            // Act and Assert
            var exception = Assert.Throws<InvalidOperationException>(() => factory.Create(context));
            Assert.Equal(
                $"The type '{type.FullName}' cannot be activated by '{typeof(DefaultControllerActivator).FullName}' " +
                "because it is either a value type, an interface, an abstract class or an open generic type.",
                exception.Message);
        }

        [Fact]
        public void DefaultControllerActivator_ReleasesNonIDisposableController()
        {
            // Arrange
            var activator = new DefaultControllerActivator(Mock.Of<ITypeActivatorCache>());
            var controller = new object();

            // Act + Assert (does not throw)
            activator.Release(Mock.Of<ControllerContext>(), controller);
        }

        [Fact]
        public void Create_TypeActivatesTypesWithServices()
        {
            // Arrange
            var activator = new DefaultControllerActivator(new TypeActivatorCache());
            var serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
            var testService = new TestService();
            serviceProvider.Setup(s => s.GetService(typeof(TestService)))
                           .Returns(testService)
                           .Verifiable();

            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProvider.Object
            };

            var actionContext = new ControllerContext(
                new ActionContext(
                    httpContext,
                    new RouteData(),
                    new ControllerActionDescriptor
                    {
                        ControllerTypeInfo = typeof(TypeDerivingFromControllerWithServices).GetTypeInfo()
                    }));

            // Act
            var instance = activator.Create(actionContext);

            // Assert
            var controller = Assert.IsType<TypeDerivingFromControllerWithServices>(instance);
            Assert.Same(testService, controller.TestService);
            serviceProvider.Verify();
        }

        public class Controller
        {
        }

        private class TypeDerivingFromController : Controller
        {
        }

        private class TypeDerivingFromControllerWithServices : Controller
        {
            public TypeDerivingFromControllerWithServices(TestService service)
            {
                TestService = service;
            }

            public TestService TestService { get; }
        }

        private IServiceProvider GetServices()
        {
            var metadataProvider = new EmptyModelMetadataProvider();
            var services = new Mock<IServiceProvider>();
            services.Setup(s => s.GetService(typeof(IUrlHelper)))
                    .Returns(Mock.Of<IUrlHelper>());
            services.Setup(s => s.GetService(typeof(IModelMetadataProvider)))
                    .Returns(metadataProvider);
            services.Setup(s => s.GetService(typeof(IObjectModelValidator)))
                    .Returns(new DefaultObjectValidator(metadataProvider));
            return services.Object;
        }

        private class PocoType
        {
        }

        private class TestService
        {
        }

        private class OpenGenericType<T> : Controller
        {
        }

        private abstract class AbstractType : Controller
        {
        }

        private interface InterfaceType
        {
        }
    }
}
