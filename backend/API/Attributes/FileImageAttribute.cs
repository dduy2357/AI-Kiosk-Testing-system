using System.ComponentModel.DataAnnotations;

namespace API.Attributes
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_extensions.Contains(ext))
                {
                    return new ValidationResult($"File extension {ext} is not allowed!");
                }
            }

            return ValidationResult.Success;
        }
    }

    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly long _maxSize;

        public MaxFileSizeAttribute(long maxSize)
        {
            _maxSize = maxSize;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                if (file.Length > _maxSize)
                {
                    return new ValidationResult($"Maximum allowed file size is {_maxSize / (1024 * 1024)} MB.");
                }
            }
            return ValidationResult.Success;
        }
    }

}
