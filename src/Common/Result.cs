namespace Common
{
    public record struct Result(in Error Error = null)
    {
        public readonly bool Success => Error == null;

        public static implicit operator Result(in string code) =>
            code is not null ? new(code) : new();

        public static implicit operator Result((string code, string description) cd) =>
            new(cd);

        public static implicit operator Error(in Result r) =>
            r.Error;

        public static implicit operator Boolean(in Result r) =>
            r.Success;
    }

    public sealed record class Error(string Code,
                                     string Description = null)
    {
        public static implicit operator Error(in string code) =>
            code is not null ? new(code) : throw new ArgumentNullException(nameof(code));

        public static implicit operator Error((string code, string description) cd) =>
            cd.code is not null ? new(cd.code, cd.description) : throw new ArgumentNullException(nameof(cd.code));
    }

    public sealed class ResultException(string code, string description) : Exception
    {
        public readonly string Code = code;
        public readonly string Description = description;
    }

    public record struct Result<T>(T Value, Error Error = null)
    {
        public readonly bool Success => Error == null;

        public static implicit operator Result<T>(T val) =>
            new(val);

        public static implicit operator T(Result<T> r) =>
            r.Success ? r.Value : throw new ResultException(r.Error.Code,
                                                            r.Error.Description);

        public static implicit operator Result<T>(in string code) =>
            new(default, code);

        public static implicit operator Result<T>((string code, string description) cd) =>
            new(default, cd);

        public static implicit operator Result(in Result<T> r) =>
            new(r.Error);

        public static implicit operator Result<T>(in Result r) =>
            new(default, r.Error);

        public static implicit operator Error(in Result<T> r) =>
            r.Error;

        public static implicit operator bool(in Result<T> r) =>
            r.Success;
    }

    public abstract record class Validatable
    {
        public abstract Result IsValid();

        public Result IsValid(out Result r) =>
            r = IsValid();

        public static implicit operator Result(Validatable v) =>
            v is null ? null : v.IsValid();
    }

    public static class ValidatedExtension
    {
        public static Result IsValid(this IEnumerable<Validatable> v, out Result r, bool required = true) =>
            r = v.IsValid(required);

        public static Result IsValid(this IEnumerable<Validatable> v, bool required = true) =>
            required && !v.Any() ?
                ErrorCodes.INVALID_COLLECTION :
                v.FirstOrDefault(x => !x.IsValid());
    }
}