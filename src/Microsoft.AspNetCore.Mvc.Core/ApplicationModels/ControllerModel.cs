// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Microsoft.AspNetCore.Mvc.ApplicationModels
{
    [DebuggerDisplay("Name={ControllerName}, Type={ControllerType.Name}," +
                     " Routes: {AttributeRoutes.Count}, Filters: {Filters.Count}")]
    public class ControllerModel : ICommonModel, IFilterModel, IApiExplorerModel
    {
        public ControllerModel(
            TypeInfo controllerType,
            IReadOnlyList<object> attributes)
        {
            if (controllerType == null)
            {
                throw new ArgumentNullException(nameof(controllerType));
            }

            if (attributes == null)
            {
                throw new ArgumentNullException(nameof(attributes));
            }

            ControllerType = controllerType;

            Actions = new List<ActionModel>();
            ApiExplorer = new ApiExplorerModel();
            Attributes = new List<object>(attributes);
            AttributeRoutes = new List<AttributeRouteModel>();
            ActionConstraints = new List<IActionConstraintMetadata>();
            Filters = new List<IFilterMetadata>();
            RouteConstraints = new List<IRouteConstraintProvider>();
            Properties = new Dictionary<object, object>();
            ControllerProperties = new List<PropertyModel>();
        }

        public ControllerModel(ControllerModel other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            ControllerName = other.ControllerName;
            ControllerType = other.ControllerType;

            // Still part of the same application
            Application = other.Application;

            // These are just metadata, safe to create new collections
            ActionConstraints = new List<IActionConstraintMetadata>(other.ActionConstraints);
            Attributes = new List<object>(other.Attributes);
            Filters = new List<IFilterMetadata>(other.Filters);
            RouteConstraints = new List<IRouteConstraintProvider>(other.RouteConstraints);
            Properties = new Dictionary<object, object>(other.Properties);

            // Make a deep copy of other 'model' types.
            Actions = new List<ActionModel>(other.Actions.Select(a => new ActionModel(a)));
            ApiExplorer = new ApiExplorerModel(other.ApiExplorer);
            AttributeRoutes = new List<AttributeRouteModel>(
                other.AttributeRoutes.Select(a => new AttributeRouteModel(a)));
            ControllerProperties =
                new List<PropertyModel>(other.ControllerProperties.Select(p => new PropertyModel(p)));
        }

        public IList<IActionConstraintMetadata> ActionConstraints { get; private set; }

        public IList<ActionModel> Actions { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ApiExplorerModel"/> for this controller.
        /// </summary>
        /// <remarks>
        /// <see cref="ControllerModel.ApiExplorer"/> allows configuration of settings for ApiExplorer
        /// which apply to all actions in the controller unless overridden by <see cref="ActionModel.ApiExplorer"/>.
        /// 
        /// Settings applied by <see cref="ControllerModel.ApiExplorer"/> override settings from
        /// <see cref="ApplicationModel.ApiExplorer"/>.
        /// </remarks>
        public ApiExplorerModel ApiExplorer { get; set; }

        public ApplicationModel Application { get; set; }

        public IList<AttributeRouteModel> AttributeRoutes { get; private set; }

        public IReadOnlyList<object> Attributes { get; }

        MemberInfo ICommonModel.MemberInfo => ControllerType;

        string ICommonModel.Name => ControllerName;

        public string ControllerName { get; set; }

        public TypeInfo ControllerType { get; private set; }

        public IList<PropertyModel> ControllerProperties { get; }

        public IList<IFilterMetadata> Filters { get; private set; }

        public IList<IRouteConstraintProvider> RouteConstraints { get; private set; }

        /// <summary>
        /// Gets a set of properties associated with the controller.
        /// These properties will be copied to <see cref="Abstractions.ActionDescriptor.Properties"/>.
        /// </summary>
        /// <remarks>
        /// Entries will take precedence over entries with the same key
        /// in <see cref="ApplicationModel.Properties"/>.
        /// </remarks>
        public IDictionary<object, object> Properties { get; }
    }
}
