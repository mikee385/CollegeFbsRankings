using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollegeFbsRankings.Domain.Repositories
{
    public interface IQuery<out T>
    {
        T Execute();
    }
}
