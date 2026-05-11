namespace Accounts.PracticeOperations.Application.Abstractions;

public interface IPasswordHasher
{
    string Hash(string plaintextPassword);
    bool Verify(string hash, string plaintextPassword);
}
