using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TPSyntheseRT
{
    class Box
    {
        Vector3 start;
        Vector3 end;

        public Box(Vector3 _start, Vector3 _end)
        {
            this.start = _start;
            this.end = _end;
        }

    }
}
