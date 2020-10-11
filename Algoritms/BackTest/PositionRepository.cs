using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Algoritms.BackTest
{
    public class PositionRepository
    {
        readonly List<Position> positions;
        public PositionRepository()
        {
            positions = new List<Position>();
        }

        public IEnumerable<Position> Get()
        {
            return positions;
        }

        public IEnumerable<Position> GetOpen()
        {
            return positions.Where(x => x.IsClose == false);
        }

        public Position Create(Position item)
        {
            positions.Add(item);
            return item;
        }
    }
}
