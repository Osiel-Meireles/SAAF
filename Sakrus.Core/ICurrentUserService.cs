namespace Sakrus.Core;

public interface ICurrentUserService
{
    int? UserId { get; }
    string UserName { get; }
}
