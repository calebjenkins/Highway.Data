using System.Data.Entity;

namespace Highway.Data
{
    public interface IContextConfiguration : IContextConfiguration<DbContext>
    {
    }
}