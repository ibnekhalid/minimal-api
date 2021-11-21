namespace Common.MinimalValidator;

public interface IMinimalValidator
{
    ValidationResult Validate<T>(T model);
}

