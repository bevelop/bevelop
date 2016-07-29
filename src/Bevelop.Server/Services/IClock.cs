using System;

namespace Bevelop.Server.Services
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}