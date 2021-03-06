// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Internal;
#if DOTNET5_4
using System.Reflection;
#endif
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Mvc.ModelBinding.Metadata
{
    /// <summary>
    /// A default <see cref="ModelMetadata"/> implementation.
    /// </summary>
    public class DefaultModelMetadata : ModelMetadata
    {
        private readonly IModelMetadataProvider _provider;
        private readonly ICompositeMetadataDetailsProvider _detailsProvider;
        private readonly DefaultMetadataDetails _details;

        private ReadOnlyDictionary<object, object> _additionalValues;
        private ModelMetadata _elementMetadata;
        private bool? _isBindingRequired;
        private bool? _isReadOnly;
        private bool? _isRequired;
        private ModelPropertyCollection _properties;
        private bool? _validateChildren;
        private ReadOnlyCollection<object> _validatorMetadata;

        /// <summary>
        /// Creates a new <see cref="DefaultModelMetadata"/>.
        /// </summary>
        /// <param name="provider">The <see cref="IModelMetadataProvider"/>.</param>
        /// <param name="detailsProvider">The <see cref="ICompositeMetadataDetailsProvider"/>.</param>
        /// <param name="details">The <see cref="DefaultMetadataDetails"/>.</param>
        public DefaultModelMetadata(
            IModelMetadataProvider provider,
            ICompositeMetadataDetailsProvider detailsProvider,
            DefaultMetadataDetails details)
            : base(details.Key)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (detailsProvider == null)
            {
                throw new ArgumentNullException(nameof(detailsProvider));
            }

            if (details == null)
            {
                throw new ArgumentNullException(nameof(details));
            }

            _provider = provider;
            _detailsProvider = detailsProvider;
            _details = details;
        }

        /// <summary>
        /// Gets the set of attributes for the current instance.
        /// </summary>
        public ModelAttributes Attributes
        {
            get
            {
                return _details.ModelAttributes;
            }
        }

        /// <summary>
        /// Gets the <see cref="Metadata.BindingMetadata"/> for the current instance.
        /// </summary>
        /// <remarks>
        /// Accessing this property will populate the <see cref="Metadata.BindingMetadata"/> if necessary.
        /// </remarks>
        public BindingMetadata BindingMetadata
        {
            get
            {
                if (_details.BindingMetadata == null)
                {
                    var context = new BindingMetadataProviderContext(Identity, _details.ModelAttributes);
                    _detailsProvider.GetBindingMetadata(context);
                    _details.BindingMetadata = context.BindingMetadata;
                }

                return _details.BindingMetadata;
            }
        }

        /// <summary>
        /// Gets the <see cref="Metadata.DisplayMetadata"/> for the current instance.
        /// </summary>
        /// <remarks>
        /// Accessing this property will populate the <see cref="Metadata.DisplayMetadata"/> if necessary.
        /// </remarks>
        public DisplayMetadata DisplayMetadata
        {
            get
            {
                if (_details.DisplayMetadata == null)
                {
                    var context = new DisplayMetadataProviderContext(Identity, _details.ModelAttributes);
                    _detailsProvider.GetDisplayMetadata(context);
                    _details.DisplayMetadata = context.DisplayMetadata;
                }

                return _details.DisplayMetadata;
            }
        }

        /// <summary>
        /// Gets the <see cref="Metadata.ValidationMetadata"/> for the current instance.
        /// </summary>
        /// <remarks>
        /// Accessing this property will populate the <see cref="Metadata.ValidationMetadata"/> if necessary.
        /// </remarks>
        public ValidationMetadata ValidationMetadata
        {
            get
            {
                if (_details.ValidationMetadata == null)
                {
                    var context = new ValidationMetadataProviderContext(Identity, _details.ModelAttributes);
                    _detailsProvider.GetValidationMetadata(context);
                    _details.ValidationMetadata = context.ValidationMetadata;
                }

                return _details.ValidationMetadata;
            }
        }

        /// <inheritdoc />
        public override IReadOnlyDictionary<object, object> AdditionalValues
        {
            get
            {
                if (_additionalValues == null)
                {
                    _additionalValues = new ReadOnlyDictionary<object, object>(DisplayMetadata.AdditionalValues);
                }

                return _additionalValues;
            }
        }

        /// <inheritdoc />
        public override BindingSource BindingSource
        {
            get
            {
                return BindingMetadata.BindingSource;
            }
        }

        /// <inheritdoc />
        public override string BinderModelName
        {
            get
            {
                return BindingMetadata.BinderModelName;
            }
        }

        /// <inheritdoc />
        public override Type BinderType
        {
            get
            {
                return BindingMetadata.BinderType;
            }
        }

        /// <inheritdoc />
        public override bool ConvertEmptyStringToNull
        {
            get
            {
                return DisplayMetadata.ConvertEmptyStringToNull;
            }
        }

        /// <inheritdoc />
        public override string DataTypeName
        {
            get
            {
                return DisplayMetadata.DataTypeName;
            }
        }

        /// <inheritdoc />
        public override string Description
        {
            get
            {
                if (DisplayMetadata.Description == null)
                {
                    return null;
                }

                return DisplayMetadata.Description();
            }
        }

        /// <inheritdoc />
        public override string DisplayFormatString
        {
            get
            {
                return DisplayMetadata.DisplayFormatString;
            }
        }

        /// <inheritdoc />
        public override string DisplayName
        {
            get
            {
                if (DisplayMetadata.DisplayName == null)
                {
                    return null;
                }

                return DisplayMetadata.DisplayName();
            }
        }

        /// <inheritdoc />
        public override string EditFormatString
        {
            get
            {
                return DisplayMetadata.EditFormatString;
            }
        }

        /// <inheritdoc />
        public override ModelMetadata ElementMetadata
        {
            get
            {
                if (_elementMetadata == null && ElementType != null)
                {
                    _elementMetadata = _provider.GetMetadataForType(ElementType);
                }

                return _elementMetadata;
            }
        }

        /// <inheritdoc />
        public override IEnumerable<KeyValuePair<EnumGroupAndName, string>> EnumGroupedDisplayNamesAndValues
        {
            get
            {
                return DisplayMetadata.EnumGroupedDisplayNamesAndValues;
            }
        }

        /// <inheritdoc />
        public override IReadOnlyDictionary<string, string> EnumNamesAndValues
        {
            get
            {
                return DisplayMetadata.EnumNamesAndValues;
            }
        }

        /// <inheritdoc />
        public override bool HasNonDefaultEditFormat
        {
            get
            {
                return DisplayMetadata.HasNonDefaultEditFormat;
            }
        }

        /// <inheritdoc />
        public override bool HideSurroundingHtml
        {
            get
            {
                return DisplayMetadata.HideSurroundingHtml;
            }
        }

        /// <inheritdoc />
        public override bool HtmlEncode
        {
            get
            {
                return DisplayMetadata.HtmlEncode;
            }
        }

        /// <inheritdoc />
        public override bool IsBindingAllowed
        {
            get
            {
                if (MetadataKind == ModelMetadataKind.Property)
                {
                    return BindingMetadata.IsBindingAllowed;
                }
                else
                {
                    return true;
                }
            }
        }

        /// <inheritdoc />
        public override bool IsBindingRequired
        {
            get
            {
                if (!_isBindingRequired.HasValue)
                {
                    if (MetadataKind == ModelMetadataKind.Type)
                    {
                        _isBindingRequired = false;
                    }
                    else
                    {
                        _isBindingRequired = BindingMetadata.IsBindingRequired;
                    }
                }

                return _isBindingRequired.Value;
            }
        }

        /// <inheritdoc />
        public override bool IsEnum
        {
            get
            {
                return DisplayMetadata.IsEnum;
            }
        }

        /// <inheritdoc />
        public override bool IsFlagsEnum
        {
            get
            {
                return DisplayMetadata.IsFlagsEnum;
            }
        }

        /// <inheritdoc />
        public override bool IsReadOnly
        {
            get
            {
                if (!_isReadOnly.HasValue)
                {
                    if (MetadataKind == ModelMetadataKind.Type)
                    {
                        _isReadOnly = false;
                    }
                    else if (BindingMetadata.IsReadOnly.HasValue)
                    {
                        _isReadOnly = BindingMetadata.IsReadOnly;
                    }
                    else
                    {
                        _isReadOnly = _details.PropertySetter == null;
                    }
                }

                return _isReadOnly.Value;
            }
        }

        /// <inheritdoc />
        public override bool IsRequired
        {
            get
            {
                if (!_isRequired.HasValue)
                {
                    if (ValidationMetadata.IsRequired.HasValue)
                    {
                        _isRequired = ValidationMetadata.IsRequired;
                    }
                    else
                    {
                        // Default to IsRequired = true for non-Nullable<T> value types.
                        _isRequired = !IsReferenceOrNullableType;
                    }
                }

                return _isRequired.Value;
            }
        }

        /// <inheritdoc />
        public override IModelBindingMessageProvider ModelBindingMessageProvider
        {
            get
            {
                return BindingMetadata.ModelBindingMessageProvider;
            }
        }

        /// <inheritdoc />
        public override string NullDisplayText
        {
            get
            {
                return DisplayMetadata.NullDisplayText;
            }
        }

        /// <inheritdoc />
        public override int Order
        {
            get
            {
                return DisplayMetadata.Order;
            }
        }

        /// <inheritdoc />
        public override ModelPropertyCollection Properties
        {
            get
            {
                if (_properties == null)
                {
                    var properties = _provider.GetMetadataForProperties(ModelType);
                    properties = properties.OrderBy(p => p.Order);
                    _properties = new ModelPropertyCollection(properties);
                }

                return _properties;
            }
        }

        /// <inheritdoc />
        public override IPropertyBindingPredicateProvider PropertyBindingPredicateProvider
        {
            get
            {
                return BindingMetadata.PropertyBindingPredicateProvider;
            }
        }

        /// <inheritdoc />
        public override bool ShowForDisplay
        {
            get
            {
                return DisplayMetadata.ShowForDisplay;
            }
        }

        /// <inheritdoc />
        public override bool ShowForEdit
        {
            get
            {
                return DisplayMetadata.ShowForEdit;
            }
        }

        /// <inheritdoc />
        public override string SimpleDisplayProperty
        {
            get
            {
                return DisplayMetadata.SimpleDisplayProperty;
            }
        }

        /// <inheritdoc />
        public override string TemplateHint
        {
            get
            {
                return DisplayMetadata.TemplateHint;
            }
        }

        /// <inheritdoc />
        public override bool ValidateChildren
        {
            get
            {
                if (!_validateChildren.HasValue)
                {
                    if (ValidationMetadata.ValidateChildren.HasValue)
                    {
                        _validateChildren = ValidationMetadata.ValidateChildren.Value;
                    }
                    else if (IsComplexType || IsEnumerableType)
                    {
                        _validateChildren = true;
                    }
                    else
                    {
                        _validateChildren = false;
                    }
                }

                return _validateChildren.Value;
            }
        }

        /// <inheritdoc />
        public override IReadOnlyList<object> ValidatorMetadata
        {
            get
            {
                if (_validatorMetadata == null)
                {
                    _validatorMetadata = new ReadOnlyCollection<object>(ValidationMetadata.ValidatorMetadata);
                }

                return _validatorMetadata;
            }
        }

        /// <inheritdoc />
        public override Func<object, object> PropertyGetter
        {
            get
            {
                return _details.PropertyGetter;
            }
        }

        /// <inheritdoc />
        public override Action<object, object> PropertySetter
        {
            get
            {
                return _details.PropertySetter;
            }
        }
    }
}