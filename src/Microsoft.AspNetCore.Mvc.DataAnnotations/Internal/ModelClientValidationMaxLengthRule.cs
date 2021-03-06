// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Microsoft.AspNetCore.Mvc.DataAnnotations.Internal
{
    public class ModelClientValidationMaxLengthRule : ModelClientValidationRule
    {
        private const string MaxLengthValidationType = "maxlength";
        private const string MaxLengthValidationParameter = "max";

        public ModelClientValidationMaxLengthRule(string errorMessage, int maximumLength)
            : base(MaxLengthValidationType, errorMessage)
        {
            ValidationParameters[MaxLengthValidationParameter] = maximumLength;
        }
    }
}
