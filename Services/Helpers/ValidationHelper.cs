using System.ComponentModel.DataAnnotations;

namespace Services.Helpers;

public class ValidationHelper
{
    public static void ModelValidation(object o)
    {
        //Model validations
        ValidationContext validationContext = new ValidationContext(o);
        List<ValidationResult> validationResults = new();
        bool isValid = Validator.TryValidateObject(o, validationContext, validationResults, true);
        if (!isValid) throw new ArgumentException(validationResults[0].ErrorMessage, nameof(o));
    }
}