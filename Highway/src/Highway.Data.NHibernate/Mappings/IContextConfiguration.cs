using NHibernate;

namespace Highway.Data
{
    public interface IContextConfiguration : IContextConfiguration<ISession>
    {
    }
}