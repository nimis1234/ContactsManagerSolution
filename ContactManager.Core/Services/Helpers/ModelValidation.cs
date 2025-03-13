using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Helpers
{
    public class ModelValidation // used for model validation
    {
        public static bool ValidateModel<T>(T model, out List<ValidationResult> validationResults) // out reference parameter
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            validationResults = new List<ValidationResult>(); // Initialize the out parameter

            ValidationContext validationContext = new ValidationContext(model); // validation context supplies info about different data annotations check used in model, it also
                                                                                // capture validation errors
            /* The ValidationContext is a class in .NET used to provide context about the object being validated. It works with the Validator class to validate objects or properties using data annotations. The ValidationContext contains metadata about the object, 
             * such as the object instance, its properties, and any additional information required for validation.*/
            /* 
             * Fully validate an object to catch all possible validation issues (validateAllProperties = true).
               Partially validate an object, focusing only on the presence of required values (validateAllProperties = false).
             * 
             * */

            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return Validator.TryValidateObject(model, validationContext, validationResults, validateAllProperties: true);
        }
    }
}
