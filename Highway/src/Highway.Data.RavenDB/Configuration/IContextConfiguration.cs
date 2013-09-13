using Raven.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Highway.Data
{
    public interface IContextConfiguration : IContextConfiguration<IDocumentSession>
    {
    }
}
